# Feature: Activity Creation
**Status**: complete
**Date**: 2026-05-10
**Author**: Claude Code

## Overview
Activity creation lets a signed-in frontend user submit a new activity and persist it through the existing activities API before updating the local UI.

## Business Logic
- The form requires title, date, city, and venue before submitting (`client/src/feature/activities/form/ActivityForm.tsx`).
- The creator display name is set from the current user shown by the application shell (`client/src/feature/activities/form/ActivityForm.tsx`).
- The frontend calls `POST /api/activities/` and only clears the form after the API save succeeds (`client/src/app/layout/App.tsx`, `client/src/feature/activities/form/ActivityForm.tsx`).
- If saving fails, the form shows an error and keeps entered values so the user can retry (`client/src/feature/activities/form/ActivityForm.tsx`).
- The backend maps `CreatorDisplayName` to an existing or newly created `Person` record before saving the activity (`Application/Activities/Commands/CreateActivity.cs`).

## Architecture
| Layer | File(s) | Responsibility |
|---|---|---|
| Domain | `Domain/Activity.cs` | Activity entity definition |
| Application | `Application/Activities/Commands/CreateActivity.cs` | CQRS handler and creator mapping |
| Persistence | `Persistence/AppDbContext.cs` | EF Core activity persistence |
| API | `API/Controllers/AcitivitiesController.cs` | HTTP create endpoint |
| Frontend | `client/src/app/layout/App.tsx`, `client/src/feature/activities/form/ActivityForm.tsx` | Form submission and UI state update |

## API Contract
**Endpoint**: `POST /api/activities/`
**Request body**:
```json
{
  "id": "guid",
  "title": "Created activity",
  "date": "2026-11-12T18:30:00",
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
  "title": "Created activity",
  "date": "2026-11-12T18:30:00",
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
- `400 Bad Request` — invalid request payload
- `500 Internal Server Error` — persistence failure

## Known Limitations / TODOs
- The current frontend authentication model is local-only demo state and does not provide server-side authorization.
