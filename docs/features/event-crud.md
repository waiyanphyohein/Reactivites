# Feature: Event CRUD
**Status**: complete
**Date**: 2026-06-08
**Author**: Claude Code

## Overview
Event CRUD exposes event list, detail, create, update, delete, and export operations through the API. Event route parameters use the public `EventId`, even though `Event` inherits from `Group` and EF Core stores the inherited `GroupId` as the primary key.

## Business Logic
- Creating an event with both `EventId` and `GroupId` empty generates one GUID and assigns it to both identifiers in `Application/Events/Commands/AddEvent.cs`.
- Creating an event with only one identifier set reuses that identifier for the missing one in `Application/Events/Commands/AddEvent.cs`.
- Details, edit, and delete operations locate events by `EventId` in `Application/Events/Queries/GetEventDetails.cs`, `Application/Events/Commands/EditEvent.cs`, and `Application/Events/Commands/DeleteEvent.cs`.
- Cancelled event detail, edit, and delete operations return request-timeout semantics instead of internal server errors.

## Architecture
| Layer | File(s) | Responsibility |
|---|---|---|
| Domain | `Domain/Event.cs`, `Domain/Group.cs` | Event entity inherits group fields and identifiers |
| Application | `Application/Events/Commands/AddEvent.cs`, `Application/Events/Commands/EditEvent.cs`, `Application/Events/Commands/DeleteEvent.cs`, `Application/Events/Queries/GetEventDetails.cs`, `Application/Events/Queries/GetEventList.cs` | CQRS handlers for event operations |
| Persistence | `Persistence/AppDbContext.cs` | EF Core DbSets and event/group relationship mapping |
| API | `API/Controllers/EventsController.cs` | HTTP event endpoints |
| Frontend | N/A | Event API is backend-only in this code path |

## API Contract
**Endpoint**: `GET /api/events/{eventId}`
**Response**:
```json
{ "eventId": "guid", "eventName": "Event name" }
```
**Errors**:
- `404 Not Found` — event does not exist
- `408 Request Timeout` — request was cancelled

**Endpoint**: `POST /api/events`
**Request body**:
```json
{ "eventName": "Event name", "groupName": "Group name", "organizers": [] }
```
**Response**:
```json
{ "eventId": "guid", "groupId": "guid", "eventName": "Event name" }
```
**Errors**:
- `400 Bad Request` — event body is null
- `408 Request Timeout` — request was cancelled

**Endpoint**: `PUT /api/events/{eventId}`
**Request body**:
```json
{ "eventId": "guid", "eventName": "Updated event name" }
```
**Errors**:
- `400 Bad Request` — route `eventId` does not match body `eventId`
- `404 Not Found` — event does not exist
- `408 Request Timeout` — request was cancelled

**Endpoint**: `DELETE /api/events/{eventId}`
**Errors**:
- `404 Not Found` — event does not exist
- `408 Request Timeout` — request was cancelled

## Known Limitations / TODOs
- `GroupName` and `GroupDescription` are fields on `Group`; future schema work should confirm whether they need persistence as mapped properties.
