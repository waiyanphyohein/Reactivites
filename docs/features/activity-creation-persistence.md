# Feature: Activity Creation Persistence
**Status**: complete
**Date**: 2026-06-01
**Author**: Claude Code

## Overview
Activity creation from the React UI now persists through the backend API before the new activity is shown in the list or profile event collections.

## Business Logic
- `client/src/app/layout/App.tsx` posts submitted activities to `POST /api/activities`.
- The UI adds an activity to local state only after the API returns successfully.
- `client/src/feature/activities/form/ActivityForm.tsx` keeps entered form values and displays an error if the save fails.
- Profile event state is updated only for successfully persisted activities created by the current user.

## Architecture
| Layer | File(s) | Responsibility |
|---|---|---|
| API | `API/Controllers/AcitivitiesController.cs` | Accepts `POST /api/activities` and delegates to MediatR |
| Application | `Application/Activities/Commands/CreateActivity.cs` | Persists activities and creator metadata |
| Frontend | `client/src/app/layout/App.tsx` | Calls the API and updates UI state after persistence |
| Frontend | `client/src/feature/activities/form/ActivityForm.tsx` | Submits activity data and reports save failures |

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
  "date": "2026-11-12T18:30:00",
  "city": "New York",
  "venue": "Innovation Loft",
  "creatorDisplayName": "Jeff"
}
```
**Errors**:
- `400 Bad Request` — invalid activity payload
- `500 Internal Server Error` — persistence failure

## Known Limitations / TODOs
- Authentication is demo/local-storage based; creator identity is still supplied by the client.
