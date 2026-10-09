[CmdletBinding()]
param(
    [switch]$Execute,
    [switch]$BypassDependencyCheck
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path $PSScriptRoot -Parent
$cliDirectory = Join-Path $repositoryRoot 'Migration.Tool.CLI'
$settingsPath = Join-Path $cliDirectory 'appsettings.json'
$configuration = Get-Content -LiteralPath $settingsPath -Raw | ConvertFrom-Json
$settings = $configuration.Settings
$runDirectory = Join-Path $cliDirectory ('logs\runs\' + (Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
[void](New-Item -ItemType Directory -Path $runDirectory -Force)
Start-Transcript -Path (Join-Path $runDirectory 'preflight.log') | Out-Null

function Read-Database {
    param([string]$ConnectionString, [string]$Query, [hashtable]$Parameters = @{})

    $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
    try {
        $connection.Open()
        $command = $connection.CreateCommand()
        $command.CommandText = $Query
        $command.CommandTimeout = 30
        foreach ($name in $Parameters.Keys) {
            [void]$command.Parameters.AddWithValue('@' + $name, $Parameters[$name])
        }
        $table = New-Object System.Data.DataTable
        try {
            $reader = $command.ExecuteReader()
            try { $table.Load($reader) }
            finally { $reader.Dispose() }
            return , $table
        }
        finally {
            $command.Dispose()
        }
    }
    finally {
        $connection.Dispose()
    }
}

function Test-IncludedCodeName {
    param([string]$Name, $Entity)

    return (!$Entity.IncludeCodeNames -or $Entity.IncludeCodeNames -contains $Name) -and
    (!$Entity.ExcludeCodeNames -or $Entity.ExcludeCodeNames -notcontains $Name)
}

function Test-IncludedPagePath {
    param([string]$Path)

    if (!$settings.MigrateOnlyPagePaths -or $settings.MigrateOnlyPagePaths.Count -eq 0) {
        return $true
    }
    $normalizedPath = '/' + $Path.Trim().Trim('/')
    foreach ($selection in $settings.MigrateOnlyPagePaths) {
        $normalizedSelection = '/' + $selection.Trim().Trim('/')
        if ($normalizedSelection.EndsWith('/*')) {
            $base = $normalizedSelection.Substring(0, $normalizedSelection.Length - 2)
            if (!$base -or $normalizedPath -eq $base -or $normalizedPath.StartsWith($base + '/', [StringComparison]::OrdinalIgnoreCase)) {
                return $true
            }
        }
        elseif ($normalizedPath -eq $normalizedSelection) {
            return $true
        }
    }
    return $false
}

function Get-SourceSetting {
    param([int]$SiteId, [string]$Name)

    $siteValue = @($sourceSettings.Rows | Where-Object { $_.KeyName -eq $Name -and $_.SiteID -eq $SiteId })
    if ($siteValue.Count -gt 0) {
        return [string]$siteValue[0].KeyValue
    }
    $globalValue = @($sourceSettings.Rows | Where-Object { $_.KeyName -eq $Name -and $_.SiteID -is [DBNull] })
    if ($globalValue.Count -gt 0) {
        return [string]$globalValue[0].KeyValue
    }
    return ''
}

try {
    if (Test-Path (Join-Path $cliDirectory 'appsettings.local.json')) {
        throw 'appsettings.local.json exists. Reconcile its overrides with appsettings.json before using this preflight runner.'
    }
    foreach ($name in @('KxCmsDirPath', 'XbyKDirPath')) {
        if (!(Test-Path -LiteralPath $settings.$name -PathType Container)) {
            throw "Settings.$name does not point to an existing directory."
        }
    }
    if (!$settings.MigrationProtocolPath) {
        throw 'Settings.MigrationProtocolPath must be configured.'
    }
    if ($settings.MigrateMediaToMediaLibrary) {
        throw 'This runner expects media migration to Content hub (MigrateMediaToMediaLibrary=false).'
    }
    if ($settings.OptInFeatures.QuerySourceInstanceApi.Enabled) {
        throw 'This runner expects the registered BDO widget migrations with API discovery disabled.'
    }
    foreach ($name in @('ConvertClassesToContentHub', 'CreateReusableFieldSchemaForClasses')) {
        if ($settings.$name -is [array]) {
            throw "Settings.$name must be a semicolon-separated string, not an array."
        }
    }

    $sourceBuilder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder($settings.KxConnectionString)
    $targetConnection = $settings.XbyKApiSettings.ConnectionStrings.CMSConnectionString
    $targetBuilder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder($targetConnection)
    Write-Host "Source: $($sourceBuilder.DataSource) / $($sourceBuilder.InitialCatalog)"
    Write-Host "Target: $($targetBuilder.DataSource) / $($targetBuilder.InitialCatalog)"
    if ($sourceBuilder.DataSource -eq $targetBuilder.DataSource -and $sourceBuilder.InitialCatalog -eq $targetBuilder.InitialCatalog) {
        throw 'Source and target databases must be different.'
    }
    $targetVersion = Read-Database $targetConnection "SELECT KeyValue FROM CMS_SettingsKey WHERE KeyName='CMSDBVersion'"
    [xml]$apiProject = Get-Content (Join-Path $repositoryRoot 'Migration.Tool.KXP.Api\Migration.Tool.KXP.Api.csproj') -Raw
    $packageVersion = ($apiProject.Project.ItemGroup.PackageReference | Where-Object { $_.Include -eq 'Kentico.Xperience.Admin' }).Version
    if ($targetVersion.Rows.Count -ne 1 -or $targetVersion.Rows[0].KeyValue -ne $packageVersion) {
        throw "Target database version does not match Kentico.Xperience.Admin $packageVersion."
    }

    $siteQuery = 'SELECT SiteID, SiteName FROM CMS_Site'
    $siteParameters = @{}
    $siteParameterNames = @()
    foreach ($name in $settings.EntityConfigurations.CMS_Site.IncludeCodeNames) {
        $parameterName = 'site' + $siteParameters.Count
        $siteParameters[$parameterName] = $name
        $siteParameterNames += '@' + $parameterName
    }
    if ($siteParameterNames.Count -gt 0) { $siteQuery += ' WHERE SiteName IN (' + ($siteParameterNames -join ',') + ')' }
    $siteTable = Read-Database $settings.KxConnectionString $siteQuery $siteParameters
    $sites = @($siteTable.Rows | Where-Object { Test-IncludedCodeName $_.SiteName $settings.EntityConfigurations.CMS_Site })
    if ($sites.Count -eq 0) {
        throw 'No source sites match EntityConfigurations.CMS_Site.'
    }
    $siteIds = @($sites | ForEach-Object { [int]$_.SiteID })
    $siteIdSql = $siteIds -join ','
    $pageTable = Read-Database $settings.KxConnectionString @"
SELECT t.NodeID, t.NodeGUID, t.NodeAliasPath, t.NodeSiteID, t.NodeLinkedNodeID,
       t.NodeLinkedNodeSiteID, c.ClassName, s.SiteName, COUNT(d.DocumentID) AS Documents
FROM CMS_Tree t
JOIN CMS_Class c ON c.ClassID=t.NodeClassID
JOIN CMS_Site s ON s.SiteID=t.NodeSiteID
LEFT JOIN CMS_Document d ON d.DocumentNodeID=t.NodeID
WHERE t.NodeSiteID IN ($siteIdSql)
GROUP BY t.NodeID, t.NodeGUID, t.NodeAliasPath, t.NodeSiteID, t.NodeLinkedNodeID,
         t.NodeLinkedNodeSiteID, c.ClassName, s.SiteName
ORDER BY s.SiteName, t.NodeAliasPath
"@
    $pages = @($pageTable.Rows | Where-Object { $siteIds -contains $_.NodeSiteID } | ForEach-Object {
            $reason = 'Selected'
            if (!(Test-IncludedPagePath $_.NodeAliasPath)) { $reason = 'ExcludedPath' }
            elseif (!(Test-IncludedCodeName $_.ClassName $settings.EntityConfigurations.CMS_Class)) { $reason = 'ExcludedClass' }
            elseif ($_.ClassName -eq 'CMS.Root') { $reason = 'UnsupportedRoot' }
            elseif ($_.NodeLinkedNodeSiteID -isnot [DBNull] -and $_.NodeLinkedNodeSiteID -ne $_.NodeSiteID -and $_.NodeLinkedNodeID -isnot [DBNull]) { $reason = 'CrossSiteLink' }
            elseif ($_.Documents -eq 0 -and ($_.NodeLinkedNodeID -is [DBNull] -or $_.NodeLinkedNodeID -eq 0)) { $reason = 'NoDocuments' }
            [pscustomobject]@{ Site = $_.SiteName; NodeID = $_.NodeID; NodeGUID = $_.NodeGUID; Path = $_.NodeAliasPath; Class = $_.ClassName; Documents = $_.Documents; Selection = $reason }
        })
    $pages | Export-Csv (Join-Path $runDirectory 'pages.csv') -NoTypeInformation
    $pages | Group-Object Class, Selection | Select-Object Name, Count | Export-Csv (Join-Path $runDirectory 'page-types.csv') -NoTypeInformation
    $selectedNodeIds = @($pages | Where-Object { $_.Selection -eq 'Selected' } | ForEach-Object { $_.NodeID })
    if ($selectedNodeIds.Count -eq 0) { throw 'No source pages are selected.' }

    $documentTable = Read-Database $settings.KxConnectionString "SELECT d.DocumentID, d.DocumentNodeID, d.DocumentCulture, d.DocumentPageBuilderWidgets FROM CMS_Document d JOIN CMS_Tree t ON t.NodeID=d.DocumentNodeID WHERE t.NodeSiteID IN ($siteIdSql) AND NULLIF(d.DocumentPageBuilderWidgets, '') IS NOT NULL"
    $widgetRows = @()
    $areaRows = @()
    foreach ($document in $documentTable.Rows | Where-Object { $selectedNodeIds -contains $_.DocumentNodeID }) {
        $builder = $document.DocumentPageBuilderWidgets | ConvertFrom-Json
        foreach ($area in $builder.editableAreas) {
            $areaRows += [pscustomobject]@{ NodeID = $document.DocumentNodeID; Culture = $document.DocumentCulture; SourceArea = $area.identifier; TargetArea = $(if ($settings.TargetEditableAreaIdentifier) { $settings.TargetEditableAreaIdentifier } else { $area.identifier }) }
            foreach ($section in $area.sections) {
                foreach ($zone in $section.zones) {
                    foreach ($widget in $zone.widgets) {
                        $widgetRows += [pscustomobject]@{ NodeID = $document.DocumentNodeID; DocumentID = $document.DocumentID; Culture = $document.DocumentCulture; Area = $area.identifier; Widget = $widget.type; Instance = $widget.identifier }
                    }
                }
            }
        }
    }
    $widgetRows | Export-Csv (Join-Path $runDirectory 'widget-instances.csv') -NoTypeInformation
    $areaRows | Export-Csv (Join-Path $runDirectory 'editable-areas.csv') -NoTypeInformation
    $targetConstantsPath = Join-Path $settings.XbyKDirPath 'Components\ComponentIdentifiers.cs'
    $targetConstants = Get-Content $targetConstantsPath -Raw
    $renames = @{ 'BDO.Widget.Accordion' = 'AccordionWidget'; 'BDO.Widget.Faq' = 'FaqWidget'; 'BDO.Widget.EventsWebinarsSlider' = 'EventWebinarSliderWidget'; 'BDO.Widget.InsightAnimationCard' = 'AnimatedTileCarouselWidget' }
    $widgetSummary = @($widgetRows | Group-Object Widget | Sort-Object Name | ForEach-Object {
            $sourceType = $_.Name
            $targetSuffix = $sourceType -replace '^BDO\.Widget\.', ''
            if ($renames.ContainsKey($sourceType)) { $targetSuffix = $renames[$sourceType] }
            $isForm = $sourceType -eq 'Kentico.FormWidget'
            $supported = $isForm -or ($sourceType.StartsWith('BDO.Widget.') -and $targetConstants.Contains('WidgetPrefix + "' + $targetSuffix + '"'))
            [pscustomobject]@{ Source = $sourceType; Target = $(if ($isForm) { $sourceType } else { 'BDO.Widget.' + $targetSuffix }); Instances = $_.Count; Mapping = $(if ($isForm) { 'BuiltIn' } elseif ($renames.ContainsKey($sourceType)) { 'Rename' } else { 'IdentityWithPropertyTransforms' }); TargetDeclared = $supported }
        })
    $widgetSummary | Export-Csv (Join-Path $runDirectory 'widgets.csv') -NoTypeInformation
    if (@($widgetSummary | Where-Object { !$_.TargetDeclared }).Count -gt 0) {
        throw 'Unmapped widget identifiers found. Review widgets.csv before executing.'
    }

    $sourceSettings = Read-Database $settings.KxConnectionString "SELECT KeyName, SiteID, KeyValue FROM CMS_SettingsKey WHERE KeyName IN ('CMSMediaLibrariesFolder','CMSUseMediaLibrariesSiteFolder')"
    $mediaTable = Read-Database $settings.KxConnectionString @"
SELECT m.FileID, m.FileGUID, m.FileName, m.FilePath, m.FileSiteID,
       l.LibraryName, l.LibraryFolder, s.SiteName
FROM Media_File m
JOIN Media_Library l ON l.LibraryID=m.FileLibraryID
JOIN CMS_Site s ON s.SiteID=m.FileSiteID
WHERE m.FileSiteID IN ($siteIdSql)
ORDER BY s.SiteName, l.LibraryName, m.FilePath
"@
    $mediaRows = @($mediaTable.Rows | Where-Object { $siteIds -contains $_.FileSiteID } | ForEach-Object {
            $folder = Get-SourceSetting $_.FileSiteID 'CMSMediaLibrariesFolder'
            $useSiteFolder = (Get-SourceSetting $_.FileSiteID 'CMSUseMediaLibrariesSiteFolder') -ne 'false'
            $parts = @()
            if ($folder) {
                if ([IO.Path]::IsPathRooted($folder)) { $parts += $folder }
                else { $parts += $settings.KxCmsDirPath; $parts += $folder -replace '^~[/\\]', '' }
                if ($useSiteFolder) { $parts += $_.SiteName }
            }
            else {
                $parts += $settings.KxCmsDirPath
                if ($useSiteFolder) { $parts += $_.SiteName }
                $parts += 'media'
            }
            $parts += $_.LibraryFolder
            $parts += $_.FilePath
            $filePath = [IO.Path]::Combine([string[]]$parts)
            $available = Test-Path -LiteralPath $filePath -PathType Leaf
            [pscustomobject]@{ FileID = $_.FileID; FileGUID = $_.FileGUID; Site = $_.SiteName; Library = $_.LibraryName; Path = $filePath; Available = $available; Action = $(if ($settings.MigrateOnlyMediaFileInfo) { 'MetadataOnly' } elseif ($available) { 'Import' } else { 'SkipMissing' }) }
        })
    $mediaRows | Export-Csv (Join-Path $runDirectory 'media.csv') -NoTypeInformation
    $summary = [ordered]@{
        Status = 'PreflightComplete'; SourceDatabase = $sourceBuilder.InitialCatalog; TargetDatabase = $targetBuilder.InitialCatalog
        TargetVersion = $targetVersion.Rows[0].KeyValue; Sites = @($sites | ForEach-Object { $_.SiteName })
        PagePathFilters = @($settings.MigrateOnlyPagePaths); SourceNodes = $pages.Count; SelectedNodes = $selectedNodeIds.Count
        WidgetTypes = $widgetSummary.Count; WidgetInstances = $widgetRows.Count; TargetEditableArea = $settings.TargetEditableAreaIdentifier
        ReimportExistingPages = $settings.ReimportExistingPages; MediaRecords = $mediaRows.Count
        MediaAvailable = @($mediaRows | Where-Object { $_.Available }).Count; MediaMissing = @($mediaRows | Where-Object { !$_.Available }).Count
        Notes = @('Page selection is a preflight candidate list; directors, linked-node dependencies, cultures, target schemas and import validation determine final outcomes.', 'mainArea merging was explicitly requested; target views also contain other areas.', 'Original runner was absent; this is a replacement, not a recovered copy.')
    }
    $summary | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $runDirectory 'summary.json') -Encoding UTF8
    Write-Host "Pages: $($selectedNodeIds.Count) selected candidates of $($pages.Count) source nodes. Widget types: $($widgetSummary.Count)."
    Write-Host "Media: $($mediaRows.Count) records, $($summary.MediaAvailable) available, $($summary.MediaMissing) missing."
    if ($settings.TargetEditableAreaIdentifier) {
        Write-Warning "All editable areas will merge into '$($settings.TargetEditableAreaIdentifier)'; target views with other area identifiers may not render that content."
    }
    Write-Host "Tracking directory: $runDirectory"

    $migrationArguments = @('migrate', '--nowait', '--sites', '--custom-modules', '--users', '--page-types', '--pages', '--categories', '--media-libraries', '--forms')
    if ($BypassDependencyCheck) { $migrationArguments += '--bypass-dependency-check' }
    Write-Host ('Migration flags: ' + ($migrationArguments -join ' '))
    if ($Execute) {
        Push-Location $cliDirectory
        try {
            & dotnet build 'Migration.Tool.CLI.csproj' --verbosity quiet 2>&1 | Tee-Object (Join-Path $runDirectory 'build.log')
            if ($LASTEXITCODE -ne 0) { throw 'Build failed. Migration was not started.' }
            $summary.Status = 'Running'
            $summary | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $runDirectory 'summary.json') -Encoding UTF8
            $protocolPath = $settings.MigrationProtocolPath
            if (![IO.Path]::IsPathRooted($protocolPath)) { $protocolPath = Join-Path $settings.XbyKDirPath $protocolPath }
            $protocolDirectory = Split-Path $protocolPath -Parent
            [void](New-Item -ItemType Directory -Path $protocolDirectory -Force)
            $consoleLogPath = Join-Path $protocolDirectory ('migration-run-' + (Split-Path $runDirectory -Leaf) + '.log')
            & dotnet run --no-build -- @migrationArguments 2>&1 | Tee-Object $consoleLogPath | Tee-Object (Join-Path $runDirectory 'migration-run.log')
            $summary.ExitCode = $LASTEXITCODE
            $summary.Status = 'RunFinishedReviewRequired'
            if (Test-Path -LiteralPath $protocolPath) { Copy-Item -LiteralPath $protocolPath -Destination $runDirectory }
            $summary.ProtocolPath = $protocolPath
            $summary.ConsoleLogPath = $consoleLogPath
            $summary | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $runDirectory 'summary.json') -Encoding UTF8
            if ($summary.ExitCode -ne 0) { throw "Migration exited with code $($summary.ExitCode). Review migration-run.log." }
            Write-Host 'CLI finished. Review per-item warnings/errors and validate target data before declaring migration successful.'
        }
        finally { Pop-Location }
    }
    else {
        Write-Host 'Preflight only. Add -Execute to run the combined migration after reviewing the inventories.'
    }
}
catch {
    if ($null -ne $summary) {
        $summary.Status = 'Failed'
        $summary.FailureMessage = $_.Exception.Message
        $summary.FailureStack = $_.ScriptStackTrace
        $summary | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $runDirectory 'summary.json') -Encoding UTF8
    }
    [ordered]@{ Status = 'Failed'; Message = $_.Exception.Message; Stack = $_.ScriptStackTrace } |
    ConvertTo-Json | Set-Content (Join-Path $runDirectory 'failure.json') -Encoding UTF8
    throw
}
finally {
    Stop-Transcript | Out-Null
}