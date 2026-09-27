targetScope = 'subscription'

@description('Resource group name.')
param name string

@description('Azure region for the resource group.')
param location string

@description('Tags to apply to the resource group.')
param tags object = {}

resource resourceGroup 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: name
  location: location
  tags: tags
}

output resourceGroupId string = resourceGroup.id
output resourceGroupName string = resourceGroup.name
