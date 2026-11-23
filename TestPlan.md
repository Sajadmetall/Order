# Test Plan — Commerce

**Status:** Approved
**Last updated:** 2026-11-23
**Scope:** Commerce bounded context — Orders and Payments
**Implementation baseline:** PR #9

## 1. Purpose

This document defines the objectives, scope, approach, resources and acceptance criteria for testing Commerce.

The goals are to:

- Protect domain invariants and valid state transitions.
- Verify collaboration between Application, Domain, Infrastructure and API.
- Validate persistence, transaction boundaries and concurrency controls.
- Verify transactional message persistence and delivery.
- Validate observable API contracts.
- Protect existing behavior through automated regression testing.
- Keep testing repeatable, maintainable and practical for development and CI.

Testing priorities follow business importance and technical risk.

This plan defines how testing is organized. Detailed test cases and execution results are maintained separately.

## 2. Scope

### 2.1 In Scope

| Area | Testing scope |
|---|---|
| Domain | Aggregates, Entities, Value Objects, domain invariants, calculations and state transitions |
| Domain Events | Event collection and handling when implemented |
| Application | CQRS handlers, validation, orchestration, idempotency and transaction boundaries |
| Infrastructure | EF Core mappings, repositories, queries, migrations, constraints and concurrency |
| Messaging | MassTransit EF Bus Outbox, RabbitMQ integration, message contracts and recovery behavior |
| API | Routing, serialization, DTO contracts, HTTP status codes, validation and ProblemDetails |
| Access control | Authentication, authorization and ownership checks when introduced |
| Regression | Continued correctness of agreed and implemented behavior |

Orders and Payments belong to the Commerce bounded context.

Tests must respect aggregate boundaries and the responsibilities of each architectural layer.

### 2.2 Out of Scope

The following are outside the current implementation phase:

- UI and browser testing.
- Complete end-to-end journeys across Commerce and Flash Cards.
- Actual payment processing by a third-party provider.
- Comprehensive performance, load and endurance testing.
- Full penetration testing.
- Production deployment validation and Azure infrastructure acceptance testing.

These exclusions do not remove future requirements. The scope must be revised when the corresponding functionality or release phase is introduced.

Third-party provider behavior is outside our implementation responsibility. Our integration with that provider—including request construction, callback verification and error handling—enters scope when its adapter is implemented.

## 3. Quality Attributes and Expectations

| Attribute | Expectation |
|---|---|
| Correctness | Business rules and valid state transitions are enforced |
| Data integrity | Failed operations do not leave invalid partial changes |
| Reliability | Retries, repeated inputs and technical interruptions are handled consistently |
| Isolation | Tests do not interfere with other tests or shared business data |
| Maintainability | Tests clearly express behavior and remain practical to update |
| Speed | Unit tests provide fast feedback; Integration tests target meaningful boundaries |
| Observability | Failures and execution results provide useful diagnostic information |
| Security | Sensitive information is protected and access rules are enforced at the Backend |

These are testing objectives. Their inclusion does not imply that every attribute is already fully verified.

## 4. Test Levels & Approach

Use a risk-based test pyramid:

- Many fast Unit Tests for domain rules and independent decisions.
- Targeted Integration Tests for persistence, messaging and component collaboration.
- A limited set of API and acceptance tests for important observable behavior.

### 4.1 Domain Layer — Unit Tests, TDD-first

**Primary goal:** Protect business rules independently of technical infrastructure.

**Approach:**

- Use real domain objects.
- Do not use databases, HTTP, message brokers or mocks.
- Exercise aggregates through public business methods.
- Verify entity behavior through aggregate roots where they control the entity lifecycle.
- Verify validation, calculations, equality and state transitions.
- Verify Domain Events when implemented.
- Avoid assertions against private implementation details.

**Expected coverage:**

Every significant domain rule should have direct test coverage. Additional cases should target boundaries and risks rather than increase test counts.

### 4.2 Application Layer — Unit and Integration Tests

**Primary goal:** Verify validation, decisions and orchestration.

**Approach:**

