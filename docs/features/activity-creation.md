# Feature: Activity Creation
**Status**: complete
**Date**: 2026-05-20
**Author**: Claude Code

## Overview
Activity creation lets an authenticated user submit a new activity from the React dashboard and persists it through the activities API before updating local UI state.

## Business Logic
- The activity form requires title, date, city, and venue before submitting (`client/src/feature/activities/form/ActivityForm.tsx`).
- The submitted activity is sent to `POST /api/activities`; local activity and profile state update only after the API returns a valid persisted activity (`client/src/app/layout/App.tsx`).
- If persistence fails, the unsaved activity is not added to local state and form values remain available for retry (`client/src/feature/activities/form/ActivityForm.tsx`).
- The backend maps `CreatorDisplayName` to a persisted `Person` record when provided (`Application/Activities/Commands/CreateActivity.cs`).

## Architecture
| Layer | File(s) | Responsibility |
|---|---|---|
| Domain | `Domain/Activity.cs` | Activity model including creator metadata |
| Application | `Application/Activities/Commands/CreateActivity.cs` | CQRS command handler and creator mapping |
| Persistence | `Persistence/AppDbContext.cs` | EF Core activity and creator relationship persistence |
| API | `API/Controllers/AcitivitiesController.cs` | `POST /api/activities` endpoint |
| Frontend | `client/src/feature/activities/form/ActivityForm.tsx`, `client/src/app/layout/App.tsx` | Collects form input, posts to API, updates UI after persistence |

## API Contract
**Endpoint**: `POST /api/activities`

**Request body**:
```json
{
  "id": "guid",
  "title": "Community meetup",
  "date": "2026-11-12T18:30",
  "description": "Created from the dashboard",
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
  "title": "Community meetup",
  "date": "2026-11-12T18:30",
  "description": "Created from the dashboard",
  "category": "Networking",
  "city": "New York",
  "venue": "Innovation Loft",
  "latitude": 40.71,
  "longitude": -74,
  "creatorDisplayName": "Jeff"
}
```

**Errors**:
- `400 Bad Request` — invalid activity payload
- `500 Internal Server Error` — persistence or unexpected creation failure

## Known Limitations / TODOs
- The current login flow is local demo authentication, so creator identity is supplied by the client display name.
