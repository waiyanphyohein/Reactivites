# Feature: Activity Creation
**Status**: complete
**Date**: 2026-05-09
**Author**: Claude Code

## Overview
Activity creation lets a signed-in frontend user create an activity and persist it through the Activities API before it is shown in local activity/profile state.

## Business Logic
- `ActivityForm` validates that title, date, city, and venue are present before submitting.
- `App.handleCreateActivity` sends the new activity to `POST /api/activities` and only updates React state after the API returns successfully.
- `ActivityForm` keeps entered data visible and shows an error if persistence fails.
- `CreateActivity.Handler` assigns a new id when needed and maps `CreatorDisplayName` to an existing or newly created `Person`.

## Architecture
| Layer | File(s) | Responsibility |
|---|---|---|
| Domain | `Domain/Activity.cs` | Activity entity, including creator metadata |
| Application | `Application/Activities/Commands/CreateActivity.cs` | CQRS handler and creator-person mapping |
| Persistence | `Persistence/AppDbContext.cs` | Activity-to-person relationship |
| API | `API/Controllers/AcitivitiesController.cs` | `POST /api/activities` endpoint |
| Frontend | `client/src/app/layout/App.tsx`, `client/src/feature/activities/form/ActivityForm.tsx` | API submission, state update, and form error handling |

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
- `400 Bad Request` - activity payload is invalid.
- `500 Internal Server Error` - persistence fails unexpectedly.

## Known Limitations / TODOs
- The frontend still uses a lightweight local login flow rather than a real authenticated identity.
