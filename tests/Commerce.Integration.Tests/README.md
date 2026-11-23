# Commerce integration tests

The suite exercises the real ASP.NET Core host, MediatR pipeline, domain model,
EF Core mappings, migrations, EF Bus Outbox and disposable SQL Server/RabbitMQ instances. Order creation
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
one OrderPaidMessage in the MassTransit EF Bus Outbox in the same SaveChanges
transaction. A separate recovery test restarts the application, receives the
persisted event through RabbitMQ and waits for acknowledged outbox cleanup.

Additional scenarios cover repeated preparation, amount and currency mismatch,
replayed and conflicting success results, attempts belonging to another payment,
a second pending attempt, unknown orders, stale writers, transaction rollback and
upgrading an existing database while preserving the original order and items.

The settlement command accepts a trusted, already-verified provider outcome. A
provider adapter must authenticate and verify that outcome before dispatching it.
MassTransit supplies the outbox dispatcher. Activating an entitlement in Flash
Cards still requires a product-side consumer. Delivery is at least once, so that
consumer must deduplicate messages (for example with its own transactional
consumer outbox/inbox); a single persisted event is not an exactly-once delivery
guarantee.

## Test conventions

- Use SQL Server and RabbitMQ, matching the production database provider and transport. The container
  image and NuGet versions are pinned.
- Share the expensive containers through an xUnit class fixture. Give
  every test a unique database and dispose of its host, clients and database.
- Apply actual migrations before hosted delivery services start. Include an upgrade test from the
  initial schema to protect existing data.
- Use synthetic data and container-generated credentials. Connection settings
  always point to the disposable container.
- Resolve production handlers from the real host. Use a fresh DI scope for each
  command and verify persisted state through another scope or an HTTP read.
- Assert observable outcomes and database consistency. Unit tests cover the
  detailed domain-rule permutations.
- Exercise duplicate delivery, failed writes and stale writers explicitly.
  Database uniqueness and optimistic concurrency prevent duplicate outcomes.
- Pause the real delivery service only while inspecting committed outbox rows;
  enable it for the broker delivery/restart test. Bind the test subscriber before
  publishing, use a unique queue, and bound asynchronous waits with cancellation.
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

## MassTransit version and runtime configuration

MassTransit.EntityFrameworkCore and MassTransit.RabbitMQ are pinned to **8.5.12**,
the latest stable 8.x release checked on 2026-11-23. This version is Apache-2.0;
MassTransit 9 introduces a commercial license and is deliberately not selected.

The scoped IEventPublisher adapter uses IPublishEndpoint. The EF Bus Outbox stores
the message in the same OrderDbContext as the order/payment updates. Do not replace
it with IBus in application handlers because that bypasses the scoped bus outbox.
MassTransit owns OutboxMessage, OutboxState and InboxState; there is no custom
outbox table or delivery loop in the application.

For local development, provide SQL Server and RabbitMQ and override configuration:

- `RabbitMq__Host`, `RabbitMq__Port` and `RabbitMq__VirtualHost` identify the broker.
- `RabbitMq__Username` and `RabbitMq__Password` should come from user secrets or the
  environment. Local defaults are RabbitMQ's guest account on localhost.
- Keep `ConnectionStrings__DefaultConnection` in user secrets or the environment.

Production must provision its broker, durable subscriber queue and credentials
before relying on events. This PR changes no Azure deployment workflow.
The unmerged PR's AddPaymentWorkflow migration was regenerated to create the
MassTransit tables directly. The original InitialCreate migration is unchanged;
the upgrade test verifies existing orders survive. A disposable development
database already using the PR's previous migration should be recreated before
applying this revised migration set.

- [MassTransit 8.5.12 package and license](https://www.nuget.org/packages/MassTransit/8.5.12)
- [MassTransit EF transactional outbox](https://masstransit.io/documentation/configuration/middleware/outbox)
- [Testcontainers RabbitMQ module](https://dotnet.testcontainers.org/modules/rabbitmq/)
