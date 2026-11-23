# Commerce integration tests

The suite exercises the real ASP.NET Core host, MediatR pipeline, domain model,
EF Core mappings, migrations and a disposable SQL Server instance. Order creation
and reads use the HTTP API. Payment commands use the application's registered
MediatR handlers because payment-provider endpoints are still to be designed.

## Run

Install .NET 10 and start a Docker-compatible container runtime, then run from the
repository root.

```bash
dotnet test tests/Commerce.Integration.Tests/Commerce.Integration.Tests.csproj -c Release
```

The existing solution-wide CI test command also runs this project. Missing Docker
causes the integration suite to fail during fixture setup so that required
coverage cannot silently disappear.

## Covered workflow

An order with four items at EUR 2.50 each is created through the API. Preparing
payment confirms the order and records the expected EUR 10 amount. The first
attempt fails. A second attempt succeeds, marking the order Paid and recording
one OrderPaidMessage in OrderPaidOutbox in the same SaveChanges transaction.

Additional scenarios cover repeated preparation, amount and currency mismatch,
replayed and conflicting success results, attempts belonging to another payment,
a second pending attempt, unknown orders, stale writers, transaction rollback and
upgrading an existing database while preserving the original order and items.

The settlement command accepts a trusted, already-verified provider outcome. A
provider adapter must authenticate and verify that outcome before dispatching it.
The suite proves persistence of the outgoing purchase outcome. Sending it to
Flash Cards and activating an entitlement require an outbox dispatcher and a
product-side consumer.

## Test conventions

- Use SQL Server, matching the application's database provider. The container
  image and NuGet versions are pinned.
- Share the expensive SQL Server container through an xUnit class fixture. Give
  every test a unique database and dispose of its host, clients and database.
- Apply actual migrations with MigrateAsync. Include an upgrade test from the
  initial schema to protect existing data.
- Use synthetic data and container-generated credentials. Connection settings
  always point to the disposable container.
- Resolve production handlers from the real host. Use a fresh DI scope for each
  command and verify persisted state through another scope or an HTTP read.
- Assert observable outcomes and database consistency. Unit tests cover the
  detailed domain-rule permutations.
- Exercise duplicate delivery, failed writes and stale writers explicitly.
  Database uniqueness and optimistic concurrency prevent duplicate outcomes.
- Allow tests to run without depending on their order. Await operations and use
  the container's readiness checks instead of fixed sleeps.

A concurrency or uniqueness exception aborts that unit of work. A retry must use
an independent scope and reload persisted state. Repeating a committed success
with the same attempt and provider reference returns successfully without another
outbox entry.

## References

- [ASP.NET Core integration tests](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-10.0)
- [EF Core testing strategy](https://learn.microsoft.com/en-us/ef/core/testing/choosing-a-testing-strategy)
- [Testcontainers SQL Server module](https://dotnet.testcontainers.org/modules/mssql/)
