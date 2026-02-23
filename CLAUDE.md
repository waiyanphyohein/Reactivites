# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Reactivities is a full-stack application built with:
- **Backend**: ASP.NET Core Web API (.NET 10.0) using Clean Architecture with CQRS pattern
- **Frontend**: React + TypeScript with Vite, Material-UI
- **Database**: SQLite with Entity Framework Core

## Architecture

### Clean Architecture Layers

The backend follows Clean Architecture with four distinct projects:

1. **Domain** (`Domain/`)
   - Contains core business entities (e.g., `Activity.cs`)
   - No dependencies on other projects
   - Pure domain models

2. **Application** (`Application/`)
   - Implements CQRS pattern using MediatR
   - Commands: Create, Edit, Delete operations (`Application/Activities/Commands/`)
   - Queries: Read operations (`Application/Activities/Queries/`)
   - Dependencies: Domain, Persistence
   - Uses AutoMapper for object mapping (`Application/Core/MappingProfiles.cs`)
   - Contains helper utilities like `ExcelExporter` (`Application/Core/Helper/`)

3. **Persistence** (`Persistence/`)
   - Data access layer with Entity Framework Core
   - `AppDbContext.cs`: Database context
   - `DbInitializer.cs`: Seeds database on startup
   - SQLite database file: `API/reactivites.db`

4. **API** (`API/`)
   - ASP.NET Core Web API entry point
   - `Program.cs`: Application configuration, middleware pipeline, dependency injection
   - Controllers inherit from `BaseApiController.cs` which provides lazy-loaded IMediator and ILogger
   - Controllers are thin - they delegate to MediatR handlers

### CQRS Pattern

All business logic uses the MediatR CQRS pattern:
- **Queries**: Return data without modifying state (e.g., `GetActivityList`, `GetActivityDetails`, `GetActivityListExcel`)
- **Commands**: Modify state (e.g., `CreateActivity`, `EditActivity`, `DeleteActivity`)

When adding new features:
1. Define Query/Command class with nested Handler in `Application/Activities/`
2. Handler implements `IRequestHandler<TRequest, TResponse>`
3. Register in `Program.cs` via `AddMediatR()`
4. Controller calls `mediator.Send(new YourQuery/Command())`

### AutoMapper Configuration

`MappingProfiles.cs` contains custom mapping logic that:
- Maps Activity to Activity (for updates)
- Ignores null, empty, zero, and default values to prevent overwriting destination values
- Used by `EditActivity` command to update only provided fields

## Development Commands

### Backend (.NET API)

From repository root:

```bash
# Run the API (starts on https://localhost:5001)
cd API
dotnet run

# Build solution
dotnet build

# Restore packages
dotnet restore

# Run from solution root
dotnet run --project API/API.csproj
```

### Database Migrations

From repository root:

```bash
# Create new migration
dotnet ef migrations add <MigrationName> -p Persistence -s API

# Apply migrations
dotnet ef database update -p Persistence -s API

# Remove last migration
dotnet ef migrations remove -p Persistence -s API
```

Note: Database auto-migrates and seeds on startup via `DbInitializer` in `Program.cs`

### Frontend (React + Vite)

From `client/` directory:

```bash
# Install dependencies
npm install

# Run dev server (starts on http://localhost:3000)
npm run dev

# Build for production
npm run build

# Lint
npm run lint

# Preview production build
npm run preview
```

## Security Features

The API implements several security measures (configured in `Program.cs`):

- **HTTPS Enforcement**: All traffic redirected to HTTPS (port 5001)
- **HSTS**: HTTP Strict Transport Security with 1-year max-age, preload, and subdomain support
- **CORS**: Configured for localhost:3000 and localhost:3001 with credentials
- **Rate Limiting**: In-memory IP-based rate limiting (60 requests per minute per IP)
- **Database Naming**: Uses snake_case for database columns

## Key Configuration Files

- `API/appsettings.json`: Application configuration including:
  - Connection string (SQLite: `reactivites.db`)
  - Kestrel endpoints (HTTP:5000, HTTPS:5001)
  - HSTS and HTTPS redirection settings
  - Logging configuration

## Important Patterns

### Adding New Features

