<#
.SYNOPSIS
	Updates the definition of an existing Azure API Management API from a swagger.json (OpenAPI) file.
.DESCRIPTION
	Reads an OpenAPI/Swagger JSON file, validates that it is well formed, then imports it into an
	existing APIM API. The current API settings (path, service URL, protocols, revision, version and
	version set) are read from Azure and passed back in so the import does not reset them. After import,
	the script reports (but does not enforce) whether APIM's operations match the OpenAPI document.
.PARAMETER SwaggerFilePath
	Path to the swagger.json (OpenAPI 2.0 or 3.x) file containing the API definition.
.PARAMETER ApimSubscriptionId
	Subscription ID of the API Management service. Defaults to the Octopus variable 'apim.subscriptionId'.
.PARAMETER ApimResourceGroupName
	Resource group name of the API Management service. Defaults to the Octopus variable 'apim.resourceGroupName'.
.PARAMETER ApimServiceName
	Name of the API Management service. Defaults to the Octopus variable 'apim.serviceName'.
.PARAMETER ApiId
	Name (identifier) of the API to update. Defaults to the GWT XbyK API name derived from environment and geography.
.PARAMETER Environment
	The environment abbreviation (e.g. dev, tst, acc, prd). Defaults to the Octopus variable 'environment.abbreviation'.
.PARAMETER Geography
	The geography abbreviation (e.g. eur, sam, asa). Defaults to the Octopus variable 'geography.abbreviation'.
.PARAMETER ApiVersion
	The Azure Resource Manager API version used for the API Management REST calls.
.EXAMPLE
	.\Update-ApimApiFromSwagger.ps1 -SwaggerFilePath '.\swagger.json' -ApiId 'my-api' -ApimSubscriptionId '00000000-0000-0000-0000-000000000000' -ApimResourceGroupName 'BDO-RG-APIM' -ApimServiceName 'BDO-APIM-01'
	Updates a specific API in a specific API Management service.
.NOTES
	This script is intended to run from an Octopus Deploy process. Parameters default to the relevant Octopus Deploy variables.
	Author: Mark Bowman
	For: BDO Global IT
#>

[CmdletBinding(SupportsShouldProcess)]
param (
	[Parameter()] [string] $SwaggerFilePath = "$PSScriptRoot\swagger.json",
	[Parameter()] [string] $ApimSubscriptionId = $OctopusParameters['apim.subscriptionId'],
	[Parameter()] [string] $ApimResourceGroupName = $OctopusParameters['apim.resourceGroupName'],
	[Parameter()] [string] $ApimServiceName = $OctopusParameters['apim.serviceName'],
	[Parameter()] [string] $Environment = $OctopusParameters['environment.abbreviation'],
	[Parameter()] [string] $Geography = $OctopusParameters['geography.abbreviation'],
	[Parameter()] [string] $ApiId
)

$ErrorActionPreference = 'Stop'
$InformationPreference = 'Continue'

Import-Module Az.Accounts -ErrorAction Stop
Import-Module Az.ApiManagement -ErrorAction Stop

$ApiId = '{0}-{1}-bdo-gwt-xbyk-api' -f $Environment.ToLowerInvariant(), $Geography.ToLowerInvariant()

if (-not (Test-Path -LiteralPath $SwaggerFilePath -PathType Leaf)) {
	throw ('OpenAPI file was not found: {0}' -f $SwaggerFilePath)
}

try {
	$SwaggerDocument = Get-Content -LiteralPath $SwaggerFilePath -Raw | ConvertFrom-Json
}
catch {
	throw ('The file {0} does not contain valid JSON: {1}' -f $SwaggerFilePath, $_.Exception.Message)
}

# Build the set of operations the OpenAPI document defines, used to compare against APIM after import.
$SupportedMethods = @('get', 'post', 'put', 'delete', 'patch', 'head', 'options', 'trace')
$ExpectedOperations = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach ($PathProperty in $SwaggerDocument.paths.PSObject.Properties) {
	foreach ($Method in $SupportedMethods) {
		if ($PathProperty.Value.PSObject.Properties.Name -contains $Method) {
			$null = $ExpectedOperations.Add(('{0} {1}' -f $Method.ToUpperInvariant(), $PathProperty.Name))
		}
	}
}

