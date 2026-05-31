# Feature: Activity Creation and Profile Persistence
**Status**: complete
**Date**: 2026-05-31
**Author**: Claude Code

## Overview
Activity creation from the React UI persists through the backend API before the new activity is added to local state. Profile loading uses the active username so users do not see another user's profile data.

## Business Logic
- `client/src/app/layout/App.tsx` posts new activities to `POST /api/activities` and only prepends the created activity after the API succeeds.
- `client/src/feature/activities/form/ActivityForm.tsx` awaits the create result; failed saves leave the form values intact and show an error.
- `client/src/app/layout/App.tsx` derives the profile route from the active display name or persisted username.
- `Application/Profiles/Queries/GetUserProfile.cs` returns the requested username as the profile identity.

## Architecture
| Layer | File(s) | Responsibility |
|---|---|---|
| Domain | `Domain/Activity.cs` | Activity fields, including creator metadata |
| Application | `Application/Activities/Commands/CreateActivity.cs`, `Application/Profiles/Queries/GetUserProfile.cs` | Persist activities and load user-specific profile data |
| Persistence | `Persistence/AppDbContext.cs` | EF Core activity and creator storage |
| API | `API/Controllers/AcitivitiesController.cs`, `API/Controllers/ProfilesController.cs` | Activity create and profile endpoints |
| Frontend | `client/src/app/layout/App.tsx`, `client/src/feature/activities/form/ActivityForm.tsx` | API-backed create flow and active-user profile loading |

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
  "creatorDisplayName": "Jeff"
}
```
**Errors**:
- `400 Bad Request` — activity payload is invalid
- `500 Internal Server Error` — persistence fails

## Known Limitations / TODOs
- Authentication is still demo/local state only; username is derived from the submitted email prefix.
