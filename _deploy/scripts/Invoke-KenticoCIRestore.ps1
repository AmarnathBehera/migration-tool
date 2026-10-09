<#
.SYNOPSIS
    Stops all App Services, runs the Kentico XbyK CI restore, then starts them again.
.DESCRIPTION
    This script is intended to run as an Octopus Deploy "Run a Script" step sourced
    from the GWT.XbyK.CI.Deploy package. It orchestrates the full CI restore sequence:
      1. Stop-WebServices  — stops all App Services in the environment resource group
                             and polls until all report a Stopped state.
      2. Invoke-CIRestore  — retrieves the hash string salt from Azure Key Vault,
                             constructs the connection string, injects both as .NET
                             environment variable overrides, and runs the published
                             GWTXbyKWeb.dll with --kxp-ci-restore.
      3. Start-WebServices — starts all App Services once the restore is complete.
    The CMS application must not be running during the restore to avoid deadlocks and
    cache inconsistencies. Stopping and restarting is handled automatically by this script.
    .NET reads environment variables using the double-underscore (__) separator
    convention to override nested configuration keys, for example:
      ConnectionStrings__CMSConnectionString  =>  ConnectionStrings:CMSConnectionString
      CMSHashStringSalt                       =>  CMSHashStringSalt
.PARAMETER Environment
    The environment abbreviation (e.g. DEV, TST, ACC, PRD). Defaults to the Octopus
    variable environment.abbreviation.
.PARAMETER Geography
    The geography abbreviation (e.g. eur, sam, asa). Defaults to the Octopus variable
    geography.abbreviation.
.PARAMETER RegionPrimary
    The primary region abbreviation (e.g. we, bs, sea). Defaults to the Octopus
    variable region.primary.abbreviation.
.PARAMETER StopTimeoutSeconds
    Maximum time in seconds to wait for all App Services to reach a Stopped state
    before aborting. Defaults to 300 seconds.
.PARAMETER StopPollingIntervalSeconds
    Interval in seconds between each App Service state poll during the stop wait.
    Defaults to 10 seconds.
.PARAMETER SqlConnectionTimeoutSeconds
    SQL connection timeout in seconds used by the Kentico CI restore host when it
    opens the initial CMS database connection. Defaults to 120 seconds.
.PARAMETER ResourceGroupNameOverride
    Optional explicit resource group name. When omitted, the script derives the
    resource group name from Environment, Geography, and RegionPrimary.
.PARAMETER KeyVaultNameOverride
    Optional explicit Key Vault name. When omitted, the script derives the Key Vault
    name from Environment and Geography.
.PARAMETER DatabaseServerNameOverride
    Optional explicit Azure SQL logical server name. When omitted, the script derives
    the server name from Environment, Geography, and RegionPrimary.
.PARAMETER DatabaseNameOverride
    Optional explicit Azure SQL database name. When omitted, the script derives the
    database name from Environment, Geography, and RegionPrimary.
.PARAMETER DatabaseWarmupTimeoutSeconds
    Maximum time in seconds to wait for the Azure SQL database to reach an Online state
    before aborting. Relevant in lower environments where the database may be paused by
    Azure SQL serverless auto-pause. Defaults to 300 seconds.
.PARAMETER DatabaseWarmupPollingIntervalSeconds
    Interval in seconds between each database status poll during the warm-up wait.
    Defaults to 15 seconds.
.PARAMETER SkipWebServices
    Skips stopping and starting App Services. Use this only for local or isolated
    restore testing where no running web application is connected to the target database.
.EXAMPLE
    .\Invoke-KenticoCIRestore.ps1 -Environment 'DEV' -Geography 'eur' -RegionPrimary 'we'
    Stops all App Services, runs the CI restore for the DEV environment, and starts them again.
.NOTES
    Kentico documentation:
    https://docs.kentico.com/documentation/developers-and-admins/ci-cd/continuous-integration
#>

