# Run Tests: $ARGUMENTS

Run tests for the Reactivities application.

## Commands

```bash
# Run all tests
dotnet test

# Run only the Tests project
dotnet test Tests/Tests.csproj

# Run a specific test class
dotnet test --filter "FullyQualifiedName~$ARGUMENTS"

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run with code coverage
dotnet test /p:CollectCoverage=true

# Run and watch for changes
dotnet watch test --project Tests/Tests.csproj
```

## Test Project Layout

```
Tests/
├── Application/       # Handler tests (xUnit + NSubstitute + FluentAssertions)
│   └── Activities/
│       ├── Commands/  # CreateActivity, EditActivity, DeleteActivity
│       └── Queries/   # GetActivityList, GetActivityDetails, exports
└── API/               # Controller tests (ASP.NET Core Mvc.Testing)
    └── Controllers/
```

## Adding New Tests

- Application handler tests go in `Tests/Application/Activities/Commands/` or `Tests/Application/Activities/Queries/`
- Controller tests go in `Tests/API/Controllers/`
- Use `DbContextMockHelper` for in-memory EF Core context
- Use `NSubstitute` for mocking interfaces
- Use `FluentAssertions` for readable assertions (`result.Should().Be(...)`)
- Namespace must follow `Tests.Application.*` or `Tests.API.*`

## Conventions

- One test class per handler or controller
- Arrange / Act / Assert sections separated by blank lines
- Test method names: `MethodName_Scenario_ExpectedResult`
- Do not use `Moq` — this project uses `NSubstitute`
