targetScope = 'resourceGroup'

@description('Log Analytics workspace name.')
param name string

@description('Azure region for the workspace.')
param location string

@description('Retention period in days.')
param retentionInDays int = 30

@description('Tags to apply to the workspace.')
param tags object = {}

resource workspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: name
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: retentionInDays
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
  tags: tags
}

output workspaceId string = workspace.id
output workspaceName string = workspace.name
