targetScope = 'resourceGroup'

@description('Web app name.')
param name string

@description('Azure region for the web app.')
param location string

@description('Resource ID of the App Service plan.')
param serverFarmResourceId string

@description('Linux runtime stack, for example DOTNETCORE|8.0.')
param runtimeStack string = 'DOTNETCORE|8.0'

@description('Whether HTTPS-only is enforced.')
param httpsOnly bool = true

@description('Whether Always On is enabled.')
param alwaysOn bool = true

@description('Application settings for the web app.')
param appSettings object = {}

@description('Tags to apply to the web app.')
param tags object = {}

var formattedAppSettings = [
  for key in items(appSettings): {
    name: key.key
    value: string(key.value)
  }
]

resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: name
  location: location
  kind: 'app,linux'
  properties: {
    serverFarmId: serverFarmResourceId
    httpsOnly: httpsOnly
    siteConfig: {
      linuxFxVersion: runtimeStack
      alwaysOn: alwaysOn
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
      appSettings: formattedAppSettings
    }
  }
  tags: tags
}

output appServiceId string = webApp.id
output defaultHostName string = webApp.properties.defaultHostName
