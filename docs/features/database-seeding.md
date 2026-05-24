# Feature: Database Seeding
**Status**: complete
**Date**: 2026-05-24
**Author**: Claude Code

## Overview
Database seeding initializes an empty local database with demo activities, people, tags, groups, and events during API startup.

## Business Logic
- `Persistence/DbInitializer.cs` skips seeding when any supported table already contains data, preventing duplicate demo rows in partially populated databases.
- `Persistence/DbInitializer.cs` clears existing seedable tables only when startup explicitly passes `clearExistingData: true`.
- Seed data is inserted in dependency order: tags, people, groups, events, then activities.

## Architecture
| Layer | File(s) | Responsibility |
|---|---|---|
| Domain | `Domain/Activity.cs`, `Domain/Person.cs`, `Domain/Tag.cs`, `Domain/Group.cs`, `Domain/Event.cs` | Seeded entity definitions |
| Persistence | `Persistence/DbInitializer.cs`, `Persistence/AppDbContext.cs` | Startup seed orchestration and EF Core sets |
| API | `API/Program.cs` | Invokes database initialization on startup |

## API Contract
No public API endpoint is exposed for seeding. It runs during application startup.

## Known Limitations / TODOs
- Seeding currently uses table presence as the initialization marker rather than a dedicated seed history record.
