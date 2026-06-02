# Feature: Events
**Status**: complete
**Date**: 2026-06-02
**Author**: Claude Code

## Overview
The Events feature exposes event list, detail, create, update, delete, and export operations through CQRS handlers and the Events API controller.

## Business Logic
- Event list returns all persisted events via `Application/Events/Queries/GetEventList.cs`.
- Event detail, update, and delete resolve records by the public `EventId` value, not the inherited `GroupId` primary key, in `GetEventDetails.cs`, `EditEvent.cs`, and `DeleteEvent.cs`.
- Event creation generates missing `EventId` and `GroupId` values before persisting in `Application/Events/Commands/AddEvent.cs`.
- Event Excel and CSV exports include all persisted events through `GetEventListExcel.cs` and `GetEventListCSV.cs`.

## Architecture
| Layer | File(s) | Responsibility |
|---|---|---|
| Domain | `Domain/Event.cs`, `Domain/Group.cs` | Event model inheriting group metadata |
| Application | `Application/Events/Commands/*.cs`, `Application/Events/Queries/*.cs` | CQRS event operations |
| Persistence | `Persistence/AppDbContext.cs` | EF Core event/group relationships |
| API | `API/Controllers/EventsController.cs` | HTTP endpoints |
| Frontend | _None currently wired end-to-end_ | Event UI is not currently implemented |

## API Contract
**Endpoints**:
- `GET /api/events`
- `GET /api/events/{eventId}`
- `POST /api/events`
- `PUT /api/events/{eventId}`
- `DELETE /api/events/{eventId}`
- `GET /api/events/export`
- `GET /api/events/export/csv`

**Request body** _(create/update)_:
```json
{
  "eventId": "guid",
  "eventName": "Community meetup",
  "eventDescription": "Optional description",
  "location": "Yangon"
}
```

**Response**:
```json
{
  "eventId": "guid",
  "eventName": "Community meetup",
  "eventDescription": "Optional description",
  "location": "Yangon"
}
```

**Errors**:
- `400 Bad Request` — route `eventId` does not match body `eventId` on update
- `404 Not Found` — event does not exist

## Known Limitations / TODOs
- Event records inherit `GroupId` as their EF primary key; API callers should use `EventId`.
- Event endpoints do not currently require authentication.