- Use focused Unit Tests for independent decision logic.
- Replace external dependencies with suitable test doubles where isolation is useful.
- Use Integration Tests when correctness depends on EF queries, transactions or concurrency.
- Exercise registered MediatR handlers and relevant pipeline behaviors.
- Verify observable results and meaningful side effects.
- Verify dependency interactions only when they represent an important contract.

**Expected coverage:**

Focus on critical paths, validation, idempotency and transaction decisions. Avoid tests that merely reproduce trivial mapping or implementation steps.

### 4.3 Infrastructure Layer — Integration Tests

**Primary goal:** Verify the application's actual use of persistence and messaging infrastructure.

**Approach:**

- Use disposable SQL Server and RabbitMQ instances.
- Apply actual EF Core migrations.
- Verify mappings, relevant queries and database constraints.
- Verify transaction boundaries and optimistic concurrency.
- Verify outgoing message persistence and delivery.
- Add interruption and recovery checks according to risk.

Tests should verify our mappings, configuration and integration behavior without duplicating framework internals.

### 4.4 API Layer — HTTP Integration Tests

**Primary goal:** Verify externally observable HTTP behavior.

**Approach:**

- Use WebApplicationFactory with real application registrations.
- Verify status codes, headers, serialization and response contracts.
- Verify validation and ProblemDetails responses.
- Verify authentication, authorization and ownership when implemented.
- Verify HTTP idempotency mechanisms when introduced.

Calling Application handlers through MediatR does not establish HTTP contract coverage.

### 4.5 Acceptance and Cross-context Tests

**Primary goal:** Verify that important business outcomes work across participating components or contexts.

Within Commerce, acceptance scenarios can be automated through the appropriate Application or API boundary.

Cross-context tests will be introduced when product-side consumers and fulfillment contracts exist. They must address contract compatibility and repeated message delivery.

### 4.6 TDD Approach

For new business rules and defect fixes, follow:

1. **Red:** Express the expected behavior in a failing test.
2. **Green:** Implement the minimum necessary behavior.
3. **Refactor:** Improve the implementation while retaining passing tests.

A passing existing suite does not prove that its implementation was originally developed through TDD.

### 4.7 BDD Approach

BDD supports shared understanding of expected behavior before implementation.

The approach is to:

- Discuss business rules and concrete examples.
- Agree on acceptance criteria.
- Use Given/When/Then where it improves clarity.
- Map agreed examples to appropriate automated tests.
- Update examples when business behavior changes.

BDD applies across test levels. It is not restricted to API testing.

Automation currently uses xUnit. Introducing executable Gherkin files or an additional BDD framework is not an approved requirement.

## 5. Test Types

### 5.1 Functional Tests

Verify business rules, state transitions and agreed application behavior.

Cover successful and rejected operations according to their business significance.

### 5.2 Regression Tests

Protect established behavior against unintended changes.

Relevant regression tests should accompany defect fixes and changes to existing rules.

### 5.3 Contract Tests

Verify HTTP and message contracts, including required fields, response structure and compatibility expectations.

Expand coverage as external integrations and consumers are introduced.

### 5.4 Persistence Tests

Verify mappings, queries, migrations, constraints and transaction behavior using SQL Server.

### 5.5 Concurrency and Idempotency Tests

Verify that concurrent operations and repeated inputs cannot produce invalid or duplicate business outcomes.

### 5.6 Resilience Tests

Verify recovery from technical interruptions, failed writes and messaging failures according to risk.

Distinguish controlled restart coverage from broker outages and abrupt process failures.

### 5.7 Security Tests

Verify authentication, authorization, ownership isolation, input handling and safe error responses as the relevant functionality is introduced.

### 5.8 Performance Tests

Introduce measurable performance tests when workload assumptions and performance objectives are defined.

Full load and endurance testing remain outside the current phase.

## 6. Test Basis

Testing is based on:

- Agreed business rules and acceptance criteria.
- Commerce architecture and module boundaries.
- Public Domain and Application interfaces.
- HTTP and message contracts.
- Persistence models and supported migration paths.
- Known defects, risks and planned changes.

