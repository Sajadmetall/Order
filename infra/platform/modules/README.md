# Platform Modules

Reusable governance modules live here.

- `management-group.bicep` creates a management group and optionally attaches it to a parent.
- `management-group-subscription-assignment.bicep` associates a subscription with a target management group.

Keep these modules focused on one responsibility each so they can be reused by future platform orchestrators.