1. Create domain entity in `Domain/` if needed
2. Update `AppDbContext` and create migration
3. Create CQRS handlers in `Application/Activities/Commands/` or `Application/Activities/Queries/`
4. Register handler in `Program.cs` via `AddMediatR()`
5. Add controller endpoint that calls `mediator.Send()`
6. Update AutoMapper profiles if mapping is needed

### Error Handling

- Handlers throw `HttpRequestException` with appropriate `HttpStatusCode`
- Controllers catch `KeyNotFoundException` and return `NotFound()`
- Cancellation tokens are handled with `TaskCanceledException`

### Export Functionality

The application supports exporting activities to Excel format:
- `GetActivityListExcel.Query`: Returns byte array of Excel file
- `ExcelExporter` helper: Uses EPPlus library (NonCommercial license set in `Program.cs`)
- Endpoint: `GET /api/activities/export`

## Technology Stack Details

- **.NET 10.0**: Latest .NET version
- **MediatR**: CQRS pattern implementation
- **AutoMapper**: Object-to-object mapping
- **EPPlus**: Excel file generation
- **Entity Framework Core 9.0**: ORM with SQLite provider
- **Swagger/OpenAPI**: API documentation (development only)
- **React 19**: Frontend framework
- **Material-UI 7**: Component library
- **Axios**: HTTP client
- **Vite**: Build tool and dev server

## Testing

### Test Project Structure

All unit tests are consolidated in a single **Tests** project with the following organization:

```
Tests/
├── Tests.csproj
├── Application/                    # Application layer tests (43 tests)
│   ├── Activities/
│   │   ├── Commands/              # Command handler tests
│   │   │   ├── CreateActivityHandlerTests.cs (7 tests)
│   │   │   ├── EditActivityHandlerTests.cs (8 tests)
│   │   │   └── DeleteActivityHandlerTests.cs (7 tests)
│   │   └── Queries/               # Query handler tests
│   │       ├── GetActivityListHandlerTests.cs (4 tests)
│   │       ├── GetActivityDetailsHandlerTests.cs (5 tests)
│   │       ├── GetActivityListExcelHandlerTests.cs (6 tests)
│   │       └── GetActivityListCSVHandlerTests.cs (6 tests)
│   └── TestHelpers/
│       ├── DbContextMockHelper.cs
│       ├── ActivityTestData.cs
│       ├── MockLoggerFactory.cs
│       └── MapperFactory.cs
└── API/                           # API layer tests (22 tests)
    ├── Controllers/
    │   └── ActivitiesControllerTests.cs (22 tests)
    └── TestHelpers/
        ├── ControllerTestHelper.cs
        └── ActivityTestData.cs
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run only the Tests project
dotnet test Tests/Tests.csproj

# Run with coverage
dotnet test /p:CollectCoverage=true

# Run specific test class
dotnet test --filter "FullyQualifiedName~ActivitiesControllerTests"

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"
```

### Test Coverage Summary

- **Total Tests**: 65 (63 passed, 2 skipped)
- **Application Layer**: 43 tests covering all MediatR handlers
- **API Layer**: 22 tests covering all controller endpoints

### Testing Technologies

- **xUnit**: Test framework
- **NSubstitute**: Mocking framework
- **FluentAssertions**: Assertion library
- **EF Core InMemory**: In-memory database for testing
- **ASP.NET Core Mvc.Testing**: Controller testing utilities

### Adding New Tests

All future unit tests should be added to the **Tests** project:
- **Application layer tests**: Add to `Tests/Application/`
- **API layer tests**: Add to `Tests/API/`
- Follow the existing namespace pattern: `Tests.Application.*` or `Tests.API.*`

---

## .NET Conventions and Rules

### Naming

| Element | Convention | Example |
|---|---|---|
| Classes, records | PascalCase | `ActivityHandler` |
| Interfaces | `I` prefix + PascalCase | `IActivityRepository` |
| Methods | PascalCase | `GetActivityList` |
| Properties | PascalCase | `StartDate` |
| Private fields | `_camelCase` | `_context` |
| Local variables | camelCase | `activityId` |
| Constants | PascalCase or UPPER_SNAKE | `MaxRetryCount` |
| Namespaces | match folder structure | `Application.Activities.Queries` |

### Clean Architecture Rules

