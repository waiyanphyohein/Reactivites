# Save Context: $ARGUMENTS

Offload accumulated context to a file in `docs/context/` to free up the active context window.

## Steps

1. **Determine the category** based on the content type:

   | Category | Folder | Use when content is… |
   |---|---|---|
   | `text` | `docs/context/text/` | Research notes, free-form analysis, general notes |
   | `specs` | `docs/context/specs/` | Feature specs, requirements, design decisions |
   | `errors` | `docs/context/errors/` | Error logs, debugging sessions, stack traces |
   | `api` | `docs/context/api/` | API contracts, DTO shapes, endpoint inventories |

2. **Name the file** using `context_$ARGUMENTS.md` (kebab-case, lowercase).
   Example: `/save-context activity-export-research` → `docs/context/text/context_activity-export-research.md`

3. **Create the file** with this header:
   ```md
   # Context: <Descriptive Title>
   **Session**: $ARGUMENTS
   **Date**: YYYY-MM-DD
   **Category**: text | spec | error | api
   **Summary**: One sentence describing what this file contains.

   ---

   <content goes here>
   ```

4. **Replace the in-context copy** of the content with a one-line reference:
   > _Context saved to `docs/context/<category>/context_$ARGUMENTS.md`_

5. **Continue working** — re-read the file only when specifically needed.

## When to Use

- Content exceeds ~150 lines and is not immediately needed for the next step
- You are switching to a new sub-task and the previous one produced significant output
- The user says "remember this", "save this for later", or "store this"
- You are about to analyse a large error log, long file listing, or bulk output

## Naming Examples

| Content | File path |
|---|---|
| Research on EF Core TPH patterns | `docs/context/text/context_ef-core-tph-research.md` |
| Spec for user authentication feature | `docs/context/specs/context_user-auth-spec.md` |
| EF Core migration crash analysis | `docs/context/errors/context_ef-migration-crash.md` |
| Activities API contract | `docs/context/api/context_activities-api.md` |
| Events controller endpoint list | `docs/context/api/context_events-api.md` |

## After Saving

Always tell the user:
> ✅ Saved to `docs/context/<category>/context_<name>.md` — summary: <one sentence>
