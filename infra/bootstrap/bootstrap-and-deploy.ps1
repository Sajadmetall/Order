[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('dev', 'prod')]
    [string]$Environment,

    [Parameter(Mandatory = $true)]
    [string]$ManagementGroupId,

    [Parameter(Mandatory = $true)]
    [string]$SubscriptionId,

    [string]$Location = 'westeurope',
    [switch]$SkipPlatform,
    [switch]$SkipWhatIf
)

$ErrorActionPreference = 'Stop'
$scriptPath = Join-Path $PSScriptRoot 'bootstrap.ps1'

if (-not $SkipPlatform) {
    if (-not $SkipWhatIf) {
        & $scriptPath -Target platform -Environment $Environment -Action validate -ManagementGroupId $ManagementGroupId
        & $scriptPath -Target platform -Environment $Environment -Action what-if -ManagementGroupId $ManagementGroupId
    }

    & $scriptPath -Target platform -Environment $Environment -Action deploy -ManagementGroupId $ManagementGroupId
}

if (-not $SkipWhatIf) {
    & $scriptPath -Target order-api -Environment $Environment -Action validate -SubscriptionId $SubscriptionId -Location $Location
    & $scriptPath -Target order-api -Environment $Environment -Action what-if -SubscriptionId $SubscriptionId -Location $Location
}

& $scriptPath -Target order-api -Environment $Environment -Action deploy -SubscriptionId $SubscriptionId -Location $Location
