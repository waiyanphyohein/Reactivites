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
