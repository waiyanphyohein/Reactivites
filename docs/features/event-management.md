# Feature: Event Management
**Status**: complete
**Date**: 2026-05-16
**Author**: Claude Code

## Overview
Event management exposes list, detail, create, update, delete, and export flows for `Event` records through the API and MediatR handlers.

## Business Logic
- Events are addressed at the API boundary by `EventId`.
- Because `Event` inherits from `Group`, EF Core stores the primary key in `GroupId`; created and seeded events keep `GroupId` synchronized with `EventId`.
- Detail, update, and delete handlers locate events by `EventId` so existing records with mismatched inherited keys remain reachable.
- Database seeding only runs when the application database is empty, preventing startup from duplicating seed records into databases that already contain user data.
- Database migration or seed failures stop API startup to avoid serving traffic against an invalid schema or partial seed state.

## Architecture
| Layer | File(s) | Responsibility |
|---|---|---|
| Domain | `Domain/Event.cs`, `Domain/Group.cs` | Event model and inherited group key |
| Application | `Application/Events/Commands/AddEvent.cs`, `Application/Events/Commands/EditEvent.cs`, `Application/Events/Commands/DeleteEvent.cs`, `Application/Events/Queries/GetEventDetails.cs` | CQRS handlers for event mutations and lookup |
| Persistence | `Persistence/DbInitializer.cs`, `Persistence/AppDbContext.cs` | Seed data and EF Core model |
| API | `API/Controllers/EventsController.cs`, `API/Program.cs` | HTTP endpoints and startup database initialization |

## API Contract
**Endpoint**: `GET /api/events/{eventId}`

**Response**:
```json
{
  "eventId": "guid",
  "eventName": "Tech Startup Pitch Night"
}
```

**Errors**:
- `404 Not Found` — event does not exist
- `400 Bad Request` — update route id does not match body `eventId`

## Known Limitations / TODOs
- `EventId` is not currently enforced as a database-level unique alternate key.
