# Codex Prescreening Rules

## Purpose
This file captures pre-screening rules to apply before any new feature iteration or revision.

## Mandatory Prescreening Checklist
1. Confirm architecture alignment:
   - Backend: Clean Architecture (`Domain` -> `Application` -> `Persistence` -> `API`)
   - CQRS with MediatR for new read/write use cases
2. Confirm naming and API conventions:
   - C# types and members use PascalCase
   - API JSON output remains camelCase
   - React/TypeScript props and state are strongly typed
3. Confirm UI/theme consistency:
   - Reuse existing gradient and Material UI styling patterns
   - Keep navigation, card spacing, and typography consistent with current screens
4. Confirm data contract placement:
   - API contracts/DTOs live in `Application` query/command scope unless shared models are clearly required
   - Frontend types are updated in `client/src/lib/types/index.d.ts`
5. Confirm traceability:
   - Add/adjust tests for changed behavior (frontend and/or backend)
   - Update docs for any feature-level behavior changes

## Rules from Previous Iterations
- Keep controllers thin; delegate behavior to MediatR handlers.
- Prefer additive changes and avoid breaking existing activity flows.
- Keep action logging and documentation under `docs/` updated when feature behavior evolves.
- Use dummy or fallback data only when required, but keep API shape production-ready.
- Preserve established color theme and layout patterns for newly introduced pages.

## Revision Gate (Pre-Merge)
1. Build and test pass for touched layers.
2. New endpoint and UI states have test coverage.
3. Navigation works for both old and new views.
4. No regressions to existing activity list/detail/form behavior.
5. Feature docs and this rules file remain accurate for the latest revision.
