# Feature: Activity Create
**Status**: complete
**Date**: 2026-05-12
**Author**: Claude Code

## Overview
The activity create flow lets an authenticated frontend user submit a new activity and persists it through the backend `POST /api/activities` endpoint before the activity is shown in the dashboard.

## Business Logic
- `client/src/feature/activities/form/ActivityForm.tsx` validates the required title, date, city, and venue fields before submitting.
- `client/src/feature/activities/form/ActivityForm.tsx` disables duplicate submissions while a create request is in flight.
- `client/src/app/layout/App.tsx` updates local activity and profile state only after the API returns a created activity.
- `Application/Activities/Commands/CreateActivity.cs` maps `CreatorDisplayName` to an existing or new `Person` record before saving.

## Architecture
| Layer | File(s) | Responsibility |
|---|---|---|
| Domain | `Domain/Activity.cs` | Activity entity fields, including creator relationship metadata |
| Application | `Application/Activities/Commands/CreateActivity.cs` | CQRS handler that persists activities and creator relationships |
| Persistence | `Persistence/AppDbContext.cs` | EF Core `Activities` and related creator persistence |
| API | `API/Controllers/AcitivitiesController.cs` | HTTP `POST /api/activities` endpoint |
| Frontend | `client/src/feature/activities/form/ActivityForm.tsx`, `client/src/app/layout/App.tsx`, `client/src/lib/agent.ts` | Form submission, persisted state update, and typed API access |

## API Contract
**Endpoint**: `POST /api/activities`

**Request body**:
```json
{
  "id": "guid",
  "title": "Networking Night",
  "date": "2026-11-12T18:30:00Z",
  "description": "Meet local founders",
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
  "title": "Networking Night",
  "date": "2026-11-12T18:30:00Z",
  "description": "Meet local founders",
  "category": "Networking",
  "city": "New York",
  "venue": "Innovation Loft",
  "latitude": 40.71,
  "longitude": -74,
  "creatorDisplayName": "Jeff",
  "creatorPersonId": "guid"
}
```

**Errors**:
- `400 Bad Request` — activity payload is null or invalid.
- `408 Request Timeout` — request is cancelled while saving.
- `500 Internal Server Error` — persistence fails.

## Known Limitations / TODOs
- The frontend currently logs create failures instead of showing an inline error message.
