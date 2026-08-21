<#
.SYNOPSIS
	Updates the definition of an existing Azure API Management API from a swagger.json (OpenAPI) file.
.DESCRIPTION
	Reads an OpenAPI/Swagger JSON file, validates that it is well formed, then imports it into an
	existing APIM API. The current API settings (path, service URL, protocols, revision, version and
	version set) are read from Azure and passed back in so the import does not reset them.
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

if ([string]::IsNullOrWhiteSpace($ApiId)) {
	if ([string]::IsNullOrWhiteSpace($Environment) -or [string]::IsNullOrWhiteSpace($Geography)) {
		throw 'ApiId was not supplied and it cannot be derived because Environment or Geography is empty.'
	}

	$ApiId = '{0}-{1}-bdo-gwt-xbyk-api' -f $Environment.ToLowerInvariant(), $Geography.ToLowerInvariant()
}

if (-not (Test-Path -LiteralPath $SwaggerFilePath -PathType Leaf)) {
	throw ('OpenAPI file was not found: {0}' -f $SwaggerFilePath)
}

try {
	$SwaggerDocument = Get-Content -LiteralPath $SwaggerFilePath -Raw | ConvertFrom-Json
}
catch {
	throw ('The file {0} does not contain valid JSON: {1}' -f $SwaggerFilePath, $_.Exception.Message)
}

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

# Import-AzApiManagementApi takes the display name from info.title, so it must match the existing API's name to avoid a name collision with another API in the same APIM service.
$SwaggerDocument.info.title = $ExistingApi.Name
$PatchedSwaggerFilePath = Join-Path ([System.IO.Path]::GetTempPath()) ('{0}.json' -f [Guid]::NewGuid())
$SwaggerDocument | ConvertTo-Json -Depth 100 | Set-Content -LiteralPath $PatchedSwaggerFilePath -Encoding utf8

$ImportParameters = @{
	Context             = $ApimContext
	ApiId               = $ApiId
	SpecificationFormat = 'OpenApiJson'
	SpecificationPath   = $PatchedSwaggerFilePath
	Path                = $ExistingApi.Path
	ApiRevision         = $ExistingApi.ApiRevision
	ApiVersion          = $ExistingApi.ApiVersion
	ApiVersionSetId     = $ExistingApi.ApiVersionSetId
	ServiceUrl          = $ExistingApi.ServiceUrl
	Protocol            = @($ExistingApi.Protocols)
	ErrorAction         = 'Stop'
}

try {
	$ImportedApi = Import-AzApiManagementApi @ImportParameters
}
finally {
	Remove-Item -LiteralPath $PatchedSwaggerFilePath -ErrorAction SilentlyContinue
}

Write-Information ('Imported OpenAPI definition into API {0}.' -f $ImportedApi.ApiId)

$FinalOperations = @(Get-AzApiManagementOperation -Context $ApimContext -ApiId $ApiId -ErrorAction Stop)
Write-Information ('API {0} updated successfully with {1} operation(s).' -f $ApiId, $FinalOperations.Count)
$FinalOperations |
	Select-Object -Property Method, UrlTemplate, OperationId |
	Format-Table -AutoSize

