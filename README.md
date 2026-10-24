# Leave Management

An internal leave management system for employees and their approval hierarchy. Employees submit leave requests, managers approve them through a route that depends on the request length, and balances track annual quota, carry-over, reservations, usage, and expiry per year. Every state change is recorded in an audit trail.

## Overview

Tracking leave by hand, with spreadsheets, chat approvals, and a shared holiday calendar, breaks down once a company has more than a handful of employees. Two people can hold the same reservation, carry-over silently survives past its expiry date, and nobody can answer "who approved this, and when" without reconstructing a chat history.

This project models those rules where they belong: in the domain, as explicit invariants with unit tests, rather than as scattered checks in controllers or SQL.

- Leave is counted in **working days**: weekends and public holidays are excluded, and the result is frozen at submission so an approval later cannot change the balance that was already reserved
- Annual quota is **12 working days**, prorated from `HireDate` for employees who join mid-year, with up to 6 days of carry-over that expires on 31 March
- Requests longer than **5 working days** are routed through a final approval stage, and nobody may approve their own request. The self-decision guard already lives in `LeaveRequest`; the routing decision arrives with the Application layer
- An Administrator can **override** a decision, and the override is recorded with its reason and the original approver
- `AuditEntry` models the record of these changes (actor, entity, action, payload, timestamp). Writing one per transition will be handled by the application layer

## Status

The **Domain layer is complete** and covered by 400 unit tests:

- Aggregates and entities: `Employee`, `Position`, `LeaveRequest`, `ApprovalDecision`, `LeaveBalance`, `LeaveAllocation`, `QuotaAdjustment`, `Holiday`, `AuditEntry`, `JobExecutionMarker`
- Domain service: `LeaveCalculator`
- Value objects: `EmployeeId`, `LeaveBalanceId`, `LeaveRequestId`, `PositionId`, `LeaveDays`, `DateRange`, `LeaveAllocation`, `Email`, `Reason`

Planned next:

- Persistence with PostgreSQL and EF Core, plus migrations
- Application use cases and the HTTP API
- Authentication, authorization, and the endpoints described above

## Tech Stack

**Implemented**

- .NET 10 (SDK pinned in `global.json` with `rollForward: latestFeature`)
- Clean Architecture layout: `Api` / `Application` / `Domain` / `Infrastructure`, with `Domain` free of external dependencies
- `Result<T>` for domain outcomes instead of exceptions on business rules
- NUnit 4 for unit tests

**Planned**

- PostgreSQL + EF Core: persistence and migrations
- ASP.NET Core Identity + JWT Bearer: authentication and roles
- Docker Compose: local database and API
- GitHub Actions: build and test on every push and pull request

## Architecture

Four projects, one dependency rule: `Domain` knows nothing about the others, `Application` depends only on `Domain`, and `Infrastructure` implements the ports declared by `Application`. The API composes everything at startup.

The rules that make leave valid (how many days a request costs, what a balance may not do, who may decide) live in `Domain`. Anything that needs I/O or orchestration will live in `Application`.

See [project references](docs/diagrams/project-references.mermaid) for the exact dependency graph, and the other diagrams in `docs/diagrams/` for the domain model, state machine, and entity relationships.

## Getting started

### Prerequisites

- [.NET SDK 10.0.100](https://dotnet.microsoft.com/download/dotnet/10.0), matching `global.json`
- No database or container runtime is required yet; the project currently stops at the domain layer

### Build and test

```bash
git clone https://github.com/malllvinnn/leave-management.git
cd leave-management

dotnet restore
dotnet build LeaveManagement.sln --no-restore
dotnet test tests/LeaveManagement.Domain.Tests/LeaveManagement.Domain.Tests.csproj --no-restore
```

Expected: the build reports `0 Warning(s)` and `0 Error(s)`, and the Domain suite reports `Passed: 400`.

Run a focused group of tests with a filter on the test class or method name:

```bash
dotnet test tests/LeaveManagement.Domain.Tests/LeaveManagement.Domain.Tests.csproj --no-build --filter "FullyQualifiedName~LeaveRequestApproveFirstStageTests"
```

## Documentation

- [Project references](docs/diagrams/project-references.mermaid): layer dependencies and what each layer is responsible for
- [Domain model](docs/diagrams/class-domain.mermaid), [state machine](docs/diagrams/state-machine.mermaid), and [entity relationships](docs/diagrams/entity-relationship.mermaid)

The requirements document and task breakdown are private working documents and are not published in this repository; the status section above is the public summary. The diagrams mix implemented contracts with planned ones, so a drawn adapter or endpoint does not mean it exists in code yet.
