# Feature: Event CRUD
**Status**: complete
**Date**: 2026-06-06
**Author**: Claude Code

## Overview
Event CRUD exposes list, details, create, update, delete, and export endpoints for `Event` records. Single-event routes are keyed by `EventId`, even though EF Core stores events through the inherited `GroupId` primary key.

## Business Logic
- Details, update, and delete handlers look up events by `Event.EventId`, matching the public API route parameter.
- Create generates missing `EventId` and `GroupId` values before persisting a new event.
- Update maps only provided values onto the existing event through `MappingProfiles`, preserving null, empty, zero, and default destination values.
- Delete removes the event matched by `EventId` and returns not found when no event exists for that identifier.

## Architecture
| Layer | File(s) | Responsibility |
|---|---|---|
| Domain | `Domain/Event.cs`, `Domain/Group.cs` | Event model and inherited group identity |
| Application | `Application/Events/Queries/GetEventDetails.cs` | Event details lookup by `EventId` |
| Application | `Application/Events/Commands/AddEvent.cs` | Event creation |
| Application | `Application/Events/Commands/EditEvent.cs` | Event update by `EventId` |
| Application | `Application/Events/Commands/DeleteEvent.cs` | Event delete by `EventId` |
| Persistence | `Persistence/AppDbContext.cs` | EF Core event and group mappings |
| API | `API/Controllers/EventsController.cs` | HTTP event endpoints |

## API Contract
**Endpoint**: `GET /api/events/{eventId}`

**Response**:
```json
{
  "eventId": "guid",
  "eventName": "Community Meetup",
  "eventDescription": "Monthly meetup",
  "location": "Main Hall"
}
```

**Endpoint**: `PUT /api/events/{eventId}`

**Request body**:
```json
{
  "eventId": "guid",
  "eventName": "Updated Community Meetup"
}
```

**Endpoint**: `DELETE /api/events/{eventId}`

**Errors**:
- `404 Not Found` — no event exists for the supplied `EventId`
- `400 Bad Request` — update route ID does not match the request body's `EventId`

## Known Limitations / TODOs
- Event entities still carry both `EventId` and inherited `GroupId`; callers should use `EventId` for event routes.