[CmdletBinding()]
param (
    [Parameter()] [ValidatePattern('^(?i)(DEV|TST|ACC|PRD)\d?$')] [string] $Environment              = $OctopusParameters['environment.abbreviation'],
    [Parameter()] [ValidatePattern('^[a-z0-9-]+$')]               [string] $Geography                = $OctopusParameters['geography.abbreviation'],
    [Parameter()] [ValidatePattern('^[a-z0-9-]+$')]               [string] $RegionPrimary            = $OctopusParameters['region.primary.abbreviation'],
    [Parameter()] [ValidateRange(30, 1800)]                        [int]    $StopTimeoutSeconds       = 300,
    [Parameter()] [ValidateRange(5, 60)]                           [int]    $StopPollingIntervalSeconds = 10,
    [Parameter()] [ValidateRange(30, 600)]                         [int]    $SqlConnectionTimeoutSeconds = 120,
    [Parameter()] [ValidateRange(60, 1800)]                        [int]    $DatabaseWarmupTimeoutSeconds = 300,
    [Parameter()] [ValidateRange(5, 60)]                           [int]    $DatabaseWarmupPollingIntervalSeconds = 15,
    [Parameter()] [string]                                         $ResourceGroupNameOverride,
    [Parameter()] [string]                                         $KeyVaultNameOverride,
    [Parameter()] [string]                                         $DatabaseServerNameOverride,
    [Parameter()] [string]                                         $DatabaseNameOverride,
    [Parameter()] [switch]                                         $SkipWebServices
)

$ErrorActionPreference = 'Stop'
$InformationPreference = 'Continue'

# Configure Octopus-specific environment settings when running in a deployment context.
if ($OctopusParameters) {
    Write-Verbose ('Running in Octopus:{0}PSEdition: {1}' -f "`n", $PSVersionTable.PSEdition)
    Set-Item Env:\SuppressAzurePowerShellBreakingChangeWarnings 'true'
    Set-Item Env:\NO_COLOR '1'

    if ($PSVersionTable.PSEdition -eq 'Core') {
        $PSStyle.OutputRendering = 'PlainText'
    }
}

# Functions
function Stop-WebServices {
    <#
    .SYNOPSIS
        Stops all App Services in the given resource group and polls until all are stopped.
    #>
    [CmdletBinding()]
    param (
        [Parameter(Mandatory)] [string] $ResourceGroupName,
        [Parameter(Mandatory)] [int]    $TimeoutSeconds,
        [Parameter(Mandatory)] [int]    $PollingIntervalSeconds
    )

    $ErrorActionPreference = 'Stop'
    $InformationPreference = 'Continue'

    # Retrieve all App Services in the resource group.
    $AppServices = ((Invoke-AzRestMethod -ResourceGroupName $ResourceGroupName -ResourceProviderName 'Microsoft.Web' -ResourceType 'sites' -ApiVersion '2024-11-01').Content | ConvertFrom-Json).value

    if (-not $AppServices -or $AppServices.Count -eq 0) {
        Write-Information ('No App Services found in resource group {0}.' -f $ResourceGroupName)
        return
    }

    # Trigger a stop for every App Service concurrently before polling.
    foreach ($AppService in $AppServices) {
        Write-Information ('Stopping App Service: {0}' -f $AppService.Name)
        $null = Stop-AzWebApp -ResourceGroupName $ResourceGroupName -Name $AppService.Name
    }

    # Poll until every App Service reports a Stopped state or the timeout is reached.
    Write-Information 'Polling for App Services to reach Stopped state...'
    $Deadline              = (Get-Date).AddSeconds($TimeoutSeconds)
    $PendingServiceNames   = @($AppServices | ForEach-Object { $_.Name })

    while ($PendingServiceNames.Count -gt 0 -and (Get-Date) -lt $Deadline) {
        Start-Sleep -Seconds $PollingIntervalSeconds

        $StillRunning = @()
        foreach ($ServiceName in $PendingServiceNames) {
            $Response     = Invoke-AzRestMethod -ResourceGroupName $ResourceGroupName -ResourceProviderName 'Microsoft.Web' -ResourceType 'sites' -Name $ServiceName -ApiVersion '2024-11-01'
            $CurrentState = ($Response.Content | ConvertFrom-Json).properties.state
            Write-Information ('  {0}: {1}' -f $ServiceName, $CurrentState)

            if ($CurrentState -ne 'Stopped') {
                $StillRunning += $ServiceName
            }
        }

        $PendingServiceNames = $StillRunning
    }

    if ($PendingServiceNames.Count -gt 0) {
        throw ('The following App Services did not stop within {0} seconds: {1}' -f $TimeoutSeconds, ($PendingServiceNames -join ', '))
    }

    Write-Information 'All App Services stopped successfully.'
}

