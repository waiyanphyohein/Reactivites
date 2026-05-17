# Feature: Activity Creation
**Status**: complete
**Date**: 2026-05-17
**Author**: Claude Code

## Overview
Activity creation lets an authenticated user submit a new activity from the React form and persists it through the API before updating local UI state.

## Business Logic
- The activity form builds a complete activity payload with a client-generated id and the current creator display name.
- The app posts the payload to `POST /api/activities` before rendering the new activity.
- If the API call fails, the form keeps the entered data and shows an error instead of pretending the activity was saved.

## Architecture
| Layer | File(s) | Responsibility |
|---|---|---|
| Domain | `Domain/Activity.cs` | Activity entity definition |
| Application | `Application/Activities/Commands/CreateActivity.cs` | Persists activity and maps creator display name to a person |
| API | `API/Controllers/AcitivitiesController.cs` | Activity create endpoint |
| Frontend | `client/src/app/layout/App.tsx`, `client/src/feature/activities/form/ActivityForm.tsx` | Submit payload, handle API response, update UI state |

## API Contract
**Endpoint**: `POST /api/activities`
**Request body**:
```json
{ "id": "guid", "title": "Activity title", "date": "2026-10-12T14:30:00Z", "city": "Boston", "venue": "Downtown Hub" }
```
**Response**:
```json
{ "id": "guid", "title": "Activity title", "date": "2026-10-12T14:30:00Z", "city": "Boston", "venue": "Downtown Hub" }
```
**Errors**:
- `400 Bad Request` - activity payload is invalid
- `500 Internal Server Error` - activity could not be persisted

## Known Limitations / TODOs
- The frontend still uses the current lightweight login state as the creator source.
