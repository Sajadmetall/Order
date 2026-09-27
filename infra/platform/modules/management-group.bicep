targetScope = 'tenant'

@description('The unique name of the management group.')
param managementGroupName string

@description('The display name of the management group.')
param displayName string

@description('The resource ID of the parent management group. Leave empty to create under the tenant root group.')
param parentManagementGroupId string = ''

resource managementGroup 'Microsoft.Management/managementGroups@2023-04-01' = {
  name: managementGroupName
  properties: {
    displayName: displayName
    details: empty(parentManagementGroupId) ? null : {
      parent: {
        id: parentManagementGroupId
      }
    }
  }
}

output managementGroupId string = managementGroup.id
output managementGroupName string = managementGroup.name