function Start-WebServices {
    <#
    .SYNOPSIS
        Starts all App Services in the given resource group.
    #>
    [CmdletBinding()]
    param (
        [Parameter(Mandatory)] [string] $ResourceGroupName
    )

    $ErrorActionPreference = 'Stop'
    $InformationPreference = 'Continue'

    # Retrieve all App Services in the resource group.
    $AppServices = ((Invoke-AzRestMethod -ResourceGroupName $ResourceGroupName -ResourceProviderName 'Microsoft.Web' -ResourceType 'sites' -ApiVersion '2024-11-01').Content | ConvertFrom-Json).value

    if (-not $AppServices -or $AppServices.Count -eq 0) {
        Write-Information ('No App Services found in resource group {0}.' -f $ResourceGroupName)
        return
    }

    # Start each App Service and report the outcome.
    $StartResults = @()
    foreach ($AppService in $AppServices) {
        $ServiceName = $AppService.Name
        try {
            Write-Information ('Starting App Service: {0}' -f $ServiceName)
            $null = Start-AzWebApp -ResourceGroupName $ResourceGroupName -Name $ServiceName
            $StartResults += [PSCustomObject]@{
                AppServiceName = $ServiceName
                Status         = 'Started'
                Error          = $null
            }
        }
        catch {
            $StartResults += [PSCustomObject]@{
                AppServiceName = $ServiceName
                Status         = 'Failed'
                Error          = $_.Exception.Message
            }
        }
    }

    $StartResults | Format-Table -AutoSize
}

function Wait-DatabaseOnline {
    <#
    .SYNOPSIS
        Ensures the target Azure SQL database is online before the CI restore begins.
        If the database is in a Paused or Pausing state (Azure SQL serverless auto-pause,
        common in lower environments), this function triggers a resume and polls until the
        database reaches an Online state or the timeout is exceeded.
    #>
    [CmdletBinding()]
    param (
        [Parameter(Mandatory)] [string] $ResourceGroupName,
        [Parameter(Mandatory)] [string] $DatabaseServerName,
        [Parameter(Mandatory)] [string] $DatabaseName,
        [Parameter(Mandatory)] [int]    $TimeoutSeconds,
        [Parameter(Mandatory)] [int]    $PollingIntervalSeconds
    )

    $ErrorActionPreference = 'Stop'
    $InformationPreference = 'Continue'

    $SubscriptionId  = (Get-AzContext).Subscription.Id
    $DatabaseApiPath = '/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.Sql/servers/{2}/databases/{3}?api-version=2023-05-01-preview' -f $SubscriptionId, $ResourceGroupName, $DatabaseServerName, $DatabaseName

    $Status = ((Invoke-AzRestMethod -Method GET -Path $DatabaseApiPath).Content | ConvertFrom-Json).properties.status
    Write-Information ('Database {0} status: {1}' -f $DatabaseName, $Status)

    if ($Status -eq 'Online') {
        return
    }

    if ($Status -in @('Paused', 'Pausing')) {
        Write-Information ('Resuming database {0}...' -f $DatabaseName)
        $ResumePath     = '/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.Sql/servers/{2}/databases/{3}/resume?api-version=2023-05-01-preview' -f $SubscriptionId, $ResourceGroupName, $DatabaseServerName, $DatabaseName
        $ResumeResponse = Invoke-AzRestMethod -Method POST -Path $ResumePath
        if ($ResumeResponse.StatusCode -notin @(200, 202)) {
            throw ('Failed to resume database {0}. HTTP {1}: {2}' -f $DatabaseName, $ResumeResponse.StatusCode, $ResumeResponse.Content)
        }
        Write-Information ('Resume request accepted (HTTP {0}).' -f $ResumeResponse.StatusCode)
    }

    Write-Information ('Waiting for database {0} to come online...' -f $DatabaseName)
    $Deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Get-Date) -lt $Deadline) {
        Start-Sleep -Seconds $PollingIntervalSeconds
        $Status = ((Invoke-AzRestMethod -Method GET -Path $DatabaseApiPath).Content | ConvertFrom-Json).properties.status
        Write-Information ('Database {0} status: {1}' -f $DatabaseName, $Status)
        if ($Status -eq 'Online') {
            return
        }
    }

    throw ('Database {0} did not reach Online status within {1} seconds. Last known status: {2}' -f $DatabaseName, $TimeoutSeconds, $Status)
}