- **Domain** has zero dependencies — no EF Core, no MediatR, no HTTP references
- **Application** depends only on Domain and Persistence interfaces — never on API
- **Persistence** depends only on Domain and Application — never on API
- **API** depends on all layers but contains no business logic
- Never import a higher-level layer into a lower-level layer

### CQRS / MediatR Rules

- One handler class per file; handler is a nested class inside the Query/Command class
- Queries must not modify state; Commands must not return domain data (return `Unit` or a minimal DTO)
- Always accept and forward `CancellationToken` from the controller to the handler
- Handlers throw `HttpRequestException` for expected failures (not found, validation) with an appropriate `HttpStatusCode`
- Register handlers in `Program.cs` via `AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<T>())`

### Entity Framework Core Rules

- Use snake_case column names (already configured globally)
- Never call `SaveChangesAsync` more than once per handler
- Avoid `Include` chains deeper than 2 levels — use projection (`Select`) instead
- Do not use lazy loading — always use explicit `Include` or projections
- Migration names are PascalCase and descriptive: `AddActivityVenue`, not `Migration1`

### C# Code Style

- Use `var` when the type is obvious from the right-hand side; use explicit types for clarity when it isn't
- Prefer `async/await` over `.Result` or `.Wait()` — never block on async code
- Use primary constructors (C# 12+) where appropriate
- Use `record` types for immutable DTOs and value objects
- Use `file-scoped namespaces` (`namespace Foo.Bar;`) — not block-scoped
- Use expression-bodied members for single-line getters and simple methods
- Validate at system boundaries (controller/handler input) — trust internal invariants
- No commented-out code, no `TODO` without a linked issue

### AutoMapper Rules

- All mappings are declared in `Application/Core/MappingProfiles.cs`
- Null, empty, zero, and default values are ignored on destination to prevent overwriting (already configured)
- Do not add `ForMember` unless the property names differ or custom logic is required
- Never call `Mapper.Map` in Domain or Persistence layers

### Error Handling Rules

- Handlers throw `HttpRequestException` with `HttpStatusCode` for expected failures
- Controllers catch `KeyNotFoundException` → `NotFound()`
- Controllers catch `TaskCanceledException` → no response (request cancelled)
- Do not swallow exceptions silently; always log via the injected `ILogger`
- Do not expose stack traces or internal exception messages to the client

### Security Rules

- Never disable HTTPS or HSTS in production configuration
- Never bypass the rate limiter (`60 req/min per IP`) without explicit approval
- CORS origins are restricted to `localhost:3000` and `localhost:3001` — do not add wildcard origins
- Never store secrets in `appsettings.json` — use environment variables or Secret Manager
- Validate all user input at controller/handler boundaries; never trust raw query strings

---

## React / TypeScript Conventions and Rules

### File and Folder Structure

```
client/src/
├── app/
│   ├── layout/          # App shell, Navbar, global wrappers
│   └── shared/          # Truly generic reusable components
├── feature/
│   └── <domain>/        # Feature-scoped components (e.g., activities/)
│       ├── dashboard/
│       ├── details/
│       └── form/
└── lib/
    ├── agent.ts          # Centralised Axios API calls
    └── types.ts          # Shared TypeScript interfaces / types
```

### Component Rules

- **Named exports only** — no `export default` for components
- File name matches component name exactly: `ActivityCard.tsx` exports `ActivityCard`
- Extension is always `.tsx` for components, `.ts` for non-JSX files
- Do not use `React.FC<Props>` — use plain function declarations with typed props:
  ```tsx
  // Good
  export function ActivityCard({ activity }: Props) { ... }

  // Bad
  const ActivityCard: React.FC<Props> = ({ activity }) => { ... }
  ```
- Define `Props` interface directly above the component in the same file
- Destructure all props in the function signature

### TypeScript Rules

- No `any` — use `unknown` and narrow, or define the correct type
- All API response shapes have a corresponding TypeScript interface in `client/src/lib/types.ts`
- Use `interface` for object shapes; use `type` for unions, intersections, and aliases
- Prefer non-null assertion (`!`) only when the value is guaranteed non-null by context; otherwise narrow with a guard
- Strict mode is enabled in `tsconfig.json` — fix all type errors, do not suppress with `@ts-ignore`

### Naming Conventions

| Element | Convention | Example |
|---|---|---|
| Components | PascalCase | `ActivityCard` |
| Files (components) | PascalCase | `ActivityCard.tsx` |
| Files (utilities) | camelCase | `agent.ts` |
| Hooks | `use` prefix + camelCase | `useActivities` |
| Variables / functions | camelCase | `handleSubmit` |
| Constants | UPPER_SNAKE_CASE | `MAX_RETRY` |
| TypeScript interfaces | PascalCase | `Activity` |
| CSS classes / sx keys | camelCase in sx prop | `{ marginTop: 2 }` |

### State and Data Fetching Rules

- Prefer `useState` for local UI state; do not lift state higher than necessary
- Data fetching belongs in `useEffect` with proper cleanup (abort controller or ignore flag)
- All API calls go through `client/src/lib/agent.ts` — never call `axios` or `fetch` directly in components
- Do not put business logic in components — extract to custom hooks in `client/src/lib/` or `client/src/hooks/`
- Loading and error states must always be handled — never render undefined data without a guard

### Material-UI Rules

- Use MUI components (`Box`, `Card`, `Typography`, etc.) instead of raw HTML where an equivalent exists
- Use the `sx` prop for one-off responsive styles
- Do not use inline `style` objects for layout — use `sx` instead
- Use MUI theme tokens (`theme.spacing(2)`, `theme.palette.primary.main`) — no hardcoded pixel values
- Do not override MUI component internals via `.MuiXxx-root` class selectors unless unavoidable

### Axios / API Rules

- Base URL is `https://localhost:5001/api` — configured once in `agent.ts`
- Group API calls by resource (e.g., `agent.Activities.list()`)
- Use `async/await` with try/catch in components — do not use raw `.then().catch()` chains in UI code
- Dates returned from the API are ISO 8601 strings — parse with `new Date()` before display
- Do not store raw API responses in state if a mapped/typed version is needed — transform first

### Code Style Rules

- No unused imports or variables — the ESLint config enforces this
- Run `npm run lint` before committing — lint errors block the build
- No `console.log` in committed code — use a proper logger or remove debug statements
- Prefer `const` over `let`; avoid `var`
- Keep components under ~150 lines — extract sub-components when they grow larger
- One component per file

---

## Action Logging Rules

Every meaningful step Claude Code takes during a session **must be logged** to `docs/logs/actions_<YYYY-MM-DD>.md`. This provides a human-readable audit trail that supports rollback, replay, and debugging.

### What to Log

Log every action that creates, modifies, or deletes a file, runs a build/test command, runs a migration, or makes a structural change. Skip trivial read-only operations (file reads, searches).

### Log Entry Format

```md
## [HH:MM] <Short Action Title>

**Type**: create | modify | delete | run | migrate | refactor
**Files affected**:
- `path/to/file.cs` — added X / removed Y / updated Z
**Command run** _(if applicable)_: `dotnet ef migrations add AddActivityVenue -p Persistence -s API`
**Reason**: Why this step was taken
**Rollback**: How to undo this step (e.g., delete file, run `migrations remove`, revert diff)
```

### When to Write the Log

- **Before** running any destructive command (migration, delete, overwrite)
- **After** completing each logical step (not after each individual line of code)
- At the **end of a session**, write a summary entry titled `## Session Summary`

### Log File Location

```
docs/logs/
└── actions_YYYY-MM-DD.md    # One file per calendar day
```

If the file does not exist yet, create it with a header:
```md
# Action Log — YYYY-MM-DD
_Auto-generated by Claude Code. Do not edit manually during an active session._
```

### Rollback Guidance

Each log entry's `**Rollback**` field must be specific enough for a developer to undo the step without reading any code:
- For new files: `Delete <path>`
- For modified files: `git diff HEAD~1 -- <path>` then revert
- For migrations: `dotnet ef migrations remove -p Persistence -s API`
- For npm changes: `git checkout -- client/package.json && npm install`

---

## Context Window Management Rules

When working on tasks that accumulate large volumes of text — research findings, long error logs, analysis results, generated specs, conversation summaries — Claude Code **must offload that content to a context document file** rather than keeping it in the active conversation context.

### Directory Structure

```
docs/
├── logs/                        # Action logs (one file per day)
│   └── actions_YYYY-MM-DD.md
├── context/                     # Offloaded context documents
│   ├── text/                    # Free-form text, research, notes
│   │   └── context_<session_name>.md
│   ├── specs/                   # Feature specs, requirements, designs
│   │   └── context_<feature_name>.md
│   ├── errors/                  # Error analysis, debugging sessions
│   │   └── context_<issue_name>.md
│   └── api/                     # API contracts, DTO shapes, endpoint lists
│       └── context_<resource_name>.md
└── features/                    # Developer-facing feature documentation
    └── <feature_name>.md
```

### When to Offload to a Context File

Offload when any of the following is true:
- The content is **longer than ~150 lines** and not immediately needed for the next step
- The information is **reference material** (API shapes, error logs, research notes) rather than active working state
- You are about to start a **new sub-task** and the previous sub-task produced significant output
- The user asks to "remember" or "save" something for later in the session

### How to Offload

1. Write the content to the appropriate `docs/context/<category>/context_<name>.md` file
2. Replace the in-context copy with a one-line reference:
   > _Context saved to `docs/context/text/context_<name>.md`_
3. Continue working from the reference; re-read the file only when specifically needed

### Context File Header Format

```md
# Context: <Descriptive Title>
**Session**: <session_name>
**Date**: YYYY-MM-DD
**Category**: text | spec | error | api
**Summary**: One-sentence description of what this file contains

---

<content>
```

### Naming Convention

| Category | Path | Example name |
|---|---|---|
| Free-form notes / research | `docs/context/text/` | `context_activity_export_research.md` |
| Feature specs / designs | `docs/context/specs/` | `context_user_auth_spec.md` |
| Error / debugging session | `docs/context/errors/` | `context_ef_migration_error.md` |
| API contracts / DTOs | `docs/context/api/` | `context_activities_api.md` |

---

## Feature Documentation Rules

Whenever Claude Code **completes a new feature** (backend handler, frontend component, API endpoint, or any combination), it **must create or update a developer-facing documentation file** in `docs/features/`.

### When to Write Feature Docs

- After implementing a new CQRS query or command handler
- After adding a new API endpoint
- After creating a new React component or page
- After completing a full feature (end-to-end)
- After any change to business logic, even if no new files were created

### Feature Doc Location and Name

```
docs/features/<feature_name>.md
```

Use kebab-case: `activity-export.md`, `user-registration.md`, `event-attendees.md`

### Feature Doc Template

```md
# Feature: <Feature Name>
**Status**: complete | in-progress | deprecated
**Date**: YYYY-MM-DD
**Author**: Claude Code

## Overview
One paragraph describing what this feature does and why it exists.

## Business Logic
- Bullet-point list of all rules enforced by this feature
- Include validation rules, side effects, constraints
- Reference the handler / component where each rule lives

## Architecture
| Layer | File(s) | Responsibility |
|---|---|---|
| Domain | `Domain/...` | Entity definition |
| Application | `Application/.../HandlerName.cs` | CQRS handler, business logic |
| Persistence | `Persistence/AppDbContext.cs` | EF Core DbSet |
| API | `API/Controllers/...Controller.cs` | HTTP endpoint |
| Frontend | `client/src/feature/.../Component.tsx` | UI |

## API Contract
**Endpoint**: `GET /api/resource` _(or POST / PUT / DELETE)_
**Request body** _(if applicable)_:
```json
{ "field": "value" }
```
**Response**:
```json
{ "id": "guid", "field": "value" }
```
**Errors**:
- `404 Not Found` — resource does not exist
- `400 Bad Request` — validation failure

## Known Limitations / TODOs
- List any known gaps or deferred work
```

---

## Slash Commands (Skills)

The following slash commands are available via `.claude/commands/`:

| Command | Description |
|---|---|
| `/new-feature <name>` | Scaffold a full-stack feature following Clean Architecture + CQRS |
| `/migration <name>` | EF Core migration workflow and cheatsheet |
| `/test [filter]` | Run tests with common filter patterns |
| `/build` | Build + lint checklist for both backend and frontend |
| `/new-component <name>` | Scaffold a React component following project conventions |
| `/api-call <name>` | Add a typed Axios call connecting frontend to the .NET API |
| `/log-action <title>` | Append a structured action entry to today's action log |
| `/save-context <name>` | Offload current context to a `docs/context/` file |
