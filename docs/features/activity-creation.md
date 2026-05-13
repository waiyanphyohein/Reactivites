# Feature: Activity Creation
**Status**: complete
**Date**: 2026-05-13
**Author**: Claude Code

## Overview
Activity creation lets an authenticated user submit a new activity from the dashboard form and persist it through the API before it appears in the local activity list or profile event lists.

## Business Logic
- The frontend requires title, date, city, and venue before submitting (`client/src/feature/activities/form/ActivityForm.tsx`).
- The frontend sends the activity to `POST /api/activities/` and only updates local state after the API returns successfully (`client/src/app/layout/App.tsx`).
- The form remains populated if persistence fails, preventing a false successful create (`client/src/feature/activities/form/ActivityForm.tsx`).
- The backend assigns a new id if the request id is empty and links `CreatorDisplayName` to a persisted `Person` when provided (`Application/Activities/Commands/CreateActivity.cs`).

## Architecture
| Layer | File(s) | Responsibility |
|---|---|---|
| Domain | `Domain/Activity.cs` | Activity entity and creator fields |
| Application | `Application/Activities/Commands/CreateActivity.cs` | CQRS command that creates the activity and maps creator data |
| Persistence | `Persistence/AppDbContext.cs` | EF Core `Activities` set and creator relationship |
| API | `API/Controllers/AcitivitiesController.cs` | `POST /api/activities/` endpoint |
| Frontend | `client/src/feature/activities/form/ActivityForm.tsx`, `client/src/app/layout/App.tsx` | Form submission and state update after persistence |

## API Contract
**Endpoint**: `POST /api/activities/`
**Request body**:
```json
{
  "id": "guid-or-client-generated-id",
  "title": "Activity title",
  "date": "2026-11-12T18:30:00",
  "description": "Activity description",
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
  "id": "guid-or-client-generated-id",
  "title": "Activity title",
  "date": "2026-11-12T18:30:00",
  "description": "Activity description",
  "category": "Networking",
  "city": "New York",
  "venue": "Innovation Loft",
  "latitude": 40.71,
  "longitude": -74,
  "creatorDisplayName": "Jeff"
}
```
**Errors**:
- `400 Bad Request` — activity payload is null or invalid
- `408 Request Timeout` — request is cancelled while creating the activity
- `500 Internal Server Error` — persistence fails

## Known Limitations / TODOs
- The frontend currently logs create failures to the browser console rather than showing an inline error message.
