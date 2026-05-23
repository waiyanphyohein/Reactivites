# Feature: Activity Creation
**Status**: complete
**Date**: 2026-05-23
**Author**: Claude Code

## Overview
Activity creation lets an authenticated frontend user create an activity and have it persisted by the backend before it is shown in the activity list.

## Business Logic
- The frontend submits new activities to `POST /api/activities` before adding them to local UI state (`client/src/app/layout/App.tsx`).
- The form keeps entered data when persistence fails and only clears after a successful create response (`client/src/feature/activities/form/ActivityForm.tsx`).
- The backend creates a new activity through the MediatR `CreateActivity` command and persists creator display data when provided (`Application/Activities/Commands/CreateActivity.cs`).

## Architecture
| Layer | File(s) | Responsibility |
|---|---|---|
| Domain | `Domain/Activity.cs` | Activity entity and creator fields |
| Application | `Application/Activities/Commands/CreateActivity.cs` | CQRS handler, persistence, creator relationship mapping |
| Persistence | `Persistence/AppDbContext.cs` | EF Core activity set and creator relationship |
| API | `API/Controllers/AcitivitiesController.cs` | HTTP create endpoint |
| Frontend | `client/src/feature/activities/form/ActivityForm.tsx`, `client/src/feature/activities/dashboard/ActivityDashboard.tsx`, `client/src/app/layout/App.tsx` | Form collection and persisted create flow |

## API Contract
**Endpoint**: `POST /api/activities`
**Request body**:
```json
{
  "id": "guid",
  "title": "Created In Test",
  "date": "2026-11-12T18:30",
  "description": "Created via form",
  "category": "Networking",
  "city": "New York",
  "venue": "Innovation Loft",
  "latitude": 40.71,
  "longitude": -74.0,
  "creatorDisplayName": "Jeff"
}
```
**Response**:
```json
{
  "id": "guid",
  "title": "Created In Test",
  "date": "2026-11-12T18:30:00",
  "description": "Created via form",
  "category": "Networking",
  "city": "New York",
  "venue": "Innovation Loft",
  "latitude": 40.71,
  "longitude": -74.0,
  "creatorDisplayName": "Jeff",
  "creatorPersonId": "guid"
}
```
**Errors**:
- `400 Bad Request` — activity payload is invalid
- `500 Internal Server Error` — activity could not be persisted

## Known Limitations / TODOs
- Authentication is currently represented by local frontend state; server-side authorization is not enforced for activity creation.
