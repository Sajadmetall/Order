# Order API Workload

This workload layer provisions the application infrastructure for the Order API.

- `main.bicep` deploys the resource group and orchestrates the workload modules.
- `modules/` contains reusable resource modules for compute, database, and monitoring.
- `params/` contains environment-specific values for `dev` and `prod`.

The template is ready for CI/CD usage and expects sensitive values such as SQL credentials to be injected securely at deployment time.
