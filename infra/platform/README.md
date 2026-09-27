# Platform

This layer manages Azure governance and hierarchy concerns that sit above any individual application workload.

- `main.bicep` orchestrates management group creation and parent-child hierarchy.
- `modules/` contains reusable governance modules such as management group creation and subscription assignment.
- `params/` contains environment-specific configuration for `dev` and `prod`.

Use this layer to establish the management group structure first, then deploy workload stacks into the appropriate subscriptions.
