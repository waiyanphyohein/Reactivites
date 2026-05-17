# Feature: Event Management
**Status**: complete
**Date**: 2026-05-17
**Author**: Claude Code

## Overview
Event management exposes CRUD and export operations for `Event` records.

## Business Logic
- Events are addressed by `Event.EventId` at the API boundary.
- Event details, update, and delete handlers query by `EventId` so the public event identifier works even when inherited `GroupId` differs.
- Missing events return not-found errors from handlers and controllers.

## Architecture
| Layer | File(s) | Responsibility |
|---|---|---|
| Domain | `Domain/Event.cs`, `Domain/Group.cs` | Event entity inherits group metadata |
| Application | `Application/Events/Queries/GetEventDetails.cs`, `Application/Events/Commands/EditEvent.cs`, `Application/Events/Commands/DeleteEvent.cs` | CQRS lookup/update/delete behavior |
| Persistence | `Persistence/AppDbContext.cs` | EF Core event/group mapping |
| API | `API/Controllers/EventsController.cs` | Event HTTP endpoints |

## API Contract
**Endpoint**: `GET /api/events/{eventId}`
**Response**:
```json
{ "eventId": "guid", "eventName": "Event name" }
```
**Errors**:
- `404 Not Found` - event does not exist

## Known Limitations / TODOs
- Event identifiers and group identifiers remain separate values.
