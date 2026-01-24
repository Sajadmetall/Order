# Test Plan — Order API

## 1. Purpose
This document defines what we test, how we test it, and the expectations from tests for the Order API project.
Goals:
- Protect business rules (DDD domain invariants) with fast and reliable tests.
- Validate critical integrations (DB, messaging, outbox) with targeted integration tests.
- Validate observable API behavior (HTTP contracts, auth, validation) with API-level tests.
- Keep CI fast, deterministic, and maintainable.

## 2. Scope
### In scope
- Domain (Aggregates, Entities, Value Objects, Domain Events, Domain Exceptions)
- Application (CQRS handlers, validation, orchestration, transaction boundaries)
- Infrastructure (EF Core persistence, repository implementations, migrations, outbox/messaging)
- API (endpoints/controllers, DTO mapping, authn/authz, ProblemDetails)

### Out of scope (for now)
- UI testing
- Full performance/load testing (can be added later as a separate plan)
- Penetration testing (basic security checks only)
- Third-party provider behavior (covered via contract assumptions + stubs)

## 3. Quality Attributes (Expectations)
- **Correctness:** domain invariants and state transitions are enforced.
- **Reliability:** tests should be stable (no flaky timing/network dependencies).
- **Maintainability:** tests are readable, follow naming conventions, and use builders.
- **Speed:** unit tests should run in seconds; integration tests should be limited and targeted.
- **Observability:** API errors follow consistent ProblemDetails responses.

## 4. Test Levels & Approach
We follow a test pyramid approach:
- Many **Unit Tests** (Domain, Application decision logic)
- Some **Integration Tests** (Persistence/messaging boundaries)
- Few **API Integration / BDD-style tests** (critical user journeys)

### 4.1 Domain Layer (Unit Tests, TDD-first)
**Primary goal:** protect business rules and behavior.
- No database, no network, no EF Core.
- No mocking (pure in-memory domain objects).
- Verify:
  - invariants (e.g., invalid quantity, invalid transitions)
  - calculations (totals, money arithmetic)
  - state transitions (OrderStatus)
  - domain events raised/not raised (if implemented)

**Expected coverage:**
- High coverage of domain rules, not a numeric target.
- Every business rule must have at least one unit test.

### 4.2 Application Layer (Mostly Unit Tests)
**Primary goal:** validate orchestration and decisions.
- Mock dependencies (repositories, UoW, external services, message bus).
- Verify:
  - correct calls (save/update, commit/rollback decisions)
  - validation behavior (reject invalid commands)
  - idempotency decisions if applicable
  - domain events are handled or persisted (if outbox is used)

**Expected coverage:**
- Focus on critical paths and decision logic, not on trivial mapping.

### 4.3 Infrastructure Layer (Integration Tests)
**Primary goal:** validate real integration with external dependencies.
- EF Core mapping + migrations + repository behavior.
- Prefer Testcontainers (SQL Server/Postgres) for fidelity.
- Verify:
  - migrations apply successfully
  - repository CRUD + important queries
  - constraints (unique indexes, FK behavior, concurrency)
  - outbox behavior (if implemented)

**Note:** Avoid duplicating EF Core’s own tests; focus on our mapping/queries.

### 4.4 API Layer (API Integration Tests, BDD-style)
**Primary goal:** validate observable system behavior via HTTP.
- Use WebApplicationFactory (in-memory host) + real DI.
- Verify:
  - happy paths for critical endpoints
  - validation errors (400) with ProblemDetails
  - authn/authz (401/403) for protected endpoints
  - response shape (contract) and status codes
  - idempotency keys (if implemented)
- Tests are “BDD-style” in naming and structure: Given/When/Then mindset.

## 5. Test Types
### 5.1 Functional Tests
- D
