# Commerce — DDD & CQRS Reference Project

This repository contains a **production-oriented Order Service** built as a learning and portfolio project, with a strong focus on **clean architecture, Domain-Driven Design (DDD), and CQRS**.

The goal of this project is not to build a “toy app”, but a **realistic backend service** that demonstrates how I design, implement, test, and evolve a system in a professional environment.

---

## 🎯 Project Goals

- Apply **Domain-Driven Design** in a pragmatic, non-overengineered way  
- Separate **write and read concerns** using CQRS  
- Keep the **domain model clean and persistence-agnostic**
- Use **automated tests** to protect business rules
- Prepare the system for **cloud-native deployment on Azure**

---

## 🏗 Architecture Overview

The solution follows a layered architecture inspired by **DDD + Clean Architecture**:

/src
├── Domain
│ ├── Entities
│ ├── ValueObjects
│ ├── Aggregates
│ └── Domain Rules & Invariants
│
├── Application
│ ├── Commands
│ ├── Queries
│ ├── CommandHandlers
│ ├── QueryHandlers
│ └── Abstractions (Interfaces)
│
├── Infrastructure
│ ├── Persistence (EF Core)
│ ├── Entity Configurations
│ └── Repositories
│
└── Api
└── REST Endpoints

### Key Architectural Decisions

- **Domain layer has no dependency** on EF Core or infrastructure concerns  
- **CQRS** is implemented using command/query separation  
- **Repositories** are used only on the write side  
- EF Core mappings are isolated using `IEntityTypeConfiguration`  
- Value Objects (e.g. `Money`) enforce business invariants at creation time  

---

## Commerce domain

Commerce is one bounded context. The Domain project organizes its model into
`Commerce.Domain.Orders` and `Commerce.Domain.Payments`, with `Money` shared
inside that context. Orders owns the existing `Order` aggregate and its items.
The separate `Payment` aggregate references an order by `OrderId` and models
failed and successful attempts. The payment module currently runs as a domain
model suitable for simulated scenarios.

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
- **MediatR** (CQRS)
- **xUnit**
- **FluentAssertions**
- **REST API**

---

## 🚀 Running the Project Locally

### With Docker Compose

Use Docker in Linux container mode with Compose v2. The SQL Server image is for
x86-64 hosts. Copy `.env.example` to `.env` and set both passwords before starting
the stack. Use separate local-only passwords. For the SQL password, a prefix of
`Aa1-` followed by at least 20 random hexadecimal characters provides the
required character classes and works in the connection string. Keep `.env` out
of source control.

```bash
docker compose up --build --wait
```

Open [Swagger UI](http://localhost:8080/swagger/index.html). Change
`COMMERCE_API_PORT` in `.env` if port 8080 is already in use.

Compose runs the .NET 10 API, SQL Server 2022 Developer Edition, and RabbitMQ.
In this local Development setup, the API applies EF Core migrations and seeds
sample data at startup. SQL Server and RabbitMQ are accessible on the Compose
network. The API is published on the host loopback interface.

Run `docker compose down` to stop the stack and keep the SQL data. To reset a
disposable local database, use `docker compose down --volumes`, which deletes its
data. Keep the SQL password the same while reusing an existing SQL data volume.

CI starts a fresh Compose stack, creates an order over HTTP and reads it back
from the running API image. The xUnit integration suite continues to use its
own SQL Server and RabbitMQ instances through Testcontainers.

### With the .NET SDK

Set `ConnectionStrings__DefaultConnection` for your local SQL Server before
running the API. Configure `RabbitMq__Host`, `RabbitMq__Username` and
`RabbitMq__Password` for your broker when it differs from the local defaults.

1. Clone the repository
2. Restore dependencies
3. Apply EF Core migrations
4. Run the API project

```bash
dotnet restore Commerce.slnx
dotnet build Commerce.slnx --no-restore
dotnet test Commerce.slnx --no-build
dotnet run --project src/Commerce.Api/Commerce.Api.csproj
