# New Feature: $ARGUMENTS

Add a new feature to the Reactivities application following Clean Architecture and CQRS conventions.

## Steps

### 0. Log session start

Before writing any code, append an entry to today's action log (`docs/logs/actions_YYYY-MM-DD.md`):

```md
## [HH:MM] Start: $ARGUMENTS

**Type**: create
**Reason**: Implementing new feature — $ARGUMENTS
**Rollback**: No files created yet
```

---

### 1. Domain layer (if a new entity is needed)

- Create the entity class in `Domain/` with only primitive properties and no logic
- No dependencies on other layers
- **Log**: `## [HH:MM] Create domain entity <EntityName>` — files affected, rollback: `rm Domain/<EntityName>.cs`

### 2. Persistence layer (if a new entity is needed)

- Add `DbSet<T>` to `AppDbContext.cs`
- Run migration: `dotnet ef migrations add <MigrationName> -p Persistence -s API`
- **Log**: `## [HH:MM] Add EF migration <MigrationName>` — command run, rollback: `dotnet ef migrations remove -p Persistence -s API`

### 3. Application layer — CQRS handlers

- For reads: create `Application/Activities/Queries/<FeatureName>.cs` with a nested `Handler` implementing `IRequestHandler<Query, Result>`
- For writes: create `Application/Activities/Commands/<FeatureName>.cs` with a nested `Handler`
- Return `Result<T>` or throw `HttpRequestException` with an appropriate `HttpStatusCode` on failure
- Register in `Program.cs` via `AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<...>())`
- **Log**: one entry per handler file created

### 4. AutoMapper (if mapping is needed)

- Add mapping in `Application/Core/MappingProfiles.cs`
- Null/empty/zero values are ignored by convention — do not override destination fields with defaults
- **Log**: `## [HH:MM] Update MappingProfiles` — rollback: `git checkout HEAD -- Application/Core/MappingProfiles.cs`

### 5. API layer

- Add thin endpoint to the relevant controller (or create a new controller inheriting `BaseApiController`)
- Controller calls `mediator.Send(new YourQuery/Command(...))` and returns the result
- Use `Ok()`, `NotFound()`, or `BadRequest()` as appropriate
- **Log**: `## [HH:MM] Add endpoint <METHOD> /api/<resource>` — files affected, rollback instructions

### 6. Tests

- Add Application handler tests to `Tests/Application/Activities/Commands/` or `Tests/Application/Activities/Queries/`
- Add controller tests to `Tests/API/Controllers/`
- Follow the naming convention `<FeatureName>HandlerTests.cs`
- Run tests: `dotnet test` — log the result
- **Log**: `## [HH:MM] Add tests for $ARGUMENTS` — test count, pass/fail result

### 7. Frontend (if a UI change is needed)

- Add/update the Axios call in `client/src/lib/`
- Update React components in `client/src/feature/`
- Run lint: `npm run lint --prefix client`
- **Log**: one entry per file created or modified

### 8. Write feature documentation (REQUIRED)

After the feature is complete, create `docs/features/<feature-name>.md` using the standard template:

```md
# Feature: $ARGUMENTS
**Status**: complete
**Date**: YYYY-MM-DD
**Author**: Claude Code

## Overview
<One paragraph — what the feature does and why it exists>

## Business Logic
- <Rule 1 — e.g., "Only activities in the future can be edited">
- <Rule 2 — validation, side effects, constraints>
- <Reference the handler file where each rule is enforced>

## Architecture
| Layer | File(s) | Responsibility |
|---|---|---|
| Domain | `Domain/...` | Entity definition |
| Application | `Application/.../HandlerName.cs` | CQRS handler, business logic |
| Persistence | `Persistence/AppDbContext.cs` | EF Core DbSet |
| API | `API/Controllers/...Controller.cs` | HTTP endpoint |
| Frontend | `client/src/feature/.../Component.tsx` | UI (if applicable) |

## API Contract
**Endpoint**: `METHOD /api/<resource>`
**Request**: <body or params>
**Response**: <shape>
**Errors**: <status codes and when they occur>

## Known Limitations / TODOs
- <Any deferred work or known gaps>
```

- **Log**: `## [HH:MM] Write feature docs for $ARGUMENTS` — path: `docs/features/<name>.md`

### 9. Session summary log entry

At the end, append a session summary to the action log:

```md
## Session Summary

**Feature**: $ARGUMENTS
**Steps completed**: N
**Files created**: list all new files
**Files modified**: list all changed files
**Migrations added**: name or "none"
**Tests**: X passed / Y failed / Z skipped
**Docs**: docs/features/<name>.md created
```

---

## Constraints

- Controllers must stay thin — no business logic in the API layer
- All business logic belongs in Application handlers
- Domain entities have no dependencies
- Always pass and handle `CancellationToken` in handlers
- Every step must be logged to `docs/logs/actions_YYYY-MM-DD.md`
- Feature documentation in `docs/features/` is not optional