Write-Information ('OpenAPI file contains {0} path/method operation(s).' -f $ExpectedOperations.Count)

$CurrentContext = Get-AzContext
if ($null -eq $CurrentContext -or $CurrentContext.Subscription.Id -ne $ApimSubscriptionId) {
	Write-Information ('Selecting Azure subscription {0}.' -f $ApimSubscriptionId)
	$null = Set-AzContext -Subscription $ApimSubscriptionId -ErrorAction Stop
}

$ApimContext = New-AzApiManagementContext -ResourceGroupName $ApimResourceGroupName -ServiceName $ApimServiceName
Write-Information ('Target API: {0}/{1}/{2}.' -f $ApimServiceName, $ApimResourceGroupName, $ApiId)

$ExistingApi = Get-AzApiManagementApi -Context $ApimContext -ApiId $ApiId -ErrorAction Stop
if ($null -eq $ExistingApi) {
	throw ('Existing API {0} was not found.' -f $ApiId)
}

Write-Information ('Found API {0} (revision {1}, version {2}).' -f $ExistingApi.Name, $ExistingApi.ApiRevision, $ExistingApi.ApiVersion)

if (-not $PSCmdlet.ShouldProcess($ExistingApi.Id, ('Import OpenAPI definition from {0}' -f $SwaggerFilePath))) {
	return
}

$ImportParameters = @{
	Context             = $ApimContext
	ApiId               = $ApiId
	SpecificationFormat = 'OpenApiJson'
	SpecificationPath   = (Resolve-Path -LiteralPath $SwaggerFilePath).Path
	Path                = $ExistingApi.Path
	ApiRevision         = $ExistingApi.ApiRevision
	ApiVersion          = $ExistingApi.ApiVersion
	ApiVersionSetId     = $ExistingApi.ApiVersionSetId
	ServiceUrl          = $ExistingApi.ServiceUrl
	Protocol            = @($ExistingApi.Protocols)
	ErrorAction         = 'Stop'
}

$ImportedApi = Import-AzApiManagementApi @ImportParameters
Write-Information ('Imported OpenAPI definition into API {0}.' -f $ImportedApi.ApiId)

# Diagnostic only: report whether the import alone reconciled APIM's operations with the OpenAPI document.
$FinalOperations = @(Get-AzApiManagementOperation -Context $ApimContext -ApiId $ApiId -ErrorAction Stop)
$ActualOperations = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach ($Operation in $FinalOperations) {
	$null = $ActualOperations.Add(('{0} {1}' -f $Operation.Method.ToUpperInvariant(), $Operation.UrlTemplate.Split('?')[0]))
}

$MissingOperations = @($ExpectedOperations | Where-Object { -not $ActualOperations.Contains($_) } | Sort-Object)
$UnexpectedOperations = @($ActualOperations | Where-Object { -not $ExpectedOperations.Contains($_) } | Sort-Object)

if ($MissingOperations.Count -gt 0) {
	Write-Warning ('APIM is missing {0} operation(s) from OpenAPI: {1}' -f $MissingOperations.Count, ($MissingOperations -join '; '))
}
if ($UnexpectedOperations.Count -gt 0) {
	Write-Warning ('APIM has {0} stale operation(s) not present in OpenAPI: {1}' -f $UnexpectedOperations.Count, ($UnexpectedOperations -join '; '))
}
if ($MissingOperations.Count -eq 0 -and $UnexpectedOperations.Count -eq 0) {
	Write-Information 'Import alone reconciled APIM operations with the OpenAPI document.'
}

Write-Information ('API {0} now has {1} operation(s).' -f $ApiId, $FinalOperations.Count)
$FinalOperations |
	Select-Object -Property Method, UrlTemplate, OperationId |
	Format-Table -AutoSize

