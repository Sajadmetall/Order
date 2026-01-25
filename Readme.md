# Order Service – DDD & CQRS Sample

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

1. Clone the repository
2. Restore dependencies
3. Apply EF Core migrations
4. Run the API project

```bash
dotnet restore
dotnet ef database update
dotnet run
