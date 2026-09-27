targetScope = 'resourceGroup'

@description('App Service plan name.')
param name string

@description('Azure region for the App Service plan.')
param location string

@description('SKU name, for example B1 or P1v3.')
param skuName string

@description('SKU tier, for example Basic or PremiumV3.')
param skuTier string

@description('Instance count.')
param capacity int = 1

@description('Tags to apply to the App Service plan.')
param tags object = {}

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: name
  location: location
  sku: {
    name: skuName
    tier: skuTier
    size: skuName
    family: ''
    capacity: capacity
  }
  kind: 'linux'
  properties: {
    reserved: true
    hyperV: false
    perSiteScaling: false
    zoneRedundant: false
    elasticScaleEnabled: false
    maximumElasticWorkerCount: 1
  }
  tags: tags
}

output appServicePlanId string = appServicePlan.id
output appServicePlanName string = appServicePlan.name
