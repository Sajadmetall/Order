# Workload Modules

Reusable workload modules live here and each module owns a single Azure resource type.

- `rg.bicep` creates the resource group at subscription scope.
- `app-service-plan.bicep` provisions the shared compute plan for the API.
- `app-service.bicep` deploys the .NET API host in Azure App Service.
- `sql-server.bicep` and `sql-database.bicep` provision the relational data tier.
- `log-analytics.bicep` and `app-insights.bicep` add observability resources.

Each module accepts parameters, returns outputs, and is safe to compose from higher-level workload entrypoints.
