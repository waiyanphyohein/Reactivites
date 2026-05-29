# Feature: Activity Creation
**Status**: complete
**Date**: 2026-05-29
**Author**: Claude Code

## Overview
Users can create a new activity from the frontend activity dashboard. The client sends the activity to the API before adding it to local state so newly created activities survive refreshes and are visible to other API consumers.

## Business Logic
- The form requires title, date, city, and venue before submitting (`client/src/feature/activities/form/ActivityForm.tsx`).
- The form maps the current profile display name to `creatorDisplayName` before submitting (`client/src/feature/activities/form/ActivityForm.tsx`).
- The app posts the new activity to `POST /api/activities` and updates local activity/profile state only after the API accepts the create request (`client/src/app/layout/App.tsx`).
- If persistence fails, the form shows an error and keeps the user's input for retry (`client/src/feature/activities/form/ActivityForm.tsx`).

## Architecture
| Layer | File(s) | Responsibility |
|---|---|---|
| Domain | `Domain/Activity.cs` | Activity entity definition |
| Application | `Application/Activities/Commands/CreateActivity.cs` | Persists activities and maps creator display names to people |
| Persistence | `Persistence/AppDbContext.cs` | EF Core activity/person sets |
| API | `API/Controllers/AcitivitiesController.cs` | HTTP create endpoint |
| Frontend | `client/src/feature/activities/form/ActivityForm.tsx`, `client/src/app/layout/App.tsx` | Collects activity input, calls the API, and updates UI state after persistence |

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
  "longitude": -74,
  "creatorDisplayName": "Jeff"
}
```

**Response**:
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
  "longitude": -74,
  "creatorDisplayName": "Jeff"
}
```

**Errors**:
- `400 Bad Request` — activity payload is invalid
- `500 Internal Server Error` — persistence failed

## Known Limitations / TODOs
- The current frontend authentication state is local-only; creator identity is taken from the displayed profile name.
