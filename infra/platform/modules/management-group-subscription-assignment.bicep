targetScope = 'tenant'

@description('The name of the target management group.')
param managementGroupName string

@description('The subscription ID to associate with the management group.')
param subscriptionId string

resource managementGroup 'Microsoft.Management/managementGroups@2023-04-01' existing = {
  name: managementGroupName
}

resource subscriptionAssociation 'Microsoft.Management/managementGroups/subscriptions@2021-04-01' = {
  name: subscriptionId
  parent: managementGroup
}

output assignmentId string = subscriptionAssociation.id
output targetManagementGroupId string = managementGroup.id
