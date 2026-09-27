targetScope = 'resourceGroup'

@description('SQL Server name hosting the database.')
param serverName string

@description('SQL Database name.')
param databaseName string

@description('Azure region for the SQL Database.')
param location string

@description('Database SKU name.')
param skuName string

@description('Maximum database size in bytes.')
param maxSizeBytes int

@description('Whether the database is zone redundant.')
param zoneRedundant bool = false

@description('Tags to apply to the SQL Database.')
param tags object = {}

resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' existing = {
  name: serverName
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  parent: sqlServer
  name: databaseName
  location: location
  sku: {
    name: skuName
  }
  properties: {
    maxSizeBytes: maxSizeBytes
    zoneRedundant: zoneRedundant
    readScale: 'Disabled'
    requestedBackupStorageRedundancy: zoneRedundant ? 'Zone' : 'Local'
  }
  tags: tags
}

output sqlDatabaseId string = sqlDatabase.id
output sqlDatabaseName string = sqlDatabase.name
