# Feature: Event Identity and Database Seeding
**Status**: complete
**Date**: 2026-06-11
**Author**: Claude Code

## Overview
Events are exposed through the API by `EventId`, while EF Core stores `Event` as a subtype of `Group` with `GroupId` as the inherited primary key. Event details, edits, and deletes therefore resolve records by `EventId`. Database initialization also skips seeding when any application table already contains data, preventing duplicate seed data after migrations add new empty tables.

## Business Logic
- Event details, edit, and delete handlers look up persisted events by public `EventId`.
- Event edits preserve the existing inherited `GroupId` primary key instead of accepting client-submitted key changes.
- Database initialization with `clearExistingData: false` seeds only when all tracked tables are empty.

## Architecture
| Layer | File(s) | Responsibility |
|---|---|---|
| Domain | `Domain/Event.cs`, `Domain/Group.cs` | Event inherits the Group identity model |
| Application | `Application/Events/Queries/GetEventDetails.cs`, `Application/Events/Commands/EditEvent.cs`, `Application/Events/Commands/DeleteEvent.cs` | Event lookup and mutation behavior |
| Persistence | `Persistence/DbInitializer.cs` | Seed skip guard for existing databases |
| API | `API/Controllers/EventsController.cs` | Routes event operations by `EventId` |
| Frontend | N/A | No UI changes |

## API Contract
**Endpoint**: `GET /api/events/{eventId}`
**Request body**: N/A
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
- `400 Bad Request` — route ID does not match body `eventId`
- `404 Not Found` — event does not exist

**Endpoint**: `DELETE /api/events/{eventId}`
**Request body**: N/A
**Response**: `204 No Content`
**Errors**:
- `404 Not Found` — event does not exist

## Known Limitations / TODOs
- `EventId` is not configured as a database alternate key, so uniqueness is enforced by normal creation flows rather than a database constraint.
