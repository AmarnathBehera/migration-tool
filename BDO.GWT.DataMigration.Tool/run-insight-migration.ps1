<#
.SYNOPSIS
    One-command reproduction of the "Insight" migration flow plus the custom-module -> page-types
    migration for BDO Metadata and BDO Deal Cards.

.DESCRIPTION
    Runs, in dependency order (Insight is intentionally LAST because it references the other pages):
        1. Create/update target website channels and cultures, then build the CLI.
        2. Migrate source sites, then seed BDO Metadata and Deal Cards pages.
        3. Migrate referenced page types and pages ("Section Services onwards").
        4. (LAST) Migrate Insight page types and pages, then create the
         CopyInsight content-hub items from K13 CTA + Content sharing and reference them from
         Insight.InsightContent (seed-copyinsight-contenthub.sql).

    Each `migrate --pages` run temporarily scopes appsettings IncludeCodeNames and restores it after.
    All SQL seeds are idempotent (delete + rebuild their subtree), so the whole script is re-runnable.

.EXAMPLE
    ./run-insight-migration.ps1
    ./run-insight-migration.ps1 -SkipModules       # only referenced pages + Insight + CopyInsight
    ./run-insight-migration.ps1 -SkipBuild         # reuse existing build
    ./run-insight-migration.ps1 -ReferencesOnly    # repair Insight selectors on existing pages
    ./run-insight-migration.ps1 -ReferencesOnly -RestoreReferenceTargets
    ./run-insight-migration.ps1 -ReferencesOnly -IncludeInsightContent
    ./run-insight-migration.ps1 -PruneOnly -PreviewPrune
    ./run-insight-migration.ps1 -PruneOnly
