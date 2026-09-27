[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('platform', 'order-api')]
    [string]$Target,

    [Parameter(Mandatory = $true)]
    [ValidateSet('dev', 'prod')]
    [string]$Environment,

    [Parameter(Mandatory = $true)]
    [ValidateSet('validate', 'what-if', 'deploy')]
    [string]$Action,

    [string]$Location = 'westeurope',
    [string]$DeploymentName,
    [string]$ManagementGroupId,
    [string]$SubscriptionId
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

function Invoke-AzCli {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    Write-Host ''
    Write-Host "Executing: az $($Arguments -join ' ')" -ForegroundColor Cyan
    & az @Arguments

    if ($LASTEXITCODE -ne 0) {
        throw "Azure CLI command failed with exit code $LASTEXITCODE."
    }
}

if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
    throw 'Azure CLI is required. Install https://learn.microsoft.com/cli/azure/install-azure-cli before running this script.'
}

switch ($Target) {
    'platform' {
        if (-not $ManagementGroupId) {
            throw 'ManagementGroupId is required for platform deployments.'
        }

        $templateFile = Join-Path $root 'platform\main.bicep'
        $parametersFile = Join-Path $root "platform\params\$Environment.json"
        $scopeArgs = @('mg', 'create', '--management-group-id', $ManagementGroupId)
    }
    'order-api' {
        if (-not $SubscriptionId) {
            throw 'SubscriptionId is required for workload deployments.'
        }

        $templateFile = Join-Path $root 'workloads\order-api\main.bicep'
        $parametersFile = Join-Path $root "workloads\order-api\params\$Environment.json"
        $scopeArgs = @('sub', '--subscription', $SubscriptionId)
    }
}

if (-not (Test-Path $templateFile)) {
    throw "Template file not found: $templateFile"
}

if (-not (Test-Path $parametersFile)) {
    throw "Parameters file not found: $parametersFile"
}

$deploymentName = if ($DeploymentName) {
    $DeploymentName
} else {
    "$Target-$Environment-$(Get-Date -Format 'yyyyMMddHHmmss')"
}

switch ($Action) {
    'validate' {
        $command = switch ($Target) {
            'platform' { @('deployment', 'mg', 'validate', '--management-group-id', $ManagementGroupId) }
            'order-api' { @('deployment', 'sub', 'validate', '--subscription', $SubscriptionId, '--location', $Location) }
        }

        $command += @(
            '--name', $deploymentName,
            '--template-file', $templateFile,
            '--parameters', "@$parametersFile"
        )

        Invoke-AzCli -Arguments $command
    }
    'what-if' {
        $command = switch ($Target) {
            'platform' { @('deployment', 'mg', 'what-if', '--management-group-id', $ManagementGroupId) }
            'order-api' { @('deployment', 'sub', 'what-if', '--subscription', $SubscriptionId, '--location', $Location) }
        }

        $command += @(
            '--name', $deploymentName,
            '--template-file', $templateFile,
            '--parameters', "@$parametersFile"
        )

        Invoke-AzCli -Arguments $command
    }
    'deploy' {
        $command = switch ($Target) {
            'platform' { @('deployment', 'mg', 'create', '--management-group-id', $ManagementGroupId) }
            'order-api' { @('deployment', 'sub', 'create', '--subscription', $SubscriptionId, '--location', $Location) }
        }

        $command += @(
            '--name', $deploymentName,
            '--template-file', $templateFile,
            '--parameters', "@$parametersFile"
        )

        Invoke-AzCli -Arguments $command
    }
}
