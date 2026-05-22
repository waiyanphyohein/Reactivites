# Feature: Activity Creation
**Status**: complete
**Date**: 2026-05-22
**Author**: Claude Code

## Overview
Activity creation lets a signed-in client user create an activity through the React form and persist it through the API before showing it as saved.

## Business Logic
- `client/src/feature/activities/form/ActivityForm.tsx` builds an `Activity` payload with the current display name as `creatorDisplayName`.
- `client/src/app/layout/App.tsx` posts the payload to `POST /api/activities` and only prepends the activity to local state after the API succeeds.
- `Application/Activities/Commands/CreateActivity.cs` assigns a new ID when needed and links `CreatorDisplayName` to a `Person` record when provided.
- If the API save fails, the form keeps entered data and shows an error instead of silently treating the activity as saved.

## Architecture
| Layer | File(s) | Responsibility |
|---|---|---|
| Domain | `Domain/Activity.cs` | Activity entity and creator fields |
| Application | `Application/Activities/Commands/CreateActivity.cs` | Persist activity and creator relationship |
| Persistence | `Persistence/AppDbContext.cs` | Activity and creator relationship mapping |
| API | `API/Controllers/AcitivitiesController.cs` | `POST /api/activities` endpoint |
| Frontend | `client/src/feature/activities/form/ActivityForm.tsx`, `client/src/app/layout/App.tsx` | Form submission, API save, and local state update |

## API Contract
**Endpoint**: `POST /api/activities`
**Request body**:
```json
{
  "id": "guid-or-client-generated-id",
  "title": "Activity title",
  "date": "2026-10-12T14:30:00Z",
  "description": "Description",
  "category": "Networking",
  "city": "Boston",
  "venue": "Downtown Hub",
  "latitude": 42.36,
  "longitude": -71.06,
  "creatorDisplayName": "Jeff"
}
```
**Response**: The persisted activity.
**Errors**:
- `400 Bad Request` — activity payload is null.
- `408 Request Timeout` — request is cancelled.
- `500 Internal Server Error` — persistence fails.

## Known Limitations / TODOs
- Authentication is still client-side demo behavior; creator attribution uses a display name rather than a server-authenticated identity.
