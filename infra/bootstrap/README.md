# Bootstrap

This folder contains PowerShell entrypoints for validating, previewing, and deploying the Bicep templates with the Azure CLI.

- `bootstrap.ps1` runs `validate`, `what-if`, or `deploy` against either the platform or workload stack.
- `bootstrap-and-deploy.ps1` provides a simple end-to-end flow for promoting an environment by deploying platform first and workload second.

These scripts are designed to plug into Azure DevOps or GitHub Actions pipeline steps and avoid storing secrets in source control.
