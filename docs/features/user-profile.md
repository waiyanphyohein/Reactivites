# Feature: User Profile
**Status**: complete
**Date**: 2026-05-15
**Author**: Claude Code

## Overview
The user profile endpoint returns profile metadata and the activities created by the requested username, split into past and future event lists for the profile page.

## Business Logic
- `Application/Profiles/Queries/GetUserProfile.cs` trims the requested username before matching.
- Profile activities are limited to activities whose `CreatorDisplayName` matches the requested username case-insensitively.
- Unknown users or users without matching creator data return empty event lists instead of unrelated activities.
- When a matched user has no past activities, the handler mirrors up to three of that user's own activities as demo history.
- Profile event DTOs include `CreatorDisplayName` so client-side created-by-me filtering can work with API-loaded profile events.

## Architecture
| Layer | File(s) | Responsibility |
|---|---|---|
| Domain | `Domain/Activity.cs` | Stores creator display name and optional creator person relationship. |
| Application | `Application/Profiles/Queries/GetUserProfile.cs` | CQRS query handler, profile activity filtering, DTO mapping. |
| Persistence | `Persistence/AppDbContext.cs` | EF Core activity storage used by the query handler. |
| API | `API/Controllers/ProfilesController.cs` | HTTP profile endpoint delegating to MediatR. |
| Frontend | `client/src/feature/profile/UserProfilePage.tsx` | Renders profile event lists and client-side filters. |

## API Contract
**Endpoint**: `GET /api/profiles/{username}`

**Response**:
```json
{
  "username": "jeff",
  "displayName": "Jeff",
  "avatarUrl": "/images/jeff-placeholder.svg",
  "pastEvents": [],
  "futureEvents": [
    {
      "id": "activity-id",
      "title": "Event title",
      "date": "2026-05-16T11:00:00Z",
      "description": "Event description",
      "category": "General",
      "city": "Seattle",
      "venue": "Venue",
      "creatorDisplayName": "Jeff"
    }
  ]
}
```

**Errors**:
- The current endpoint returns an empty profile activity list for unknown users instead of a 404.

## Known Limitations / TODOs
- Profile metadata still uses the shared placeholder avatar until a real user identity source is added.
