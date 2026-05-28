# Feature: Activity Creation and Profile Ownership
**Status**: complete
**Date**: 2026-05-28
**Author**: Claude Code

## Overview
Activity creation persists new activities through the API before they appear in the client list. Profile data is loaded for the active username and only includes persisted activities owned by that creator.

## Business Logic
- `client/src/app/layout/App.tsx` posts new activities to `POST /api/activities` and updates local activity/profile state only after the API returns successfully.
- `client/src/feature/activities/form/ActivityForm.tsx` keeps form input and shows an error if activity persistence fails.
- `Application/Profiles/Queries/GetUserProfile.cs` returns only activities whose `CreatorDisplayName` matches the requested username.
- `Application/Profiles/Queries/GetUserProfile.cs` returns only persisted past/future profile events; it does not fabricate historical activity IDs.
- `Application/Activities/Commands/CreateActivity.cs` reuses an existing creator only for exact full-name matches or previously-created `<name> User` demo users.

## Architecture
| Layer | File(s) | Responsibility |
|---|---|---|
| Domain | `Domain/Activity.cs`, `Domain/Person.cs` | Activity creator fields and person identity |
| Application | `Application/Activities/Commands/CreateActivity.cs`, `Application/Profiles/Queries/GetUserProfile.cs` | Persist activities and query owner-scoped profile events |
| Persistence | `Persistence/AppDbContext.cs` | EF Core persistence for activities and people |
| API | `API/Controllers/AcitivitiesController.cs`, `API/Controllers/ProfilesController.cs` | HTTP endpoints for activity creation and profile retrieval |
| Frontend | `client/src/app/layout/App.tsx`, `client/src/feature/activities/form/ActivityForm.tsx`, `client/src/lib/api.ts` | Submit activities, load current profile, and handle save errors |

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

**Endpoint**: `GET /api/profiles/{username}`
**Response**:
```json
{
  "username": "jeff",
  "displayName": "Jeff",
  "avatarUrl": "/images/jeff-placeholder.svg",
  "pastEvents": [],
  "futureEvents": []
}
```
**Errors**:
- `500 Internal Server Error` — unexpected persistence failure

## Known Limitations / TODOs
- Authentication remains demo-only and is not backed by server-side identity.
