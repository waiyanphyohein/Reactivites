# Feature: Event Lookup
**Status**: complete
**Date**: 2026-05-14
**Author**: Claude Code

## Overview
Event detail, update, and delete operations resolve events by the public `EventId` used in API routes. This keeps the API contract stable even though events inherit from `Group` and EF Core stores `GroupId` as the database primary key.

## Business Logic
- `GET /api/events/{eventId}` returns the event whose `EventId` matches the route value (`Application/Events/Queries/GetEventDetails.cs`).
- `PUT /api/events/{eventId}` updates the event whose `EventId` matches the request body after the controller validates the route/body ID match (`Application/Events/Commands/EditEvent.cs`).
- `DELETE /api/events/{eventId}` removes the event whose `EventId` matches the route value (`Application/Events/Commands/DeleteEvent.cs`).
- Events are still stored with the inherited `GroupId` primary key; handlers do not require `GroupId` to equal `EventId`.

## Architecture
| Layer | File(s) | Responsibility |
|---|---|---|
| Domain | `Domain/Event.cs`, `Domain/Group.cs` | Event identity and inherited group primary key |
| Application | `Application/Events/Queries/GetEventDetails.cs`, `Application/Events/Commands/EditEvent.cs`, `Application/Events/Commands/DeleteEvent.cs` | Resolve event records by public `EventId` |
| Persistence | `Persistence/AppDbContext.cs` | EF Core TPH mapping for `Group`/`Event` |
| API | `API/Controllers/EventsController.cs` | Exposes event routes keyed by `EventId` |
| Frontend | N/A | No direct frontend changes |

## API Contract
**Endpoint**: `GET /api/events/{eventId}`
**Response**:
```json
{ "eventId": "guid", "eventName": "Event name" }
```
**Errors**:
- `404 Not Found` — event does not exist

**Endpoint**: `PUT /api/events/{eventId}`
**Request body**:
```json
{ "eventId": "guid", "eventName": "Updated event name" }
```
**Response**: `204 No Content`
**Errors**:
- `400 Bad Request` — route ID does not match request body `eventId`
- `404 Not Found` — event does not exist

**Endpoint**: `DELETE /api/events/{eventId}`
**Response**: `204 No Content`
**Errors**:
- `404 Not Found` — event does not exist

## Known Limitations / TODOs
- `EventId` is not currently enforced as a unique database column.