function Invoke-CIRestore {
    <#
    .SYNOPSIS
        Runs the Kentico --kxp-ci-restore command using the published application binary.
    #>
    [CmdletBinding()]
    param (
        [Parameter(Mandatory)] [string] $DllPath,
        [Parameter(Mandatory)] [string] $CiRepositoryPath,
        [Parameter(Mandatory)] [string] $ConnectionString,
        [Parameter(Mandatory)] [string] $HashStringSalt
    )

    $ErrorActionPreference = 'Stop'
    $InformationPreference = 'Continue'

    # Validate that the required files are present in the extracted package.
    if (-not (Test-Path $DllPath)) {
        throw ('GWTXbyKWeb.dll not found at expected path: {0}' -f $DllPath)
    }

    if (-not (Test-Path $CiRepositoryPath)) {
        throw ('CI repository folder not found at: {0}' -f $CiRepositoryPath)
    }

    # Retrieve a SQL Database access token from the current Az context.
    Write-Information -MessageData 'Retrieving Azure access token for Azure SQL...'
    $null = (Get-AzAccessToken -ResourceUrl 'https://database.windows.net/').Token

    $AzContext  = Get-AzContext
    $AzClientId = $AzContext.Account.Id
    $AzTenantId = $AzContext.Tenant.Id
    Write-Information ('Azure identity: {0} / tenant {1}' -f $AzClientId, $AzTenantId)

    # Inject configuration as .NET environment variable overrides so the published binary connects to the correct database without modifying appsettings.json.
    $env:ConnectionStrings__CMSConnectionString = $ConnectionString
    $env:CMSHashStringSalt                       = $HashStringSalt
    $env:AZURE_CLIENT_ID                         = $AzClientId
    $env:AZURE_TENANT_ID                         = $AzTenantId

    Write-Information 'Starting Kentico CI restore...'
    Write-Information ('  DLL:           {0}' -f $DllPath)
    Write-Information ('  CI repository: {0}' -f $CiRepositoryPath)

    # Run using the published binary — only the .NET runtime is required on the worker. The Kentico hosting builder intercepts --kxp-ci-restore, performs the restore, and exits without starting the full web application.
    try {
        & dotnet $DllPath --kxp-ci-restore

        if ($LASTEXITCODE -ne 0) {
            throw ('Kentico CI restore failed with exit code: {0}' -f $LASTEXITCODE)
        }
    }
    finally {
        $env:ConnectionStrings__CMSConnectionString = $null
        $env:CMSHashStringSalt                       = $null
        Remove-Item Env:\AZURE_CLIENT_ID -ErrorAction SilentlyContinue
        Remove-Item Env:\AZURE_TENANT_ID -ErrorAction SilentlyContinue
    }

    Write-Information 'Kentico CI restore completed successfully.'
}

# Derive all resource names from the deployment parameters.
$EnvironmentLower   = $Environment.ToLower()
$GeographyLower     = $Geography.ToLower()
$RegionPrimaryLower = $RegionPrimary.ToLower()

