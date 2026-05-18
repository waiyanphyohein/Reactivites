# Feature: Activity Creation
**Status**: complete
**Date**: 2026-05-18
**Author**: Claude Code

## Overview
Activity creation lets an authenticated user submit a new activity from the React dashboard and persist it through the backend activities API before it appears in the list.

## Business Logic
- `client/src/feature/activities/form/ActivityForm.tsx` builds an `Activity` from required form fields and the current user's display name.
- `client/src/app/layout/App.tsx` posts the activity to `POST /api/activities` and only adds it to local activity/profile state after the API returns a valid persisted activity.
- If persistence fails, the form keeps the entered values for retry and the unsaved activity is not rendered locally.
- `Application/Activities/Commands/CreateActivity.cs` persists the activity and maps `CreatorDisplayName` to a `Person` relationship when provided.

## Architecture
| Layer | File(s) | Responsibility |
|---|---|---|
| Domain | `Domain/Activity.cs` | Activity entity, including creator fields |
| Application | `Application/Activities/Commands/CreateActivity.cs` | CQRS handler that persists new activities |
| Persistence | `Persistence/AppDbContext.cs` | EF Core `Activities` set and creator relationship |
| API | `API/Controllers/AcitivitiesController.cs` | `POST /api/activities` endpoint |
| Frontend | `client/src/app/layout/App.tsx`, `client/src/feature/activities/form/ActivityForm.tsx` | Submit and render created activities after API persistence succeeds |

## API Contract
**Endpoint**: `POST /api/activities`

**Request body**:
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
  "longitude": -74,
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
  "longitude": -74,
  "creatorDisplayName": "Jeff"
}
```

**Errors**:
- `400 Bad Request` - invalid activity payload.
- `408 Request Timeout` - request cancellation while creating the activity.
- `500 Internal Server Error` - persistence failure.

## Known Limitations / TODOs
- The UI logs create failures but does not yet show an inline error message.