The implementation describes current behavior. Ambiguous business rules must be clarified before tests establish a new expected behavior.

## 7. Test Design and Organization

### 7.1 Design Techniques

Select techniques according to the rule being tested:

- Equivalence partitioning.
- Boundary value analysis.
- State transition testing.
- Decision tables.
- Positive and negative paths.
- Risk-based selection of failure, repetition and concurrency conditions.

Tests should:

- Have descriptive, behavior-focused names.
- Follow Arrange–Act–Assert or an equivalent Given–When–Then structure.
- Express a clear expected outcome.
- Avoid private implementation details.
- Avoid exact exception-message assertions unless wording is an agreed contract.
- Use builders where they improve clarity and reduce repetitive setup.

### 7.2 Folder Organization

Domain tests retain these categories:

- `Aggregates`
- `Entities`, where independently justified
- `ValueObjects`
- `Builders`
- `TestDoubles`

Order and Payment tests belong under `Aggregates`. Shared value objects remain under `ValueObjects`.

Integration tests are organized around workflows and integration boundaries. Fixtures contain supporting infrastructure and lifecycle management.

New folders and helper abstractions should be added when needed by actual tests.

## 8. Test Data and Isolation

- Use synthetic data without real customer or payment information.
- Use temporary credentials for disposable resources.
- Give each Integration test an isolated database.
- Use unique consumer queues where required.
- Keep tests independent of execution order.
- Verify persisted state through fresh scopes.
- Use new scopes and reloaded state when retrying failed units of work.
- Clean up resources after successful and failed execution.
- Use a controlled TimeProvider when exact time behavior matters.

Tests must not target production or shared development databases.

## 9. Environments and Tools

| Environment | Purpose |
|---|---|
| Local Unit Test environment | Fast feedback without external infrastructure |
| Local Integration Test environment | Disposable SQL Server and RabbitMQ verification |
| GitHub Actions | Repeatable automated checks for configured branches and Pull Requests |
| Future staging environment | Deployment, configuration and cross-context verification |

Current tools include:

- .NET and xUnit.
- FluentAssertions and xUnit assertions.
- WebApplicationFactory.
- Testcontainers.
- EF Core with SQL Server.
- MassTransit EF Bus Outbox and RabbitMQ.
- GitHub Actions.

Package and container image versions are defined in the repository.

Unavailable prerequisites must produce a visible failure rather than silently remove required coverage.

## 10. Current Position and Planned Work

### 10.1 Established Capabilities

The project currently has:

- Domain and Integration test projects.
- Coverage of important Order, Payment and Money behavior.
- Purchase workflow integration with actual persistence.
- Transaction, replay and concurrency checks.
- Migration verification.
- MassTransit Outbox delivery and controlled application restart verification.
- Solution-wide automated test execution in CI.

Detailed scenarios and individual execution results are maintained outside this plan.

### 10.2 Proposed Priorities

1. Expand Application validation and HTTP error-contract coverage.
2. Address missing domain rules and boundary conditions.
3. Extend concurrent payment preparation and settlement coverage.
4. Extend messaging interruption and recovery coverage.
5. Add provider integration tests alongside provider implementation.
6. Add consumer contract and idempotency tests alongside product-side consumption.
7. Verify access control before protected functions are exposed publicly.

Priorities should be reviewed when requirements or risks change.

## 11. Risks, Assumptions and Constraints

| Topic | Risk or constraint | Planned response |
|---|---|---|
| Provider outcomes | Settlement currently accepts already-verified results | Implement and test provider verification with the adapter |
| Message delivery | Messages can be delivered repeatedly | Require idempotent consumer processing |
| Test environments | Containers do not reproduce every Azure characteristic | Add staging and deployment checks when applicable |
| Recovery coverage | Controlled restart does not cover every failure mode | Extend coverage according to operational risk |
| Migration history | Changing applied migrations can break upgrade compatibility | Review supported schemas and upgrade paths |
| Business rules | Ambiguous rules can create contradictory expectations | Agree on behavior before implementation |
| CI resources | Containers increase execution time and resource usage | Share infrastructure appropriately while isolating test data |

