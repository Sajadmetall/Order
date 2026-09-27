targetScope = 'resourceGroup'

@description('Application Insights component name.')
param name string

@description('Azure region for the component.')
param location string

@description('Resource ID of the linked Log Analytics workspace.')
param workspaceResourceId string

@description('Tags to apply to the component.')
param tags object = {}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: name
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Flow_Type: 'Bluefield'
    WorkspaceResourceId: workspaceResourceId
    IngestionMode: 'LogAnalytics'
  }
  tags: tags
}

output appInsightsId string = appInsights.id
output connectionString string = appInsights.properties.ConnectionString
