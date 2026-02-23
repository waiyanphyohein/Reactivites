# Database Migration: $ARGUMENTS

Manage Entity Framework Core migrations for the Reactivities application.

## Common Commands

```bash
# Create a new migration (run from repo root)
dotnet ef migrations add $ARGUMENTS -p Persistence -s API

# Apply pending migrations
dotnet ef database update -p Persistence -s API

# Remove the last unapplied migration
dotnet ef migrations remove -p Persistence -s API

# List all migrations
dotnet ef migrations list -p Persistence -s API

# Generate SQL script for a migration range
dotnet ef migrations script -p Persistence -s API
```

## Rules

- Migration names must be PascalCase and descriptive (e.g., `AddActivityCategory`, `RenameUserEmail`)
- Never manually edit generated migration files — modify the model and regenerate
- Always review the generated migration before applying it
- The database auto-migrates on startup via `DbInitializer` — manual `database update` is only needed for CI or explicit runs
- SQLite database file is at `API/reactivites.db`

## After Creating a Migration

1. Review the generated file in `Persistence/Migrations/`
2. Verify Up/Down methods are correct
3. Run `dotnet build` to confirm no compile errors
4. Restart the API to apply the migration automatically, or run `dotnet ef database update -p Persistence -s API`
