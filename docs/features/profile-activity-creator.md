# Feature: Profile Activity Creator Metadata
**Status**: complete
**Date**: 2026-05-24
**Author**: Claude Code

## Overview
Profile activity responses include creator display names so clients can identify and filter activities created by the profile owner.

## Business Logic
- `Application/Profiles/Queries/GetUserProfile.cs` filters profile activities by `Activity.CreatorDisplayName` when creator metadata exists.
- `Application/Profiles/Queries/GetUserProfile.cs` preserves `CreatorDisplayName` in every returned profile activity DTO.
- If no matching creator data exists, the profile query keeps demo behavior by falling back to all activities.

## Architecture
| Layer | File(s) | Responsibility |
|---|---|---|
| Domain | `Domain/Activity.cs` | Stores creator display metadata |
| Application | `Application/Profiles/Queries/GetUserProfile.cs` | Builds profile activity DTOs |
| API | `API/Controllers/ProfilesController.cs` | Exposes profile data over HTTP |
| Frontend | `client/src/app/layout/App.tsx` | Loads profile data and applies profile activity filters |

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
      "id": "guid",
      "title": "Activity title",
      "date": "2026-05-24T11:00:00Z",
      "description": "Activity description",
      "category": "Tech",
      "creatorDisplayName": "Jeff",
      "city": "Boston",
      "venue": "Hub"
    }
  ]
}
```

**Errors**:
- Standard API error handling applies for unexpected server failures.

## Known Limitations / TODOs
- Profile identity is currently display-name based for demo data.
