targetScope = 'subscription'

@description('Azure region for all regional resources.')
param location string

@description('Deployment environment name.')
@allowed([
  'dev'
  'prod'
])
param environment string

@description('Application name used for resource naming.')
param appName string = 'order-api'

@description('Tags applied to all supported resources.')
param tags object = {}

@description('Resource group configuration.')
param resourceGroup object

@description('App Service plan configuration.')
param appServicePlan object

@description('Web application configuration.')
param appService object

@description('Azure SQL Server configuration.')
param sqlServer object

@description('Azure SQL Database configuration.')
param sqlDatabase object

@description('Controls whether monitoring resources are deployed.')
param enableMonitoring bool = true

@secure()
@description('SQL administrator login. Pass from pipeline or deployment secret store, not from source control.')
param sqlAdminLogin string

@secure()
@description('SQL administrator password. Pass from pipeline or deployment secret store, not from source control.')
param sqlAdminPassword string

module resourceGroupModule './modules/rg.bicep' = {
  name: 'rg-${uniqueString(resourceGroup.name)}'
  params: {
    name: resourceGroup.name
    location: location
    tags: union(tags, resourceGroup.tags ?? {})
  }
}

resource workloadRg 'Microsoft.Resources/resourceGroups@2024-03-01' existing = {
  name: resourceGroup.name
}

module logAnalyticsModule './modules/log-analytics.bicep' = if (enableMonitoring) {
  name: 'law-${uniqueString(resourceGroup.name, environment)}'
  scope: workloadRg
  params: {
    name: '${appName}-${environment}-law'
    location: location
    retentionInDays: 30
    tags: tags
  }
  dependsOn: [
    resourceGroupModule
  ]
}

module appInsightsModule './modules/app-insights.bicep' = if (enableMonitoring) {
  name: 'appi-${uniqueString(resourceGroup.name, environment)}'
  scope: workloadRg
  params: {
    name: '${appName}-${environment}-appi'
    location: location
    workspaceResourceId: logAnalyticsModule!.outputs.workspaceId
    tags: tags
  }
}

module appServicePlanModule './modules/app-service-plan.bicep' = {
  name: 'plan-${uniqueString(appServicePlan.name)}'
  scope: workloadRg
  params: {
    name: appServicePlan.name
    location: location
    skuName: appServicePlan.skuName
    skuTier: appServicePlan.skuTier
    capacity: appServicePlan.capacity ?? 1
    tags: tags
  }
  dependsOn: [
    resourceGroupModule
  ]
}

module sqlServerModule './modules/sql-server.bicep' = {
  name: 'sql-${uniqueString(sqlServer.name)}'
  scope: workloadRg
  params: {
    name: sqlServer.name
    location: location
    administratorLogin: sqlAdminLogin
    administratorPassword: sqlAdminPassword
    minimumTlsVersion: sqlServer.minimumTlsVersion ?? '1.2'
    publicNetworkAccess: sqlServer.publicNetworkAccess ?? 'Disabled'
    tags: tags
  }
  dependsOn: [
    resourceGroupModule
  ]
}

module sqlDatabaseModule './modules/sql-database.bicep' = {
  name: 'sqldb-${uniqueString(sqlDatabase.name)}'
  scope: workloadRg
  params: {
    serverName: sqlServer.name
    databaseName: sqlDatabase.name
    location: location
    skuName: sqlDatabase.skuName
    maxSizeBytes: sqlDatabase.maxSizeBytes
    zoneRedundant: sqlDatabase.zoneRedundant ?? false
    tags: tags
  }
  dependsOn: [
    sqlServerModule
  ]
}

module appServiceModule './modules/app-service.bicep' = {
  name: 'app-${uniqueString(appService.name)}'
  scope: workloadRg
  params: {
    name: appService.name
    location: location
    serverFarmResourceId: appServicePlanModule.outputs.appServicePlanId
    runtimeStack: appService.runtimeStack ?? 'DOTNETCORE|8.0'
    httpsOnly: appService.httpsOnly ?? true
    alwaysOn: appService.alwaysOn ?? true
    appSettings: union(appService.appSettings ?? {}, enableMonitoring ? {
      APPLICATIONINSIGHTS_CONNECTION_STRING: appInsightsModule!.outputs.connectionString
    } : {})
    tags: tags
  }
}

output resourceGroupName string = workloadRg.name
output appServiceDefaultHostName string = appServiceModule.outputs.defaultHostName
output sqlServerFullyQualifiedDomainName string = sqlServerModule.outputs.fullyQualifiedDomainName