$ResourceGroupName  = if ($ResourceGroupNameOverride) { $ResourceGroupNameOverride } else { 'bdo-rg-gwt-xbyk-{0}-{1}-{2}' -f $EnvironmentLower, $GeographyLower, $RegionPrimaryLower }
$KeyVaultName       = if ($KeyVaultNameOverride) { $KeyVaultNameOverride } else { 'bdo-kv-gwt-xbyk-{0}-{1}' -f $EnvironmentLower, $GeographyLower }
$DatabaseServerName = if ($DatabaseServerNameOverride) { $DatabaseServerNameOverride } else { 'bdo-dbs-gwt-xbyk-{0}-{1}-{2}' -f $EnvironmentLower, $GeographyLower, $RegionPrimaryLower }
$DatabaseName       = if ($DatabaseNameOverride) { $DatabaseNameOverride } else { 'bdo-db-gwt-xbyk-{0}-{1}-{2}' -f $EnvironmentLower, $GeographyLower, $RegionPrimaryLower }

Write-Information ('Resource group:  {0}' -f $ResourceGroupName)
Write-Information ('Key Vault:       {0}' -f $KeyVaultName)
Write-Information ('Database server: {0}' -f $DatabaseServerName)
Write-Information ('Database:        {0}' -f $DatabaseName)
Write-Information ('SQL timeout:     {0} seconds' -f $SqlConnectionTimeoutSeconds)

# Retrieve the CMS hash string salt from Azure Key Vault.
Write-Information ('Retrieving hash string salt from Key Vault: {0}' -f $KeyVaultName)
$HashStringSalt = $null
try {
    $HashStringSalt = Get-AzKeyVaultSecret -VaultName $KeyVaultName -Name 'gwt-xbyk-cms-hashStringSalt' -AsPlainText
}
catch {
    Write-Error ('Failed to retrieve secret from Key Vault {0}: {1}' -f $KeyVaultName, $_.Exception.Message)
    exit 1
}

if ([string]::IsNullOrWhiteSpace($HashStringSalt)) {
    Write-Error ('Secret gwt-xbyk-cms-hashStringSalt was empty or null in Key Vault {0}.' -f $KeyVaultName)
    exit 1
}

# Build the connection string using Active Directory Default authentication, which resolves via the default Azure credential chain on the Octopus worker.
$ConnectionString = 'Server=tcp:{0}.database.windows.net,1433;Initial Catalog={1};Encrypt=True;TrustServerCertificate=False;Connection Timeout={2};Authentication="Active Directory Default";' -f $DatabaseServerName, $DatabaseName, $SqlConnectionTimeoutSeconds

# Resolve the DLL and CI repository paths within the extracted package.
$DllPath          = Join-Path $PSScriptRoot 'GWTXbyKWeb.dll'
$CiRepositoryPath = Join-Path $PSScriptRoot 'App_Data' 'CIRepository'

# Stop all App Services and wait for them to reach Stopped state before restoring.
$WebServicesStopped = $false
$CIRestoreFailed   = $false

if (-not $SkipWebServices) {
    Stop-WebServices -ResourceGroupName $ResourceGroupName -TimeoutSeconds $StopTimeoutSeconds -PollingIntervalSeconds $StopPollingIntervalSeconds
    $WebServicesStopped = $true
}
else {
    Write-Information 'Skipping App Service stop/start management for this run.'
}

# Ensure the database is online before attempting the restore. Azure SQL serverless databases
# in lower environments are auto-paused after a period of inactivity and require an explicit
# resume before any connection attempt will succeed.
Wait-DatabaseOnline -ResourceGroupName $ResourceGroupName -DatabaseServerName $DatabaseServerName -DatabaseName $DatabaseName -TimeoutSeconds $DatabaseWarmupTimeoutSeconds -PollingIntervalSeconds $DatabaseWarmupPollingIntervalSeconds

try {
    # Run the CI restore with the resolved credentials and paths.
    Invoke-CIRestore -DllPath $DllPath -CiRepositoryPath $CiRepositoryPath -ConnectionString $ConnectionString -HashStringSalt $HashStringSalt
}
catch {
    $CIRestoreFailed = $true
    throw
}
finally {
    if ($WebServicesStopped) {
        try {
            # Restart all App Services even if CI restore fails, to avoid prolonged downtime.
            Start-WebServices -ResourceGroupName $ResourceGroupName
        }
        catch {
            Write-Error ('Failed to restart App Services in resource group {0}: {1}' -f $ResourceGroupName, $_.Exception.Message)
            if (-not $CIRestoreFailed) {
                throw
            }
        }
    }
}
