# Feature: Activity Creation
**Status**: complete
**Date**: 2026-06-11
**Author**: Claude Code

## Overview
The activity creation form submits new activities to the backend `POST /api/activities` endpoint and updates the local activity list and profile from the persisted API response.

## Business Logic
- The form requires title, date, city, and venue before submitting.
- The frontend sends the activity draft to the API instead of only mutating local state.
- The UI adds the created activity only after the API returns a response containing the required activity fields.
- Profile future/past event state is updated from the persisted activity when its creator matches the current user.

## Architecture
| Layer | File(s) | Responsibility |
|---|---|---|
| Domain | `Domain/Activity.cs` | Activity entity definition |
| Application | `Application/Activities/Commands/CreateActivity.cs` | Persists activity and maps creator display name |
| Persistence | `Persistence/AppDbContext.cs` | EF Core `Activities` set |
| API | `API/Controllers/AcitivitiesController.cs` | `POST /api/activities` endpoint |
| Frontend | `client/src/feature/activities/form/ActivityForm.tsx`, `client/src/app/layout/App.tsx` | Collect form input, call API, update UI state |

## API Contract
**Endpoint**: `POST /api/activities`
**Request body**:
```json
{
  "id": "guid-or-client-generated-id",
  "title": "Activity title",
  "date": "2026-11-12T18:30:00",
  "description": "Activity details",
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
{ "id": "guid", "title": "Activity title", "date": "2026-11-12T18:30:00" }
```
**Errors**:
- `400 Bad Request` — invalid activity payload
- `500 Internal Server Error` — persistence failure

## Known Limitations / TODOs
- Failed creates are logged in the browser console; there is no inline form error message yet.
