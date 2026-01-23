# copilot-instructions.md
> Project: Order API (DDD + CQRS)  
> Purpose: Make GitHub Copilot generate consistent, domain-pure code and TDD-first tests.

## 1) Core rules (must follow)
1. **Domain purity is non-negotiable**
   - Domain layer has **no** EF Core, DbContext, HTTP, logging, DI, IConfiguration, ServiceBus, caching, DateTime.Now (use IClock), etc.
   - Domain depends only on Domain types + .NET base libraries.

2. **TDD workflow**
   - Always start with **RED** test, then minimal **GREEN** code, then **REFACTOR**.
   - Prefer **behavior-focused** tests over implementation details.

3. **Public API is intention-revealing**
   - Domain methods should read like business language: `Create`, `AddItem`, `RemoveItem`, `Confirm`, `Cancel`, `Pay`, `Ship`.
   - Keep setters private; mutate state via methods.

4. **Errors**
   - Use `DomainException` for invariant violations.
   - Exception messages should be stable and meaningful (but tests should not overfit on full message strings).

5. **Domain Events**
   - Raise domain events when a meaningful business event occurs (e.g., `OrderCreated`, `OrderConfirmed`, `OrderCancelled`).
   - Do **not** publish events in Domain. Only collect them.

6. **Code comments**
   - If comments are required, write them in **English** only.

---

## 2) Architecture context (what Copilot should assume)
- **Domain layer**
  - Contains: Aggregates, Entities, Value Objects, Domain Events, Domain Exceptions, Domain Services (rare).
  - No external dependencies.

- **Application layer**
  - CQRS handlers (MediatR), validation, orchestration, transaction boundary via UoW.
  - Uses repositories/interfaces defined in Domain/Application.

- **Infrastructure layer**
  - EF Core mappings, repositories, messaging, outbox, migrations.

- **API layer**
  - Minimal API / Controllers, DTOs, auth, serialization, ProblemDetails.

---

## 3) Testing strategy by layer
### 3.1 Domain.Tests (Unit tests)
- **Framework**: xUnit
- **Assertions**: FluentAssertions
- **No mocking** inside Domain tests (pure in-memory).
- Test categories:
  - Aggregate invariants
  - Value Object equality and validation
  - State transitions
  - Totals and calculations
  - Domain events raised/not raised

**Naming convention**
- `Method_WhenCondition_ShouldExpectedResult`
  - Example: `Confirm_WhenOrderHasNoItems_ShouldThrowDomainException`

**Test style**
- Arrange / Act / Assert
- Prefer builders: `OrderBuilder`, `MoneyBuilder`

### 3.2 Application.Tests (mostly unit tests)
- Mock repository, UoW, external services, event bus.
- Assert decisions: `Save`, `Commit`, `Publish` invoked (or not).

### 3.3 Infrastructure.Tests (integration)
- Use Testcontainers when possible.
- Validate EF mapping, migrations, repository queries, outbox behavior.

### 3.4 Api.Tests (integration)
- Use `WebApplicationFactory`.
- Verify HTTP behavior + ProblemDetails + auth.

---

## 4) Domain test patterns Copilot must use
### 4.1 Builders
- Prefer a minimal builder per aggregate:
  - `OrderBuilder.WithItem(...)`
  - `OrderBuilder.WithStatus(...)`
  - `OrderBuilder.Build()`

### 4.2 Avoid brittle assertions
- Prefer:
  - `act.Should().Throw<DomainException>();`
- Avoid:
  - `WithMessage("exact full string")` (unless message is a known contract)

### 4.3 Verify events
- If aggregate collects events:
  - Assert that event type exists once, or not exists.
- Do not assert exact timestamps unless deterministic clock is used.

---

## 5) Domain design guidelines (what Copilot should generate)
### 5.1 Aggregates
- Keep invariants inside aggregate methods.
- Use private collections + read-only exposures:
  - `private readonly List<OrderItem> _items = new();`
  - `public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();`

### 5.2 Value Objects
- Immutable, value-based equality.
- Validate in constructor/factory.
- Provide factory methods when validation is needed:
  - `Money.Of(decimal amount, string currency)`

### 5.3 IDs
- Prefer strongly-typed IDs (`OrderId`) rather than raw `Guid` if already used.
- Ensure equals/hashcode.

### 5.4 Time
- Domain should not use `DateTime.UtcNow` directly if time matters.
- Prefer passing time in, or use `IClock` at application layer.

---

## 6) Anti-patterns (Copilot must NOT do)
- ❌ Add EF attributes (`[Key]`, `[Column]`) in Domain
- ❌ Add public setters to entities for convenience
- ❌ Put validation in API only; domain invariants must be enforced in Domain
- ❌ Use `dynamic`, reflection, or magic strings for business rules
- ❌ Over-test private methods
- ❌ Add new features while making tests pass (keep minimal)

---

## 7) Prompt templates (use in Copilot Chat)
### Template A — Generate RED tests from a rule
Paste the rule + relevant domain code:
