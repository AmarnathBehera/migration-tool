<#
.SYNOPSIS
	Updates the definition of an existing Azure API Management API from a swagger.json (OpenAPI) file.
.DESCRIPTION
	Reads an OpenAPI/Swagger JSON file, validates that it is well formed, then imports it into an
	existing APIM API. The current API settings (display name, path, service URL, protocols, versioning
	and authentication settings) are read from Azure and preserved so that only the operations and
	schemas defined in the specification are replaced. API level policies are stored as separate
	resources and are therefore left untouched.
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
	[Parameter()] [string] $ApiId,
	[Parameter()] [string] $ApiVersion = '2024-05-01'
)

$ErrorActionPreference = 'Stop'
$InformationPreference = 'Continue'

# Derive the GWT XbyK API name when no explicit API identifier is supplied.
if ([string]::IsNullOrWhiteSpace($ApiId)) {
	if ([string]::IsNullOrWhiteSpace($Environment) -or [string]::IsNullOrWhiteSpace($Geography)) {
		throw 'ApiId was not supplied and it cannot be derived because Environment or Geography is empty.'
	}
	$ApiId = '{0}-{1}-bdo-gwt-xbyk-api' -f $Environment.ToLower(), $Geography.ToLower()
}

foreach ($RequiredValue in @('ApimSubscriptionId', 'ApimResourceGroupName', 'ApimServiceName')) {
	if ([string]::IsNullOrWhiteSpace((Get-Variable -Name $RequiredValue -ValueOnly))) {
		throw ('Required parameter {0} has no value.' -f $RequiredValue)
	}
}

# Read the specification and confirm it is valid JSON before sending it to Azure.
$SwaggerContent = Get-Content -Path $SwaggerFilePath -Raw
try {
	$null = $SwaggerContent | ConvertFrom-Json
}
catch {
	throw ('The file {0} does not contain valid JSON: {1}' -f $SwaggerFilePath, $_.Exception.Message)
}

$ApiResourceId = '/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.ApiManagement/service/{2}/apis/{3}' -f `
	$ApimSubscriptionId, $ApimResourceGroupName, $ApimServiceName, $ApiId

Write-Information ('Target API: {0}' -f $ApiResourceId)

# Retrieve the existing API so that its current configuration can be preserved during the import.
$GetResponse = Invoke-AzRestMethod -Method 'GET' -Path ('{0}?api-version={1}' -f $ApiResourceId, $ApiVersion)
if ($GetResponse.StatusCode -ne 200) {
	throw ('Unable to retrieve API {0}. Status code {1}: {2}' -f $ApiId, $GetResponse.StatusCode, $GetResponse.Content)
}

$ExistingApi = $GetResponse.Content | ConvertFrom-Json
$ExistingProperties = $ExistingApi.properties

Write-Information ('Found API {0} (revision {1}, version {2}).' -f $ExistingProperties.displayName, $ExistingProperties.apiRevision, $ExistingProperties.apiVersion)

# Build the import payload, carrying over the existing settings that are not described by the specification.
$Properties = [ordered]@{
	'displayName'          = $ExistingProperties.displayName
	'path'                 = $ExistingProperties.path
	'protocols'            = $ExistingProperties.protocols
	'subscriptionRequired' = $ExistingProperties.subscriptionRequired
	'format'               = 'openapi+json'
	'value'                = $SwaggerContent
}

if ($ExistingProperties.description) { $Properties['description'] = $ExistingProperties.description }
if ($ExistingProperties.serviceUrl) { $Properties['serviceUrl'] = $ExistingProperties.serviceUrl }
if ($ExistingProperties.apiVersion) { $Properties['apiVersion'] = $ExistingProperties.apiVersion }
if ($ExistingProperties.apiVersionSetId) { $Properties['apiVersionSetId'] = $ExistingProperties.apiVersionSetId }
if ($ExistingProperties.apiRevision) { $Properties['apiRevision'] = $ExistingProperties.apiRevision }
if ($ExistingProperties.authenticationSettings) { $Properties['authenticationSettings'] = $ExistingProperties.authenticationSettings }
if ($ExistingProperties.subscriptionKeyParameterNames) { $Properties['subscriptionKeyParameterNames'] = $ExistingProperties.subscriptionKeyParameterNames }

$RequestBody = @{ 'properties' = $Properties } | ConvertTo-Json -Depth 100

if (-not $PSCmdlet.ShouldProcess($ApiResourceId, ('Import API definition from {0}' -f $SwaggerFilePath))) {
	return
}

# Import the specification. A PUT against the existing API identifier replaces its operations and schemas.
try {
	$PutResponse = Invoke-AzRestMethod -Method 'PUT' -Path ('{0}?api-version={1}' -f $ApiResourceId, $ApiVersion) -Payload $RequestBody
}
catch {
	Write-Error ('Failed to import the API definition: {0}' -f $_.Exception.Message)
	throw
}

if ($PutResponse.StatusCode -notin @(200, 201, 202)) {
	throw ('API import failed. Status code {0}: {1}' -f $PutResponse.StatusCode, $PutResponse.Content)
}

# A 202 response means the import is running asynchronously, so poll until it completes.
if ($PutResponse.StatusCode -eq 202) {
	$LocationHeader = ($PutResponse.Headers | Where-Object { $_.Key -eq 'Location' }).Value | Select-Object -First 1
	if ($LocationHeader) {
		Write-Information 'Import accepted, waiting for the asynchronous operation to complete.'
		do {
			Start-Sleep -Seconds 5
			$PollResponse = Invoke-AzRestMethod -Method 'GET' -Uri $LocationHeader
		} while ($PollResponse.StatusCode -eq 202)

		if ($PollResponse.StatusCode -notin @(200, 201, 204)) {
			throw ('API import failed during asynchronous processing. Status code {0}: {1}' -f $PollResponse.StatusCode, $PollResponse.Content)
		}
	}
}

# Report the operations now present on the API so the result can be confirmed in the deployment log.
$OperationsResponse = Invoke-AzRestMethod -Method 'GET' -Path ('{0}/operations?api-version={1}' -f $ApiResourceId, $ApiVersion)
if ($OperationsResponse.StatusCode -eq 200) {
	$Operations = ($OperationsResponse.Content | ConvertFrom-Json).value
	Write-Information ('API {0} updated successfully with {1} operation(s).' -f $ApiId, $Operations.Count)
	$Operations | Select-Object -Property @{ Name = 'Method'; Expression = { $_.properties.method } }, @{ Name = 'UrlTemplate'; Expression = { $_.properties.urlTemplate } }, @{ Name = 'DisplayName'; Expression = { $_.properties.displayName } } | Format-Table -AutoSize
}
else {
	Write-Information ('API {0} updated successfully.' -f $ApiId)
}
