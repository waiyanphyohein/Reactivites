# Feature: Activity Creation and Profile Ownership
**Status**: complete
**Date**: 2026-06-03
**Author**: Claude Code

## Overview
Activity creation lets an authenticated UI user create an activity that is persisted through the API and reflected in the activity list and profile event lists. Profile loading uses the logged-in username so users do not receive another user's profile metadata.

## Business Logic
- `client/src/feature/activities/form/ActivityForm.tsx` builds the activity payload with the current user's `creatorDisplayName` and waits for the create operation to complete before clearing the form.
- `client/src/app/layout/App.tsx` posts new activities to `POST /api/activities` and only updates React state from the server response after a successful save.
- `client/src/app/layout/App.tsx` loads profile data from `/api/profiles/{username}` using the persisted/logged-in username instead of a hardcoded user.
- `Application/Profiles/Queries/GetUserProfile.cs` returns profile display metadata based on the requested username.

## Architecture
| Layer | File(s) | Responsibility |
|---|---|---|
| Domain | `Domain/Activity.cs` | Activity model with optional creator metadata |
| Application | `Application/Activities/Commands/CreateActivity.cs` | Persists new activities and creator relationship metadata |
| Application | `Application/Profiles/Queries/GetUserProfile.cs` | Builds profile event lists for the requested username |
| Persistence | `Persistence/AppDbContext.cs` | EF Core activity and creator relationship mapping |
| API | `API/Controllers/AcitivitiesController.cs` | `POST /api/activities` endpoint |
| API | `API/Controllers/ProfilesController.cs` | `GET /api/profiles/{username}` endpoint |
| Frontend | `client/src/app/layout/App.tsx` | API orchestration and state updates |
| Frontend | `client/src/feature/activities/form/ActivityForm.tsx` | Activity form payload creation and submit handling |

## API Contract
**Endpoint**: `POST /api/activities`
**Request body**:
```json
{
  "id": "guid-string",
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
  "id": "guid-string",
  "title": "Created In Test",
  "date": "2026-11-12T18:30:00",
  "city": "New York",
  "venue": "Innovation Loft",
  "creatorDisplayName": "Jeff"
}
```
**Errors**:
- `400 Bad Request` - activity payload is missing.
- `500 Internal Server Error` - activity could not be persisted.

**Endpoint**: `GET /api/profiles/{username}`
**Response**:
```json
{
  "username": "jeff",
  "displayName": "jeff",
  "avatarUrl": "/images/jeff-placeholder.svg",
  "pastEvents": [],
  "futureEvents": []
}
```

## Known Limitations / TODOs
- Authentication is still demo-only and based on local storage; there is no server-side user identity enforcement.
- The profile query falls back to all activities when no creator data exists so seeded demo data remains visible.