#>
[CmdletBinding()]
param(
    [string]$Server,
    [string]$SourceDb,
    [string]$TargetDb,
    [string]$DbUser,
    [string]$DbPassword,
    [string]$SourceSiteName,
    [string]$TargetChannelName,
    [string[]]$TargetCultures = @(),
    [string]$DefaultCulture,
    [switch]$SkipModules,        # skip metadata + deal cards module->pages seeds
    [switch]$SkipBuild,          # skip building the CLI
    [switch]$SkipReferencedPages,# skip migrating the Insight-referenced page types
    [switch]$SkipInsight,        # skip the final Insight page migration + CopyInsight seed
    [switch]$ReferencesOnly,
    [switch]$RestoreReferenceTargets,
    [string[]]$ReferenceTargetClasses = @(),
    [switch]$IncludeInsightContent,
    [switch]$PruneUnrelatedPages,
    [switch]$PruneOnly,
    [switch]$PreviewPrune
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root
$appsettingsPath = Join-Path $root 'Migration.Tool.CLI/appsettings.json'
$settings = Get-Content $appsettingsPath -Raw | ConvertFrom-Json
$sourceConnection = [System.Data.SqlClient.SqlConnectionStringBuilder]::new($settings.Settings.KxConnectionString)
$targetConnection = [System.Data.SqlClient.SqlConnectionStringBuilder]::new($settings.Settings.XbyKApiSettings.ConnectionStrings.CMSConnectionString)
$channelMappings = $settings.Settings.TargetWebsiteChannelMappings
$cultureMappings = $settings.Settings.CultureCodeMappings

if (-not $Server) { $Server = $sourceConnection.DataSource }
if (-not $SourceDb) { $SourceDb = $sourceConnection.InitialCatalog }
if (-not $TargetDb) { $TargetDb = $targetConnection.InitialCatalog }
if (-not $DbUser) { $DbUser = $targetConnection.UserID }
if (-not $DbPassword) { $DbPassword = $targetConnection.Password }
if (-not $SourceSiteName) {
    $siteNames = @($channelMappings.PSObject.Properties.Name)
    if ($siteNames.Count -ne 1) { throw 'Specify -SourceSiteName when appsettings contains more than one target channel mapping.' }
    $SourceSiteName = $siteNames[0]
}
if (-not $TargetChannelName) { $TargetChannelName = $channelMappings.$SourceSiteName }
if (-not $TargetChannelName) { throw "No target channel mapping found for source site '$SourceSiteName'." }
if ($TargetCultures.Count -eq 0) {
    $TargetCultures = @($cultureMappings.PSObject.Properties.Value | Select-Object -Unique)
}
if (-not $DefaultCulture) { $DefaultCulture = $TargetCultures[0] }

$sourceCulture = ($cultureMappings.PSObject.Properties |
    Where-Object { $_.Value -eq $DefaultCulture } |
    Sort-Object { if ($_.Name -eq $DefaultCulture) { 0 } else { 1 } } |
    Select-Object -First 1).Name
if (-not $sourceCulture) { throw "No source culture maps to the default target culture '$DefaultCulture'." }

$sourceSiteIdText = sqlcmd -S $Server -d $SourceDb -U $DbUser -P $DbPassword -N -C -W -l 60 -h -1 -Q "SELECT TOP 1 SiteID FROM CMS_Site WHERE SiteName = '$SourceSiteName';"
if ($LASTEXITCODE -ne 0) { throw "Failed to resolve source site '$SourceSiteName' in '$SourceDb'." }
$sourceSiteId = ($sourceSiteIdText -split '\s+' | Where-Object { $_ -match '^\d+$' } | Select-Object -First 1)
if (-not $sourceSiteId) { throw "Source site '$SourceSiteName' was not found in '$SourceDb'." }
$sourceSiteGuid = sqlcmd -S $Server -d $SourceDb -U $DbUser -P $DbPassword -N -C -W -l 60 -h -1 -Q "SELECT CONVERT(nvarchar(36), SiteGUID) FROM CMS_Site WHERE SiteName = '$SourceSiteName';"
if ($LASTEXITCODE -ne 0) { throw "Failed to resolve source site GUID for '$SourceSiteName'." }
$sourceSiteGuid = ($sourceSiteGuid -split '\s+' | Where-Object { $_ -match '^[0-9a-fA-F-]{36}$' } | Select-Object -First 1)
if (-not $sourceSiteGuid) { throw "Source site GUID for '$SourceSiteName' could not be resolved." }

# Page types that Insights reference (migrated BEFORE Insights) - "Section Services onwards".
$referencedClasses = @(
    'BDO.SectionServices', 'BDO.BusinessLine', 'BDO.ServiceArea', 'BDO.Service', 'BDO.SubService',
    'BDO.SectionIndustries', 'BDO.IndustryCategory', 'BDO.Industry', 'BDO.IndustryService',
    'BDO.SectionSpecialties', 'BDO.SpecialtiesCategory', 'BDO.SpecialtiesArea', 'BDO.SpecialtiesPage',
    'BDO.Locations', 'BDO.LocationCity', 'BDO.LocationOffice',
    'BDO.SectionPeoplePage', 'BDO.ExternalPeopleFolder', 'BDO.InternalPeopleFolder',
    'BDO.ExternalPerson', 'BDO.InternalPerson', 'BDO.Person'
    'BDO.SectionDeals', 'BDO.RemoteDeal', 'BDO.Deal',
    'BDO.SectionMicrosites', 'BDO.MicrositeMasterPage', 'BDO.MicrositePage'
)
$insightPageTypeClasses = @('BDO.Metadata', 'BDO.ContentPage', 'BDO.ContentSectionPage', 'BDO.SectionInsightsPage', 'BDO.InsightCategory', 'BDO.Insight')
$insightClasses = @('BDO.SectionInsightsPage', 'BDO.InsightCategory', 'BDO.Insight')

function Write-Step($n, $msg) { Write-Host "`n==== STEP $n : $msg ====" -ForegroundColor Cyan }

function Remove-UnrelatedSwitzerlandPages {
    if ($SourceSiteName -ne 'Switzerland' -or $TargetChannelName -ne 'Switzerland') {
        throw 'Insight page cleanup is restricted to Switzerland.'
    }
    $sql = Get-Content (Join-Path $root 'prune-switzerland-insight-pages.sql') -Raw
    $sql = $sql.Replace('[bdo-db-gwt-tst2-eur].dbo.', "[$($SourceDb.Replace(']', ']]'))].dbo.")
    if (-not $PreviewPrune) { $sql = $sql.Replace('DECLARE @apply bit = 0;', 'DECLARE @apply bit = 1;') }
    $connection = [System.Data.SqlClient.SqlConnection]::new($targetConnection.ConnectionString)
    if ($TargetDb -ne $targetConnection.InitialCatalog) { throw 'Cleanup target must match the configured target database.' }
    try {
        $connection.Open()
        $command = $connection.CreateCommand()
        if (-not $PreviewPrune) {
            $command.CommandText = @'
DECLARE @channelID int = (SELECT wc.WebsiteChannelID FROM CMS_WebsiteChannel wc JOIN CMS_Channel ch ON ch.ChannelID=wc.WebsiteChannelChannelID WHERE ch.ChannelName='Switzerland');
SELECT * FROM CMS_WebPageItem WHERE WebPageItemWebsiteChannelID=@channelID;
SELECT * FROM CMS_WebPageUrlPath WHERE WebPageUrlPathWebPageItemID IN (SELECT WebPageItemID FROM CMS_WebPageItem WHERE WebPageItemWebsiteChannelID=@channelID);
SELECT * FROM CMS_WebPageFormerUrlPath WHERE WebPageFormerUrlPathWebPageItemID IN (SELECT WebPageItemID FROM CMS_WebPageItem WHERE WebPageItemWebsiteChannelID=@channelID) OR WebPageFormerUrlPathSourceWebPageItemID IN (SELECT WebPageItemID FROM CMS_WebPageItem WHERE WebPageItemWebsiteChannelID=@channelID);
SELECT * FROM CMS_WebPageAclMapping WHERE WebPageAclMappingWebPageItemID IN (SELECT WebPageItemID FROM CMS_WebPageItem WHERE WebPageItemWebsiteChannelID=@channelID);
SELECT * FROM CMS_WebPageAcl WHERE WebPageAclWebPageItemID IN (SELECT WebPageItemID FROM CMS_WebPageItem WHERE WebPageItemWebsiteChannelID=@channelID);
SELECT * FROM CMS_WebPageScope WHERE WebPageScopeWebPageItemID IN (SELECT WebPageItemID FROM CMS_WebPageItem WHERE WebPageItemWebsiteChannelID=@channelID);
'@
            $backupAdapter = [System.Data.SqlClient.SqlDataAdapter]::new($command)
            $backupData = [System.Data.DataSet]::new('SwitzerlandPageTreeBackup')
            [void]$backupAdapter.Fill($backupData)
            $tableNames = @('CMS_WebPageItem', 'CMS_WebPageUrlPath', 'CMS_WebPageFormerUrlPath', 'CMS_WebPageAclMapping', 'CMS_WebPageAcl', 'CMS_WebPageScope')
            for ($tableIndex = 0; $tableIndex -lt $tableNames.Count; $tableIndex++) {
                $backupData.Tables[$tableIndex].TableName = $tableNames[$tableIndex]
            }
            $backupPath = Join-Path $root ("Migration.Tool.CLI/logs/switzerland-page-cleanup-{0}.xml" -f (Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
            [void][System.IO.Directory]::CreateDirectory((Split-Path $backupPath))
            $backupData.WriteXml($backupPath, [System.Data.XmlWriteMode]::WriteSchema)
            Write-Host "Page-tree backup: $backupPath"
        }
        $command.CommandText = $sql
        $command.CommandTimeout = 120
        $adapter = [System.Data.SqlClient.SqlDataAdapter]::new($command)
        $result = [System.Data.DataSet]::new()
        [void]$adapter.Fill($result)
        $result.Tables | ForEach-Object { $_ | Format-Table -AutoSize | Out-Host }
    } finally { $connection.Dispose() }
}

if ($PruneOnly) {
    Remove-UnrelatedSwitzerlandPages
    return
}

function Invoke-Sql([string]$db, [string]$file, [string]$sourceCultureOverride = $sourceCulture, [int]$languageIdOverride = $defaultLanguageId) {
    if (-not (Test-Path $file)) { throw "SQL file not found: $file" }
    Write-Host "  sqlcmd -> $db : $file" -ForegroundColor DarkGray
    $sql = Get-Content $file -Raw
    $sql = $sql.Replace('[BDO-DB-GWT-TST-EUR].dbo.', "[$SourceDb].dbo.")
    $sql = $sql.Replace('en-GB', $sourceCultureOverride)
    $sql = $sql.Replace('en-US', $sourceCultureOverride)
    $sql = $sql.Replace("@siteColumn)+N'=1 AND", "@siteColumn)+N'=$sourceSiteId AND")
    $sql = [regex]::Replace($sql, '(?i)(NodeSiteID\s*=\s*)1', { param($m) $m.Groups[1].Value + $sourceSiteId })
    $sql = [regex]::Replace($sql, '(?i)(@websiteChannelID\s+int\s*=\s*)10', { param($m) $m.Groups[1].Value + $websiteChannelId })
    $sql = [regex]::Replace($sql, '(?i)(@channel\s+int\s*=\s*)10', { param($m) $m.Groups[1].Value + $websiteChannelId })
    $sql = [regex]::Replace($sql, '(?i)(WebPageItemWebsiteChannelID\s*=\s*)10', { param($m) $m.Groups[1].Value + $websiteChannelId })
    $sql = [regex]::Replace($sql, '(?i)(WebsiteChannelID\s*=\s*)10', { param($m) $m.Groups[1].Value + $websiteChannelId })
    $sql = [regex]::Replace($sql, '(?i)(@languageID\s+int\s*=\s*)1\b', { param($m) $m.Groups[1].Value + $languageIdOverride })
    $sql = [regex]::Replace($sql, '(?i)(@lang\s+int\s*=\s*)1\b', { param($m) $m.Groups[1].Value + $languageIdOverride })
    $sql = [regex]::Replace($sql, '(?i)(ContentItemLanguageMetadataContentLanguageID\s*=\s*)1\b', { param($m) $m.Groups[1].Value + $languageIdOverride })

    $tempSql = Join-Path $env:TEMP ("insight-seed-{0}.sql" -f [guid]::NewGuid())
    try {
        Set-Content -Path $tempSql -Value $sql -Encoding UTF8
        sqlcmd -S $Server -d $db -U $DbUser -P $DbPassword -N -C -W -l 60 -b -i $tempSql
        if ($LASTEXITCODE -ne 0) { throw "sqlcmd failed ($file) with exit code $LASTEXITCODE" }
    } finally {
        Remove-Item $tempSql -Force -ErrorAction SilentlyContinue
    }
}

function Ensure-ChannelAndCultures {
    $cultureValues = ($TargetCultures | ForEach-Object { "(N'$($_.Replace("'", "''"))')" }) -join ','
    $channelLiteral = $TargetChannelName.Replace("'", "''")
    $defaultLiteral = $DefaultCulture.Replace("'", "''")
    $createChannelSql = @"
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @Cultures TABLE (CultureCode nvarchar(50) PRIMARY KEY);
INSERT INTO @Cultures (CultureCode) VALUES $cultureValues;
IF EXISTS (SELECT 1 FROM @Cultures x WHERE NOT EXISTS (SELECT 1 FROM CMS_Culture c WHERE c.CultureCode=x.CultureCode))
    THROW 50001, 'A requested culture is missing from CMS_Culture.', 1;
INSERT INTO CMS_ContentLanguage (ContentLanguageDisplayName,ContentLanguageName,ContentLanguageIsDefault,ContentLanguageCultureFormat,ContentLanguageGUID)
SELECT c.CultureName,c.CultureCode,0,c.CultureCode,c.CultureGUID
FROM CMS_Culture c JOIN @Cultures x ON x.CultureCode=c.CultureCode
WHERE NOT EXISTS (SELECT 1 FROM CMS_ContentLanguage l WHERE l.ContentLanguageCultureFormat=c.CultureCode);
DECLARE @PrimaryLanguageID int=(SELECT ContentLanguageID FROM CMS_ContentLanguage WHERE ContentLanguageCultureFormat=N'$defaultLiteral');
IF @PrimaryLanguageID IS NULL THROW 50002, 'The default content language could not be created.', 1;
DECLARE @SourceSiteGUID uniqueidentifier=CONVERT(uniqueidentifier,'$sourceSiteGuid');
DECLARE @ChannelID int=(SELECT ChannelID FROM CMS_Channel WHERE ChannelName=N'$channelLiteral');
IF @ChannelID IS NULL
BEGIN
    INSERT INTO CMS_Channel (ChannelDisplayName,ChannelName,ChannelType,ChannelGUID)
    VALUES (N'$channelLiteral',N'$channelLiteral','Website',@SourceSiteGUID);
    SET @ChannelID=SCOPE_IDENTITY();
END;
IF EXISTS (SELECT 1 FROM CMS_Channel WHERE ChannelGUID=@SourceSiteGUID AND ChannelID<>@ChannelID)
    THROW 50003, 'Source site GUID is already assigned to another channel.', 1;
IF EXISTS (SELECT 1 FROM CMS_WebsiteChannel WHERE WebsiteChannelGUID=@SourceSiteGUID AND WebsiteChannelChannelID<>@ChannelID)
    THROW 50004, 'Source site GUID is already assigned to another website channel.', 1;
UPDATE CMS_Channel SET ChannelGUID=@SourceSiteGUID WHERE ChannelID=@ChannelID;
IF NOT EXISTS (SELECT 1 FROM CMS_WebsiteChannel WHERE WebsiteChannelChannelID=@ChannelID)
BEGIN
    DECLARE @Port int=12400+@ChannelID;
    WHILE EXISTS (SELECT 1 FROM CMS_WebsiteChannel WHERE WebsiteChannelDomain=N'localhost:'+CONVERT(nvarchar(10),@Port)) SET @Port+=1;
    INSERT INTO CMS_WebsiteChannel (WebsiteChannelGUID,WebsiteChannelDomain,WebsiteChannelChannelID,WebsiteChannelHomePage,WebsiteChannelPrimaryContentLanguageID,WebsiteChannelStoreFormerUrls)
    VALUES (@SourceSiteGUID,N'localhost:'+CONVERT(nvarchar(10),@Port),@ChannelID,N'/',@PrimaryLanguageID,1);
END
ELSE
    UPDATE CMS_WebsiteChannel SET WebsiteChannelGUID=@SourceSiteGUID,WebsiteChannelPrimaryContentLanguageID=@PrimaryLanguageID WHERE WebsiteChannelChannelID=@ChannelID;
DECLARE @WebsiteChannelID int=(SELECT WebsiteChannelID FROM CMS_WebsiteChannel WHERE WebsiteChannelChannelID=@ChannelID);
IF NOT EXISTS (SELECT 1 FROM CMS_WebPageAcl WHERE WebPageAclWebsiteChannelID=@WebsiteChannelID AND WebPageAclWebPageItemID IS NULL)
    INSERT INTO CMS_WebPageAcl (WebPageAclGUID,WebPageAclWebPageItemID,WebPageAclWebsiteChannelID) VALUES (NEWID(),NULL,@WebsiteChannelID);
COMMIT TRANSACTION;
SELECT @ChannelID AS ChannelID, @WebsiteChannelID AS WebsiteChannelID,
       @PrimaryLanguageID AS PrimaryContentLanguageID;
"@
    sqlcmd -S $Server -d $TargetDb -U $DbUser -P $DbPassword -N -C -W -l 60 -b -Q $createChannelSql
    if ($LASTEXITCODE -ne 0) { throw "Failed to create channel '$TargetChannelName' and its content languages." }

    $script:defaultLanguageId = sqlcmd -S $Server -d $TargetDb -U $DbUser -P $DbPassword -N -C -W -l 60 -h -1 -Q "SELECT ContentLanguageID FROM CMS_ContentLanguage WHERE ContentLanguageCultureFormat='$defaultLiteral';"
    if ($LASTEXITCODE -ne 0) { throw "Failed to resolve target language '$DefaultCulture'." }
    $script:defaultLanguageId = ($script:defaultLanguageId -split '\s+' | Where-Object { $_ -match '^\d+$' } | Select-Object -First 1)
    $script:websiteChannelId = sqlcmd -S $Server -d $TargetDb -U $DbUser -P $DbPassword -N -C -W -l 60 -h -1 -Q "SELECT WebsiteChannelID FROM CMS_WebsiteChannel wc JOIN CMS_Channel c ON c.ChannelID=wc.WebsiteChannelChannelID WHERE c.ChannelName='$channelLiteral';"
    if ($LASTEXITCODE -ne 0) { throw "Failed to resolve website channel ID for '$TargetChannelName'." }
    $script:websiteChannelId = ($script:websiteChannelId -split '\s+' | Where-Object { $_ -match '^\d+$' } | Select-Object -First 1)
}

function Ensure-PageAclMappings {
    $sql = @"
SET NOCOUNT ON;
DECLARE @WebsiteChannelID int=$websiteChannelId;
INSERT INTO CMS_WebPageAcl (WebPageAclGUID,WebPageAclWebPageItemID,WebPageAclWebsiteChannelID)
SELECT NEWID(),w.WebPageItemID,@WebsiteChannelID
FROM CMS_WebPageItem w
WHERE w.WebPageItemWebsiteChannelID=@WebsiteChannelID
  AND NOT EXISTS (SELECT 1 FROM CMS_WebPageAcl a WHERE a.WebPageAclWebPageItemID=w.WebPageItemID AND a.WebPageAclWebsiteChannelID=@WebsiteChannelID);
SELECT COUNT(*) AS PageAclMappings FROM CMS_WebPageAcl WHERE WebPageAclWebsiteChannelID=@WebsiteChannelID;
"@
    sqlcmd -S $Server -d $TargetDb -U $DbUser -P $DbPassword -N -C -W -l 60 -b -Q $sql
    if ($LASTEXITCODE -ne 0) { throw "Failed to ensure page ACL mappings for '$TargetChannelName'." }
}

function Invoke-ScopedSiteMigration {
    $sourceSiteNames = @(sqlcmd -S $Server -d $SourceDb -U $DbUser -P $DbPassword -N -C -W -l 60 -h -1 -Q 'SET NOCOUNT ON; SELECT SiteName FROM CMS_Site;' |
        ForEach-Object { "$($_)".Trim() } | Where-Object { $_ })
    if ($LASTEXITCODE -ne 0) { throw "Failed to list source sites in '$SourceDb'." }
    if ($SourceSiteName -notin $sourceSiteNames) { throw "Source site '$SourceSiteName' was not found in '$SourceDb'." }

    $backup = "$appsettingsPath.bak"
    Copy-Item $appsettingsPath $backup -Force
    try {
        $scopedSettings = Get-Content $appsettingsPath -Raw | ConvertFrom-Json
        $siteConfiguration = $scopedSettings.Settings.EntityConfigurations.CMS_Site
        $otherSiteNames = @($sourceSiteNames | Where-Object { $_ -ne $SourceSiteName })
        $siteConfiguration.ExcludeCodeNames = @(@($siteConfiguration.ExcludeCodeNames) + $otherSiteNames | Select-Object -Unique)
        $scopedSettings | ConvertTo-Json -Depth 100 | Set-Content -Path $appsettingsPath -Encoding UTF8

        Push-Location (Join-Path $root 'Migration.Tool.CLI')
        try {
            [void][System.IO.Directory]::CreateDirectory((Join-Path $root 'Migration.Tool.CLI/logs'))
            $consoleLog = Join-Path $root ("Migration.Tool.CLI/logs/migration-run-sites-{0}.log" -f (Get-Date -Format 'yyyyMMdd-HHmmss'))
            dotnet run --project Migration.Tool.CLI.csproj --configuration Release --no-build -- migrate --nowait --sites --bypass-dependency-check 2>&1 | Tee-Object -FilePath $consoleLog
            if ($LASTEXITCODE -ne 0) { throw "migrate --sites failed with exit code $LASTEXITCODE" }
        } finally { Pop-Location }
    } finally {
        Copy-Item $backup $appsettingsPath -Force
        Remove-Item $backup -Force -ErrorAction SilentlyContinue
        Write-Host '  appsettings.json restored.' -ForegroundColor DarkGray
    }
}

# Runs `migrate --pages` scoped to the given classes, restoring appsettings afterwards.
function Invoke-ScopedMigration([string]$command, [string[]]$classes) {
    $appsettings = Join-Path $root 'Migration.Tool.CLI/appsettings.json'
    $backup      = "$appsettings.bak"
    Copy-Item $appsettings $backup -Force
    try {
        $sourceSiteNames = @(sqlcmd -S $Server -d $SourceDb -U $DbUser -P $DbPassword -N -C -W -l 60 -h -1 -Q 'SET NOCOUNT ON; SELECT SiteName FROM CMS_Site;' |
            ForEach-Object { "$($_)".Trim() } | Where-Object { $_ })
        if ($LASTEXITCODE -ne 0) { throw "Failed to list source sites in '$SourceDb'." }

        $scopedSettings = Get-Content $appsettings -Raw | ConvertFrom-Json
        $otherSiteNames = @($sourceSiteNames | Where-Object { $_ -ne $SourceSiteName })
        $scopedSettings.Settings.EntityConfigurations.CMS_Site.ExcludeCodeNames = @(
            @($scopedSettings.Settings.EntityConfigurations.CMS_Site.ExcludeCodeNames) + $otherSiteNames | Select-Object -Unique
        )
        $scopedSettings.Settings.EntityConfigurations.CMS_Class.IncludeCodeNames = $classes
        $scopedSettings | ConvertTo-Json -Depth 100 | Set-Content -Path $appsettings -Encoding UTF8

        Push-Location (Join-Path $root 'Migration.Tool.CLI')
        try {
            [void][System.IO.Directory]::CreateDirectory((Join-Path $root 'Migration.Tool.CLI/logs'))
            $consoleLog = Join-Path $root ("Migration.Tool.CLI/logs/migration-run-{0}-{1}.log" -f $command, (Get-Date -Format 'yyyyMMdd-HHmmss'))
            dotnet run --project Migration.Tool.CLI.csproj --configuration Release --no-build -- migrate --nowait "--$command" --bypass-dependency-check 2>&1 | Tee-Object -FilePath $consoleLog
            if ($LASTEXITCODE -ne 0) { throw "migrate --$command failed with exit code $LASTEXITCODE" }
            if (Select-String -Path $consoleLog -Pattern 'Failed to import:|Failed to migrate page type' -Quiet) {
                throw "migrate --$command logged import failures. See $consoleLog"
            }
        } finally { Pop-Location }
    } finally {
        Copy-Item $backup $appsettings -Force
        Remove-Item $backup -Force -ErrorAction SilentlyContinue
        Write-Host '  appsettings.json restored.' -ForegroundColor DarkGray
    }
}

if ($RestoreReferenceTargets -and -not $ReferencesOnly) {
    throw '-RestoreReferenceTargets requires -ReferencesOnly.'
}
if ($ReferenceTargetClasses.Count -gt 0 -and -not $RestoreReferenceTargets) {
    throw '-ReferenceTargetClasses requires -RestoreReferenceTargets.'
}
if (@($ReferenceTargetClasses | Where-Object { $_ -notin $referencedClasses }).Count -gt 0) {
    throw '-ReferenceTargetClasses must contain only configured Insight reference page types.'
}
if ($IncludeInsightContent -and -not $ReferencesOnly) {
    throw '-IncludeInsightContent requires -ReferencesOnly.'
}
if ($ReferencesOnly) {
    if ($PruneUnrelatedPages -or $PruneOnly -or $SkipInsight) {
        throw '-ReferencesOnly cannot be combined with pruning or -SkipInsight.'
    }
    $channelLiteral = $TargetChannelName.Replace("'", "''")
    $channelText = sqlcmd -S $Server -d $TargetDb -U $DbUser -P $DbPassword -N -C -W -l 60 -b -h -1 -Q "SET NOCOUNT ON; SELECT wc.WebsiteChannelID FROM CMS_WebsiteChannel wc JOIN CMS_Channel ch ON ch.ChannelID=wc.WebsiteChannelChannelID WHERE ch.ChannelName=N'$channelLiteral';"
    if ($LASTEXITCODE -ne 0) { throw 'Failed to resolve the existing target channel.' }
    $script:websiteChannelId = ($channelText -split '\s+' | Where-Object { $_ -match '^\d+$' } | Select-Object -First 1)
    if (-not $websiteChannelId) { throw "Target channel '$TargetChannelName' was not found." }
    if ($RestoreReferenceTargets) {
        $restoreClasses = @(
            'BDO.SectionServices', 'BDO.BusinessLine', 'BDO.ServiceArea', 'BDO.Service', 'BDO.SubService',
            'BDO.SectionIndustries', 'BDO.IndustryCategory', 'BDO.Industry', 'BDO.IndustryService',
            'BDO.SectionSpecialties', 'BDO.SpecialtiesCategory', 'BDO.SpecialtiesArea', 'BDO.SpecialtiesPage',
            'BDO.Locations', 'BDO.LocationCity', 'BDO.LocationOffice',
            'BDO.SectionPeoplePage', 'BDO.ExternalPeopleFolder', 'BDO.InternalPeopleFolder',
            'BDO.ExternalPerson', 'BDO.InternalPerson', 'BDO.Person'
        )
        if ($ReferenceTargetClasses.Count -gt 0) { $restoreClasses = $ReferenceTargetClasses }
        Write-Step 4 'Restore Insight reference targets'
        Invoke-ScopedMigration 'pages' $restoreClasses
    }
    foreach ($culture in $TargetCultures) {
        $cultureLiteral = $culture.Replace("'", "''")
        $languageText = sqlcmd -S $Server -d $TargetDb -U $DbUser -P $DbPassword -N -C -W -l 60 -b -h -1 -Q "SET NOCOUNT ON; SELECT ContentLanguageID FROM CMS_ContentLanguage WHERE ContentLanguageCultureFormat=N'$cultureLiteral';"
        if ($LASTEXITCODE -ne 0) { throw "Failed to resolve language '$culture'." }
        $languageId = ($languageText -split '\s+' | Where-Object { $_ -match '^\d+$' } | Select-Object -First 1)
        if (-not $languageId) { throw "Target language '$culture' was not found." }
        $seedCulture = ($cultureMappings.PSObject.Properties | Where-Object { $_.Value -eq $culture } |
            Sort-Object { if ($_.Name -eq $culture) { 0 } else { 1 } } | Select-Object -First 1).Name
        if (-not $seedCulture) { throw "No source culture maps to '$culture'." }
        if ($RestoreReferenceTargets -and $ReferenceTargetClasses.Count -eq 0 -and $culture -eq $DefaultCulture) {
            Write-Step 3 "Rebuild metadata reference targets ($culture)"
            Invoke-Sql $TargetDb (Join-Path $root 'seed-bdo-metadata-pages.sql') $seedCulture ([int]$languageId)
        }
        if ($IncludeInsightContent) {
            Write-Step 5 "Repair CopyInsight content ($culture)"
            Invoke-Sql $TargetDb (Join-Path $root 'seed-copyinsight-contenthub.sql') $seedCulture ([int]$languageId)
        }
        Write-Step 5 "Repair Insight page references ($culture)"
        Invoke-Sql $TargetDb (Join-Path $root 'seed-insight-references.sql') $seedCulture ([int]$languageId)
    }
    Write-Host 'Insight reference repair completed.' -ForegroundColor Green
    return
}

Write-Step 0 'Ensure Switzerland target channel and cultures'
Ensure-ChannelAndCultures

# ---- 1. Build the CLI ----
if (-not $SkipBuild) {
    Write-Step 1 'Build Migration Tool CLI'
    dotnet build Migration.Tool.CLI/Migration.Tool.CLI.csproj --configuration Release --no-restore
    if ($LASTEXITCODE -ne 0) { throw "Build failed with exit code $LASTEXITCODE" }
} else {
    Write-Host 'Skipping build (-SkipBuild).' -ForegroundColor Yellow
}

# ---- 2. Migrate sites so source site cultures/channels are synchronized ----
Write-Step 2 'Migrate source sites and synchronize cultures'
Invoke-ScopedSiteMigration

# ---- 3. Custom module -> pages (referenced by Insights) ----
if (-not $SkipModules) {
    Write-Step 3 'BDO Metadata custom module -> XbyK page tree'
    Invoke-Sql $TargetDb (Join-Path $root 'seed-bdo-metadata-pages.sql')

    Write-Step 3 'BDO Deal Cards custom module -> XbyK page tree'
    Invoke-Sql $TargetDb (Join-Path $root 'seed-bdo-dealcards-pages.sql')
} else {
    Write-Host 'Skipping module->pages seeds (-SkipModules).' -ForegroundColor Yellow
}
Write-Step 3 'Ensure Switzerland page ACL mappings'
Ensure-PageAclMappings

# ---- 4. Migrate the Insight-referenced page types and pages ----
if (-not $SkipReferencedPages) {
    Write-Step 4 'Migrate Insight-referenced page types'
    Invoke-ScopedMigration 'page-types' @($referencedClasses + $insightPageTypeClasses | Select-Object -Unique)

    Write-Step 4 'Migrate Insight-referenced pages (Section Services / Industries / Specialties / Locations)'
    Invoke-ScopedMigration 'pages' $referencedClasses
} else {
    Write-Host 'Skipping referenced-pages migration (-SkipReferencedPages).' -ForegroundColor Yellow
}

# ---- 5. (LAST) Insight page types/pages + CopyInsight content hub ----
if (-not $SkipInsight) {
    Write-Step 5 'Migrate Insight page types'
    Invoke-ScopedMigration 'page-types' $insightPageTypeClasses

    Write-Step 5 'Migrate Insight pages (BDO.SectionInsightsPage + BDO.Insight)'
    Invoke-ScopedMigration 'pages' $insightClasses

    foreach ($culture in $TargetCultures) {
        $cultureLiteral = $culture.Replace("'", "''")
        $languageText = sqlcmd -S $Server -d $TargetDb -U $DbUser -P $DbPassword -N -C -W -l 60 -h -1 -Q "SET NOCOUNT ON; SELECT ContentLanguageID FROM CMS_ContentLanguage WHERE ContentLanguageCultureFormat='$cultureLiteral';"
        if ($LASTEXITCODE -ne 0) { throw "Failed to resolve language '$culture'." }
        $languageId = ($languageText -split '\s+' | Where-Object { $_ -match '^\d+$' } | Select-Object -First 1)
        if (-not $languageId) { throw "Target language '$culture' was not found." }
        $seedCulture = ($cultureMappings.PSObject.Properties | Where-Object { $_.Value -eq $culture } |
            Sort-Object { if ($_.Name -eq $culture) { 0 } else { 1 } } | Select-Object -First 1).Name
        Write-Step 5 "CopyInsight content and references ($culture)"
        Invoke-Sql $TargetDb (Join-Path $root 'seed-copyinsight-contenthub.sql') $seedCulture ([int]$languageId)
        Invoke-Sql $TargetDb (Join-Path $root 'seed-insight-references.sql') $seedCulture ([int]$languageId)
        Invoke-Sql $TargetDb (Join-Path $root 'seed-remote-deal-references.sql') $seedCulture ([int]$languageId)
    }
} else {
    Write-Host 'Skipping Insight migration (-SkipInsight).' -ForegroundColor Yellow
}

if ($PruneUnrelatedPages) {
    Write-Step 6 'Remove Switzerland pages unrelated to Insights'
    Remove-UnrelatedSwitzerlandPages
}

if (-not $SkipInsight) {
    $verificationConnection = [System.Data.SqlClient.SqlConnection]::new($targetConnection.ConnectionString)
    try {
        $verificationConnection.Open()
        $verificationCommand = $verificationConnection.CreateCommand()
        $sourceDatabaseIdentifier = '[' + $SourceDb.Replace(']', ']]') + ']'
        $verificationCommand.CommandText = @"
SELECT (SELECT COUNT(DISTINCT NodeID) FROM $sourceDatabaseIdentifier.dbo.View_CMS_Tree_Joined WHERE NodeSiteID=@siteID AND ClassName='BDO.Insight') ExpectedPages,
       (SELECT COUNT(*) FROM CMS_WebPageItem page JOIN CMS_ContentItem item ON item.ContentItemID=page.WebPageItemContentItemID JOIN CMS_Class cl ON cl.ClassID=item.ContentItemContentTypeID WHERE page.WebPageItemWebsiteChannelID=@channelID AND cl.ClassName='BDO.Insight') ActualPages;
"@
        [void]$verificationCommand.Parameters.AddWithValue('@siteID', [int]$sourceSiteId)
        [void]$verificationCommand.Parameters.AddWithValue('@channelID', [int]$websiteChannelId)
        $verificationResult = [System.Data.DataTable]::new()
        $verificationResult.Load($verificationCommand.ExecuteReader())
        $verificationResult | Format-Table -AutoSize | Out-Host
        if ($verificationResult.Rows[0].ExpectedPages -ne $verificationResult.Rows[0].ActualPages) {
            throw 'Insight page totals do not match the source joined view. Inspect the migration log before accepting this run.'
        }
    } finally { $verificationConnection.Dispose() }
}

Write-Host "`nAll requested steps completed." -ForegroundColor Green
