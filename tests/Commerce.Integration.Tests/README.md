# Commerce Integration Tests

This xUnit project verifies that the Commerce API, application handlers, domain model, SQL Server persistence, and MassTransit messaging work together. `WebApplicationFactory` hosts the ASP.NET Core pipeline in process. Testcontainers starts disposable SQL Server and RabbitMQ instances.

The [Commerce Test Plan](../../TestPlan.md) defines the project-wide testing approach. See [Commerce Domain Unit Tests](../Commerce.Domain.Tests/README.md) for isolated business rules and their boundary cases.

## Where each test enters the application

| Boundary | How the suite exercises it |
|---|---|
| Order HTTP API | Creates and reads orders through `POST /api/Order` and `GET /api/Order/{id}`. |
| Payment application workflow | Sends payment commands through the registered MediatR handlers in fresh DI scopes. Settlement receives a synthetic, already verified payment result. |
| SQL Server and EF Core | Applies the actual migrations and checks committed state, rollback, optimistic concurrency, and preservation of existing order data on upgrade. |
| MassTransit and RabbitMQ | Checks that settlement saves an outgoing `OrderPaidMessage` with the Order and Payment changes, and that the saved message reaches RabbitMQ after an application restart. |

The HTTP coverage currently concerns Orders. Provider callback verification and Flash Cards entitlement activation are separate integration boundaries and remain future work.

## Project layout

| File | Responsibility |
|---|---|
| `Fixtures/CommerceFixture.cs` | Starts and disposes SQL Server and RabbitMQ. The tests in each class share that class's container instances. |
| `Fixtures/CommerceApiFactory.cs` | Starts the API in the `Testing` environment and supplies the test database and broker settings. |
| `Fixtures/PurchaseTestBase.cs` | Creates and migrates a unique database for each test, provides HTTP and MediatR helpers, and cleans up the host and database. |
| `Workflows/PurchaseWorkflowTests.cs` | Covers purchase behavior across Orders and Payments, including failed attempts, retries, repeated results, and rollback on a rejected write. |
| `Workflows/PersistenceSafetyTests.cs` | Covers stale writes and migration from the initial schema with existing order data. |
| `Workflows/OutboxDeliveryTests.cs` | Covers message persistence across an application restart, actual delivery to RabbitMQ, and outbox cleanup. |

## How the Outbox tests work

Settlement uses the scoped `IPublishEndpoint` through `IEventPublisher`. MassTransit EF Bus Outbox stores `OrderPaidMessage` in the same SQL transaction as the Order and Payment updates. MassTransit handles delivery after the transaction commits. This branch pins MassTransit to `8.5.12`.

Tests that inspect committed outbox rows pause the delivery service so those rows remain available for assertions. The delivery test starts a subscriber on a unique RabbitMQ queue, restarts the application against the same database with delivery enabled, then verifies the received message and outbox cleanup.

Replaying an accepted settlement and receiving duplicate broker deliveries are distinct cases. The suite checks that replaying settlement does not create another outgoing message. Broker delivery is at least once, so a downstream consumer must handle repeated messages safely.

## Run the tests

Install the .NET 10 SDK and start a Docker-compatible runtime with access to the container images. The fixture supplies connection settings and temporary credentials.

From the repository root:

```bash
dotnet test tests/Commerce.Integration.Tests/Commerce.Integration.Tests.csproj -c Release
```

To run only the Outbox delivery test:

```bash
dotnet test tests/Commerce.Integration.Tests/Commerce.Integration.Tests.csproj -c Release \
  --filter "FullyQualifiedName~OutboxDeliveryTests"
```

GitHub Actions runs this project as part of the solution-wide test job. An unavailable Docker runtime or image pull failure fails fixture setup. When setup fails, check container startup and readiness first, then migration and host startup logs.

## Add a test

Choose the entry point that matches the behavior you want to verify. Use HTTP for an API contract, MediatR for application workflow behavior, independent database scopes for concurrency, and a RabbitMQ subscriber for delivery.

Use synthetic data and verify committed results through a fresh scope or an HTTP read. For a rollback test, check the stored business state and any outgoing message affected by the failed operation. For asynchronous delivery, wait for an observable condition with a timeout. Keep each test independent of execution order and keep migration experiments inside the test's disposable database.
