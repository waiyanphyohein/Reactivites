# Feature: Database Initialization
**Status**: complete
**Date**: 2026-05-22
**Author**: Claude Code

## Overview
Database initialization seeds demo data when the database is empty and skips seeding when any core application table already contains data.

## Business Logic
- `Persistence/DbInitializer.cs` clears all seeded data only when explicitly requested.
- Without clear mode, initialization skips seeding if any core table (`Activities`, `Events`, `People`, `Tags`, or `Groups`) has existing rows.
- This prevents partially populated databases from receiving duplicate seed records on application restart.

## Architecture
| Layer | File(s) | Responsibility |
|---|---|---|
| Domain | `Domain/Activity.cs`, `Domain/Event.cs`, `Domain/Person.cs`, `Domain/Tag.cs`, `Domain/Group.cs` | Seeded entity shapes |
| Application | N/A | No CQRS handler involved |
| Persistence | `Persistence/DbInitializer.cs` | Clear and seed behavior |
| API | `API/Program.cs` | Invokes migrations and initialization at startup |
| Frontend | N/A | No client behavior |

## API Contract
No direct API endpoint. Initialization runs during API startup.

## Known Limitations / TODOs
- There is no seed marker/version table; the guard uses existing data in core tables.
