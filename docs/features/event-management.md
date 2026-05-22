# Feature: Event Management
**Status**: complete
**Date**: 2026-05-22
**Author**: Claude Code

## Overview
Event management exposes list, detail, create, update, delete, and export endpoints for `Event` records. Route-based operations use the public `EventId` value, independent of the inherited `GroupId` EF primary key.

## Business Logic
- `Application/Events/Queries/GetEventDetails.cs` retrieves event details by `EventId` and returns `404 Not Found` when no event exists.
- `Application/Events/Commands/EditEvent.cs` updates an existing event selected by `EventId` and preserves existing values when mapped source fields are empty/default.
- `Application/Events/Commands/DeleteEvent.cs` deletes the event selected by `EventId`.
- `Application/Events/Commands/AddEvent.cs` creates events and generates missing identifiers.

## Architecture
| Layer | File(s) | Responsibility |
|---|---|---|
| Domain | `Domain/Event.cs`, `Domain/Group.cs` | Event entity inherits group fields |
| Application | `Application/Events/Queries/GetEventDetails.cs`, `Application/Events/Commands/EditEvent.cs`, `Application/Events/Commands/DeleteEvent.cs` | Route-keyed event operations |
| Persistence | `Persistence/AppDbContext.cs` | EF Core event/group mapping |
| API | `API/Controllers/EventsController.cs` | HTTP endpoints |
| Frontend | N/A | No dedicated event UI in the current client |

## API Contract
**Endpoints**:
- `GET /api/events/{eventId}`
- `PUT /api/events/{eventId}`
- `DELETE /api/events/{eventId}`

**Response**:
```json
{
  "eventId": "guid",
  "eventName": "Tech Startup Pitch Night",
  "location": "SoMa Startup Hub, San Francisco, CA"
}
```
**Errors**:
- `400 Bad Request` — update route ID does not match body `eventId`.
- `404 Not Found` — event does not exist.
- `408 Request Timeout` — request is cancelled.
- `500 Internal Server Error` — persistence fails.

## Known Limitations / TODOs
- `EventId` is not configured as a database unique index; current handlers assume it is unique.
