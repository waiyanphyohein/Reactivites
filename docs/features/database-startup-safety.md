# Feature: Database Startup Safety
**Status**: complete
**Date**: 2026-05-31
**Author**: Claude Code

## Overview
Database startup now fails fast when migrations or seeding fail, and seeding is skipped when any existing table already contains data. This prevents a broken deployment from appearing healthy and avoids duplicating seed data after partial startup state.

## Business Logic
- `API/Program.cs` logs and rethrows migration or seed exceptions in every environment.
- `Persistence/DbInitializer.cs` treats the database as non-empty if any seeded table contains rows.
- `Tests/Persistence/DbInitializerTests.cs` verifies that a partially populated database is not seeded again.

## Architecture
| Layer | File(s) | Responsibility |
|---|---|---|
| Domain | `Domain/Activity.cs`, `Domain/Event.cs`, `Domain/Group.cs`, `Domain/Person.cs`, `Domain/Tag.cs` | Seeded entities |
| Application | N/A | No application handler changes |
| Persistence | `Persistence/DbInitializer.cs` | Seed guard and seed data creation |
| API | `API/Program.cs` | Migration and seed execution during startup |
| Frontend | N/A | No UI changes |

## API Contract
**Endpoint**: N/A
**Request body** _(if applicable)_:
```json
{}
```
**Response**:
```json
{}
```
**Errors**:
- Startup failure — migration or seed exceptions are rethrown so the host exits instead of serving with an unknown database state.

## Known Limitations / TODOs
- Existing partially seeded databases may still require manual cleanup or reseeding with `Database:ClearDataOnStartup` / `Database:ReseedOnStartup`.
