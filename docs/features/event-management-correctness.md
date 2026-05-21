# Feature: Event Management Correctness
**Status**: complete
**Date**: 2026-05-21
**Author**: Claude Code

## Overview
Event detail, update, and delete operations use the public `EventId` identifier exposed by API routes and response payloads. Startup seeding now avoids adding duplicate seed data when any core table already contains data.

## Business Logic
- `GetEventDetails.Handler` retrieves events by `Event.EventId`, not the inherited `GroupId` primary key.
- `EditEvent.Handler` updates the event whose `EventId` matches the request payload.
- `DeleteEvent.Handler` deletes the event whose `EventId` matches the route value.
- `DbInitializer.Initialize` skips automatic seeding when activities, events, people, tags, or groups already exist.

## Architecture
| Layer | File(s) | Responsibility |
|---|---|---|
| Domain | `Domain/Event.cs`, `Domain/Group.cs` | Event inherits group fields while exposing a separate public event identifier. |
| Application | `Application/Events/Queries/GetEventDetails.cs` | Retrieve a single event by public `EventId`. |
| Application | `Application/Events/Commands/EditEvent.cs` | Update a single event by public `EventId`. |
| Application | `Application/Events/Commands/DeleteEvent.cs` | Delete a single event by public `EventId`. |
| Persistence | `Persistence/DbInitializer.cs` | Seed only an empty database unless clearing is explicitly requested. |
| API | `API/Controllers/EventsController.cs` | Exposes event routes keyed by `EventId`. |

## API Contract
**Endpoint**: `GET /api/events/{eventId}`
**Response**:
```json
{ "eventId": "guid", "eventName": "string" }
```
**Errors**:
- `404 Not Found` - event does not exist.

**Endpoint**: `PUT /api/events/{eventId}`
**Request body**:
```json
{ "eventId": "guid", "eventName": "string" }
```
**Response**: `204 No Content`
**Errors**:
- `400 Bad Request` - route and body IDs differ.
- `404 Not Found` - event does not exist.

**Endpoint**: `DELETE /api/events/{eventId}`
**Response**: `204 No Content`
**Errors**:
- `404 Not Found` - event does not exist.

## Known Limitations / TODOs
- The EF model still uses inherited `GroupId` as the table primary key for events.
