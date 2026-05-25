# Feature: Critical Correctness Fixes
**Status**: complete
**Date**: 2026-05-25
**Author**: Claude Code

## Overview
This change set fixes high-impact correctness regressions in event lookup, startup seeding, activity creation, and session state handling.

## Business Logic
- Event detail, update, and delete handlers resolve records by the public `EventId` instead of the inherited EF primary key `GroupId`.
- Automatic startup seeding is skipped when any core table already has data, preventing duplicate seed rows after partial migrations.
- Activity creation in the frontend posts to the backend before updating local state.
- Failed activity creation leaves the list unchanged and shows an error in the form.
- Logout and new login clear in-memory activities, profile data, and selections before loading fresh data.

## Architecture
| Layer | File(s) | Responsibility |
|---|---|---|
| Application | `Application/Events/Queries/GetEventDetails.cs` | Load event details by `EventId` |
| Application | `Application/Events/Commands/EditEvent.cs` | Update event by `EventId` |
| Application | `Application/Events/Commands/DeleteEvent.cs` | Delete event by `EventId` |
| Persistence | `Persistence/DbInitializer.cs` | Skip automatic seed when existing data is present |
| Frontend | `client/src/app/layout/App.tsx` | Persist activity creates and reset session state |
| Frontend | `client/src/feature/activities/form/ActivityForm.tsx` | Await activity creation and show failures |

## API Contract
**Endpoint**: `POST /api/activities`
**Request body**:
```json
{
  "id": "guid",
  "title": "Activity title",
  "date": "2026-11-12T18:30:00",
  "description": "Description",
  "category": "Networking",
  "city": "New York",
  "venue": "Innovation Loft",
  "latitude": 40.71,
  "longitude": -74,
  "creatorDisplayName": "Jeff"
}
```
**Response**:
```json
{
  "id": "guid",
  "title": "Activity title",
  "date": "2026-11-12T18:30:00",
  "city": "New York",
  "venue": "Innovation Loft"
}
```
**Errors**:
- `400 Bad Request` — activity payload is invalid
- `500 Internal Server Error` — activity could not be persisted

## Known Limitations / TODOs
- Event API contracts still expose both `EventId` and inherited `GroupId`; this fix preserves the existing model shape.
- Frontend authentication remains a local demo flow and is not a production authorization boundary.
