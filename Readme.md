# Commerce — DDD & CQRS Reference Project

This repository contains a **production-oriented Order Service** built as a learning and portfolio project, with a strong focus on **clean architecture, Domain-Driven Design (DDD), and CQRS**.

The goal of this project is not to build a “toy app”, but a **realistic backend service** that demonstrates how I design, implement, test, and evolve a system in a professional environment.

Explore the interactive [Commerce Domain Atlas](https://commerce-domain-atlas.sajadre.chatgpt.site) for the domain model, order and payment workflows, and test coverage.

---

## 🎯 Project Goals

- Apply **Domain-Driven Design** in a pragmatic, non-overengineered way  
- Separate **write and read concerns** using CQRS  
- Keep the **domain model clean and persistence-agnostic**
- Use **automated tests** to protect business rules
- Prepare the system for **cloud-native deployment on Azure**

---

## 🏗 Architecture Overview

The solution follows a layered architecture inspired by **DDD + Clean Architecture**.

| Path | Responsibility |
| --- | --- |
| [`src/Domain`](src/Domain) | Order and Payment aggregates under `Orders` and `Payments`, with shared types such as `Money` in `Common`. |
| [`src/Application`](src/Application) | MediatR commands and queries under `Features`, plus persistence and messaging abstractions. |
| [`src/Infrastructure`](src/Infrastructure) | SQL Server, EF Core mappings and migrations, MassTransit Outbox and RabbitMQ. |
| [`src/Commerce.Api`](src/Commerce.Api) | HTTP endpoints, request handling and application startup. |
| [`tests`](tests) | Domain unit tests and integration tests. |
| [`infra`](infra) | Azure Bicep templates and deployment scripts. CI validates Bicep. |

### Key Architectural Decisions

- **Domain layer has no dependency** on EF Core or infrastructure concerns  
- **CQRS** is implemented using command/query separation  
- Reads and writes use `IApplicationDbContext`. EF Core commits changes to Orders, Payments and the Outbox in one transaction.
- EF Core mappings are isolated using `IEntityTypeConfiguration`  
- Value Objects (e.g. `Money`) enforce business invariants at creation time  

---

## Commerce domain

Commerce is one bounded context. The Domain project organizes its model into
`Commerce.Domain.Orders` and `Commerce.Domain.Payments`, with `Money` shared
inside that context. Orders owns the existing `Order` aggregate and its items.
The separate `Payment` aggregate references the order by `OrderId` and tracks
payment attempts.

The order can progress from `Created` to `Confirmed`, `Paid`, and `Shipped`.
The application has internal commands for confirmation, payment preparation,
starting and failing attempts, settlement, and shipment. Only a paid order can
ship. Replaying the same verified settlement preserves shipment. The initial
settlement records one purchase outcome.

`SettlePaymentCommand` accepts a trusted payment outcome. The handler commits the
order and payment changes alongside `OrderPaidMessage` in the MassTransit EF Bus
Outbox. MassTransit delivers the recorded message to RabbitMQ after the commit.
The public HTTP API supports order creation and retrieval. Provider checkout,
callback verification, and public payment endpoints are future work.

## 🧠 Domain Model Highlights

- `Order` as an **Aggregate Root**
- `OrderItem` as a child entity
- `Money` as a **Value Object** with validation rules
- Strongly typed IDs (e.g. `OrderId`) to avoid primitive obsession

Example domain rule:
- An order cannot be created with invalid monetary values
- Invalid states are prevented **by design**, not by validation later

---

## 🧪 Testing Strategy

The project includes **unit tests for the domain layer**, focusing on business correctness:

- **xUnit** as test framework  
- **FluentAssertions** for expressive assertions  

Test coverage includes:
- Value Objects validation (`Money`)
- Domain behavior and invariants
- Constructor and factory method rules

The intention is:
> *Test business rules, not implementation details.*

---

## 🛠 Tech Stack

- **.NET** (latest LTS)
- **C#**
- **Entity Framework Core**
- **SQL Server** for persistence
- **MediatR** (CQRS)
- **MassTransit 8.5.12**, **EF Bus Outbox** and **RabbitMQ** for messaging
- **Docker Compose** for local development and container smoke tests
- **Azure Bicep** for infrastructure definitions under `infra/`
- **xUnit**
- **FluentAssertions**
- **REST API**

---

## 🚀 Running the Project Locally

### With Docker Compose

Use Docker in Linux container mode with Compose v2. The SQL Server image is for
x86-64 hosts. Run the commands from the repository root, using the root
`compose.yaml` and `Dockerfile`. Copy the root `.env.example` to `.env` and
set both passwords before starting the stack. Use separate local-only
passwords. A prefix of `Aa1-` followed by at least 20 random hexadecimal
characters works for the SQL password. Keep `.env` out of source control.

```bash
docker compose config --quiet
docker compose up --build --wait
```

`config --quiet` validates the configuration without printing the resolved
values. `up --build --wait` builds and starts the stack.

Open [Swagger UI](http://localhost:8080/swagger/index.html). Change
`COMMERCE_API_PORT` in `.env` if port 8080 is already in use.

The multi-stage Dockerfile builds and publishes the API with the .NET 10 SDK
and runs it as a non-root user in the ASP.NET Core runtime image. Compose builds
this image as `commerce-api:local` and starts it alongside SQL Server 2022
Developer Edition and RabbitMQ. At startup, the Development API applies
pending EF Core migrations and seeds missing sample data.

| Setting in `.env` | How Compose uses it |
| --- | --- |
| `COMMERCE_SQL_PASSWORD` | Sets the SQL container's `MSSQL_SA_PASSWORD` and supplies the same value in the API's `ConnectionStrings__DefaultConnection`. The API connects to `sqlserver,1433`, database `OrderDb`. |
| `COMMERCE_RABBITMQ_PASSWORD` | Sets `RABBITMQ_DEFAULT_PASS` for the broker's `commerce` user and supplies the same value through `RabbitMq__Password` in the API. |
| `COMMERCE_API_PORT` | Publishes the API on the host loopback interface. It defaults to 8080 and maps to port 8080 inside the API container. |

Compose injects these settings at runtime. The Docker build excludes `.env`.
The API reads its connection string as `ConnectionStrings:DefaultConnection`
through the standard ASP.NET Core configuration provider.

Compose waits for its dependencies before starting the API. The SQL Server
health check runs `SELECT 1` through `sqlcmd`, and the RabbitMQ health check
runs `rabbitmq-diagnostics -q check_port_connectivity`. Both run every five
seconds. The API's `depends_on` entries use `condition: service_healthy`.

After changing the API code, run `docker compose up --build --wait api` to
rebuild its image and replace the API container when the image changes.

Run `docker compose down` to stop the stack and keep the SQL data. To reset a
disposable local database, use `docker compose down --volumes`, which deletes its
data. Keep the SQL password the same while reusing an existing SQL data volume.

### Container smoke test in CI

The [`container-smoke` job](.github/workflows/ci.yml) generates temporary
passwords and starts the root Compose stack on the GitHub Actions runner. It
waits for the API's Swagger endpoint, then sends `POST /api/Order` for two items
at EUR 6.50 each and checks for HTTP 201 and an order ID. It reads the order back
with `GET /api/Order/{id}` and checks its ID, customer, item, quantity, currency
and EUR 13 total. CI prints container logs on failure and removes the temporary
containers and volumes after the run.

The xUnit [integration suite](tests/Commerce.Integration.Tests/README.md) uses
its own SQL Server and RabbitMQ instances through Testcontainers and covers
payment behavior and Outbox delivery separately.

### With the .NET SDK

Set `ConnectionStrings__DefaultConnection` for your local SQL Server before
running the API. Configure `RabbitMq__Host`, `RabbitMq__Username` and
`RabbitMq__Password` for your broker when it differs from the local defaults.
The local SQL Express and RabbitMQ credential defaults are available in
`Development` only. Other environments must explicitly configure the SQL
connection string and RabbitMQ host, username and password before startup.

1. Clone the repository
2. Restore dependencies
3. Apply EF Core migrations
4. Run the API project

```bash
dotnet restore Commerce.slnx
dotnet build Commerce.slnx --no-restore
dotnet test Commerce.slnx --no-build
dotnet run --project src/Commerce.Api/Commerce.Api.csproj
