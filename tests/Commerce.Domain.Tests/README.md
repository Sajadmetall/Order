# Commerce Domain Unit Tests

This xUnit project verifies the business rules of the Commerce bounded context. It references `Commerce.Domain` directly and runs against real domain objects in memory. It requires only the .NET 10 SDK.

The [Commerce Test Plan](../../TestPlan.md) defines the project-wide testing approach.

## What belongs here

| Area | What the current tests verify |
|---|---|
| Orders | Order creation, item addition and the rules for confirming and shipping an order |
| Order settlement | When an order can become paid, how the verified amount and currency must match the order total, and how a repeated settlement is handled |
| Payments | Payment attempts, failed attempts followed by retries, preservation of attempt history, and handling of repeated or conflicting results |
| Money | Construction, rejection of negative amounts and missing currencies, and equality by amount and currency |

Use this project when the expected result can be observed through the domain model itself. For example, the rule that a payment rejects a mismatched currency belongs here. Verifying that a rejected settlement leaves the stored order and outbox unchanged belongs in [Commerce Integration Tests](../Commerce.Integration.Tests/README.md).

## Project layout

| Folder | Contents |
|---|---|
| `Aggregates/` | Tests for `Order` and `Payment`. Order settlement tests also live here because they exercise the `Order` aggregate. |
| `ValueObjects/` | Tests for shared value objects such as `Money`. |
| `Builders/` | Reusable setup such as `OrderBuilder`, which creates orders through their public domain methods. |
| `TestDoubles/` | Test identifier helpers. |

Exercise an entity through its aggregate root when the root controls its lifecycle. Add an `Entities/` folder when a separate entity test has a clear purpose.

## Run the tests

From the repository root:

```bash
dotnet test tests/Commerce.Domain.Tests/Commerce.Domain.Tests.csproj -c Release
```

To run only the Payment aggregate tests:

```bash
dotnet test tests/Commerce.Domain.Tests/Commerce.Domain.Tests.csproj -c Release \
  --filter "FullyQualifiedName~PaymentTests"
```

The solution-wide GitHub Actions test job also runs this project.

## Add a test

Use the aggregate's public methods to arrange the starting state, perform one meaningful action, and assert the resulting state or rejection. Use `OrderBuilder` for common setup and keep values that determine the outcome visible in the test. Use an xUnit `[Theory]` when several inputs exercise the same rule. For rejected operations, check that the relevant state remains unchanged when that is part of the rule.