The current integration approach assumes SQL Server and RabbitMQ. Architectural changes require review of the affected testing approach.

## 12. Roles and Responsibilities

The proposed working model is:

| Role | Responsibility |
|---|---|
| Business owner / Product Owner | Confirm rules, priorities and acceptance criteria |
| Developer | Implement, execute and maintain tests and application changes |
| Reviewer | Assess test adequacy, clarity and change impact |
| Project owner | Evaluate remaining risks and make merge or release decisions |
| CI automation | Execute configured checks and expose results |

One person may perform multiple roles. The associated responsibilities must remain explicit.

## 13. Execution, Resources and Schedule

Planning currently follows development events. Calendar dates and effort estimates are established for the relevant work item.

| Stage | Testing activity |
|---|---|
| Before implementation | Identify risks, expected behavior and affected test levels |
| During development | Run focused tests frequently |
| Before review | Execute relevant tests and required integration checks |
| During Pull Request validation | Run the configured full CI checks |
| After dependency or schema changes | Verify affected integrations and compatibility |
| Before a future release | Perform release-specific acceptance, security and deployment checks |

There is no separately agreed testing budget.

Test implementation, infrastructure usage and maintenance effort must be included in change estimates.

## 14. Entry, Exit and Suspension Criteria

### 14.1 Entry Criteria

- Expected behavior and scope are sufficiently clear.
- Required test data and infrastructure are available.
- Relevant interfaces and dependencies are identified.
- Migration starting points and supported upgrade paths are known where applicable.

### 14.2 Pull Request Exit Criteria

- Build and required automated checks pass.
- Important new or changed behavior has appropriate coverage.
- Required tests have not been unjustifiably disabled or skipped.
- Relevant persistence and contract changes have been reviewed.
- Known gaps and remaining risks are documented.
- Review findings are resolved or explicitly accepted as open.

Passing Pull Request checks does not automatically establish production readiness.

### 14.3 Suspension and Resumption

Suspend affected testing when:

- The environment cannot produce trustworthy results.
- Shared or production data could be affected.
- A fundamental failure invalidates subsequent results.

Resume after correcting the cause and restoring a defined starting state.

## 15. Deliverables, Reporting and Defect Management

Expected outputs include:

- Automated tests.
- Reviewable CI results.
- Defect reports with reproduction information.
- Documented coverage gaps and remaining risks.
- Test-result and coverage artifacts when required.

Defects are prioritized by business impact and urgency.

Failure analysis should distinguish environment problems, incorrect starting conditions and application defects. Repeated execution is not a substitute for diagnosing the cause.

Automatic publication of TRX and coverage artifacts is not currently configured. It can be introduced when reporting needs justify it.

## 16. Metrics

Useful indicators include:

- Passed, failed and skipped tests.
- Coverage of important business rules and risks.
- Execution time and suite stability.
- Recurring failures and flaky tests.
- Open defects by severity.
- Code Coverage as a supporting indicator.

No numeric Code Coverage threshold is currently enforced.

Metrics support judgment; a percentage alone does not demonstrate adequate testing.

## 17. Maintenance and Review

Review this plan when requirements, architecture, integrations, risks or development practices change.

Keep planned capabilities distinguishable from implemented capabilities.

The following remain subject to agreement:

- Priorities for additional test work.
- Role allocation and independent review expectations.
- Extent of dedicated Application Unit Tests.
- Timing of security, performance and staging verification.
- Requirements for retained test and coverage reports.

## 18. References

- [ISTQB CTFL — Section 5.1: Test Planning](https://www.istqb.org/wp-content/uploads/2024/11/ISTQB_CTFL_Syllabus_v4.0.1.pdf#page=48)
- [TestRail — How to Create a Test Plan](https://www.testrail.com/blog/create-a-test-plan/)
- [Atlassian — Test Plan Template](https://www.atlassian.com/software/confluence/resources/guides/how-to/test-plan)
- [TestRail — Test Plan vs. Test Strategy](https://www.testrail.com/blog/test-plan-vs-test-strategy/)
