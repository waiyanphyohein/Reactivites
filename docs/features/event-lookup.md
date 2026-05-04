# Feature: Event Lookup
**Status**: complete
**Date**: 2026-05-04
**Author**: Claude Code

## Overview
Event read, update, and delete operations resolve records by the API-facing `EventId` value returned to clients. This keeps the event routes aligned with their public contract even though EF Core stores `Event` entities using the inherited `GroupId` primary key.

## Business Logic
- `GET /api/events/{eventId}` returns the event whose `EventId` matches the route value.
- `PUT /api/events/{eventId}` updates the event whose `EventId` matches the request body and route value.
- `DELETE /api/events/{eventId}` removes the event whose `EventId` matches the route value.
- Missing events still return the existing not-found behavior from the handlers/controllers.

## Architecture
| Layer | File(s) | Responsibility |
|---|---|---|
| Domain | `Domain/Event.cs`, `Domain/Group.cs` | `Event` exposes `EventId` and inherits the EF primary key `GroupId`. |
| Application | `Application/Events/Queries/GetEventDetails.cs` | Queries events by `EventId`. |
| Application | `Application/Events/Commands/EditEvent.cs` | Locates the existing event by `EventId` before mapping updates. |
| Application | `Application/Events/Commands/DeleteEvent.cs` | Locates the existing event by `EventId` before deletion. |
| API | `API/Controllers/EventsController.cs` | Exposes event routes using `{eventId}`. |

## API Contract
**Endpoint**: `GET /api/events/{eventId}`
**Response**:
```json
{ "eventId": "guid", "eventName": "string" }
```

**Endpoint**: `PUT /api/events/{eventId}`
**Request body**:
```json
{ "eventId": "guid", "eventName": "string" }
```

**Endpoint**: `DELETE /api/events/{eventId}`
**Response**: `204 No Content`

**Errors**:
- `404 Not Found` - no event exists with the requested `EventId`.
- `400 Bad Request` - update route `eventId` does not match the request body `EventId`.

## Known Limitations / TODOs
- Event entities still expose both `EventId` and inherited `GroupId`; future schema work could simplify this to a single public and persisted key.
