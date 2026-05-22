# Feature: Profile Activity Filtering
**Status**: complete
**Date**: 2026-05-22
**Author**: Claude Code

## Overview
Profile activity filtering returns the activities whose `CreatorDisplayName` matches the requested username, split into past and future collections for the profile page.

## Business Logic
- `Application/Profiles/Queries/GetUserProfile.cs` filters activities by `CreatorDisplayName` using case-insensitive comparison.
- Unknown usernames return empty event lists instead of falling back to all activities.
- Returned profile event DTOs include `CreatorDisplayName` so the frontend can apply creator-based filters.
- If a matched user has future events but no past events, synthetic past entries are still generated for demo profile history.

## Architecture
| Layer | File(s) | Responsibility |
|---|---|---|
| Domain | `Domain/Activity.cs` | Creator display name field |
| Application | `Application/Profiles/Queries/GetUserProfile.cs` | Profile DTO construction and activity filtering |
| Persistence | `Persistence/AppDbContext.cs` | Activity storage |
| API | `API/Controllers/ProfilesController.cs` | `GET /api/profiles/{username}` endpoint |
| Frontend | `client/src/feature/profile/UserProfilePage.tsx` | Profile display and event tabs |

## API Contract
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
- No explicit error response for unknown users; the API returns an empty profile event list.

## Known Limitations / TODOs
- Display name and avatar are currently hard-coded demo values.
