targetScope = 'resourceGroup'

@description('SQL Server name.')
param name string

@description('Azure region for the SQL Server.')
param location string

@secure()
@description('SQL administrator login.')
param administratorLogin string

@secure()
@description('SQL administrator password.')
param administratorPassword string

@description('Minimum TLS version.')
param minimumTlsVersion string = '1.2'

@description('Public network access setting.')
@allowed([
  'Enabled'
  'Disabled'
])
param publicNetworkAccess string = 'Disabled'

@description('Tags to apply to the SQL Server.')
param tags object = {}

resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: name
  location: location
  properties: {
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorPassword
    minimalTlsVersion: minimumTlsVersion
    publicNetworkAccess: publicNetworkAccess
  }
  tags: tags
}

output sqlServerId string = sqlServer.id
output sqlServerName string = sqlServer.name
output fullyQualifiedDomainName string = sqlServer.properties.fullyQualifiedDomainName
