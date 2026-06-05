# Feature: Event Management
**Status**: complete
**Date**: 2026-06-05
**Author**: Claude Code

## Overview
Event management exposes API operations for listing, retrieving, creating, editing, deleting, and exporting events. Public event routes identify events by `EventId`, while EF Core stores events as a `Group` subtype whose primary key is the inherited `GroupId`.

## Business Logic
- Event details, edits, and deletes resolve records by `EventId` in `Application/Events/Queries/GetEventDetails.cs`, `Application/Events/Commands/EditEvent.cs`, and `Application/Events/Commands/DeleteEvent.cs`.
- Missing events return the existing not-found behavior: `GetEventDetails` throws an `HttpRequestException` with `NotFound`, while edit/delete throw `KeyNotFoundException` for controller translation.
- Creating an event generates `EventId` and `GroupId` when either is empty in `Application/Events/Commands/AddEvent.cs`.
- Export queries return all events as Excel or CSV without modifying state.

## Architecture
| Layer | File(s) | Responsibility |
|---|---|---|
| Domain | `Domain/Event.cs`, `Domain/Group.cs` | Event entity and inherited group identity |
| Application | `Application/Events/Queries/*.cs`, `Application/Events/Commands/*.cs` | CQRS handlers for event reads, writes, and exports |
| Persistence | `Persistence/AppDbContext.cs` | EF Core event set and relationships |
| API | `API/Controllers/EventsController.cs` | HTTP endpoints using `EventId` route values |
| Frontend | N/A | No dedicated event UI is currently documented |

## API Contract
**Endpoint**: `GET /api/events/{eventId}`
**Response**:
```json
{ "eventId": "guid", "eventName": "string", "groupId": "guid" }
```
**Errors**:
- `404 Not Found` - event does not exist

**Endpoint**: `POST /api/events`
**Request body**:
```json
{ "eventName": "string", "groupName": "string", "organizers": [] }
```
**Response**:
```json
{ "eventId": "guid", "eventName": "string", "groupId": "guid" }
```

**Endpoint**: `PUT /api/events/{eventId}`
**Request body**:
```json
{ "eventId": "guid", "eventName": "string" }
```
**Errors**:
- `400 Bad Request` - route ID and body `EventId` differ
- `404 Not Found` - event does not exist

**Endpoint**: `DELETE /api/events/{eventId}`
**Errors**:
- `404 Not Found` - event does not exist

## Known Limitations / TODOs
- `EventId` is not configured as a database alternate key; handlers assume the application-generated GUID is unique.
