# Feature: Activity Creation
**Status**: complete
**Date**: 2026-05-06
**Author**: Claude Code

## Overview
The activity creation flow lets an authenticated user create a new activity from the dashboard form and persists it through the backend activities API before updating the local UI state.

## Business Logic
- The form requires title, date, city, and venue before submission in `client/src/feature/activities/form/ActivityForm.tsx`.
- New activities include the current display name as `creatorDisplayName` so profile ownership can be associated by the backend.
- The app posts the activity to `POST /api/activities` and updates the dashboard only after the API returns successfully in `client/src/app/layout/App.tsx`.
- If persistence fails, the form remains populated and displays an error instead of presenting an unsaved activity as created.

## Architecture
| Layer | File(s) | Responsibility |
|---|---|---|
| Domain | `Domain/Activity.cs` | Activity entity fields, including optional creator metadata |
| Application | `Application/Activities/Commands/CreateActivity.cs` | Persists the activity and maps creator display names to `Person` records |
| Persistence | `Persistence/AppDbContext.cs` | EF Core `Activities` and creator relationship configuration |
| API | `API/Controllers/AcitivitiesController.cs` | Exposes `POST /api/activities` |
| Frontend | `client/src/feature/activities/form/ActivityForm.tsx`, `client/src/app/layout/App.tsx` | Collects form input, saves via API, and updates UI state after persistence |

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
- `400 Bad Request` - invalid request body
- `500 Internal Server Error` - persistence failure

## Known Limitations / TODOs
- The login/profile identity is still demo-only and does not authenticate against the API.
