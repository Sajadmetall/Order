targetScope = 'tenant'

@description('Configuration for the management group hierarchy.')
param managementGroups array

@description('Subscription assignments to management groups.')
param subscriptionAssignments array = []

module managementGroupModules './modules/management-group.bicep' = [for mg in managementGroups: {
  name: 'mg-${uniqueString(mg.name)}'
  params: {
    managementGroupName: mg.name
    displayName: mg.displayName
    parentManagementGroupId: contains(mg, 'parent') && !empty(string(mg.parent))
      ? tenantResourceId('Microsoft.Management/managementGroups', string(mg.parent))
      : ''
  }
}]

module subscriptionAssignmentModules './modules/management-group-subscription-assignment.bicep' = [for assignment in subscriptionAssignments: {
  name: 'sub-assignment-${uniqueString(assignment.subscriptionId, assignment.managementGroupName)}'
  params: {
    managementGroupName: assignment.managementGroupName
    subscriptionId: assignment.subscriptionId
  }
  dependsOn: [
    managementGroupModules
  ]
}]

output managementGroupIds array = [for (mg, i) in managementGroups: managementGroupModules[i].outputs.managementGroupId]
output subscriptionAssignmentIds array = [for (assignment, i) in subscriptionAssignments: subscriptionAssignmentModules[i].outputs.assignmentId]
