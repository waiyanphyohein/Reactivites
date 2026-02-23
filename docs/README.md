# docs/

This directory is maintained by Claude Code and human developers together.
All files here are committed to source control and serve as the project's living documentation.

---

## Directory Structure

```
docs/
├── README.md                       # This file
├── logs/                           # Claude Code action audit trail
│   └── actions_YYYY-MM-DD.md       # One file per calendar day
├── context/                        # Offloaded context documents (saves AI context window)
│   ├── text/                       # Free-form research notes, analysis
│   │   └── context_<name>.md
│   ├── specs/                      # Feature specs, requirements, design decisions
│   │   └── context_<name>.md
│   ├── errors/                     # Debugging sessions, error logs, stack traces
│   │   └── context_<name>.md
│   └── api/                        # API contracts, DTO shapes, endpoint inventories
│       └── context_<name>.md
└── features/                       # Developer-facing feature documentation
    └── <feature-name>.md
```

---

## `logs/` — Action Logs

**Purpose**: Every meaningful step Claude Code takes is logged here for auditability, rollback guidance, and replay.

**Format**: `actions_YYYY-MM-DD.md` — one file per calendar day.

**When to use**: Read these files when you need to:
- Understand what was done in a session
- Roll back a specific step
- Reproduce a session on a different branch

**Rollback**: Each entry contains an explicit `**Rollback**` field with the exact command or action needed to undo that step.

---

## `context/` — Offloaded Context Documents

**Purpose**: When Claude Code accumulates large volumes of text during a session (research, error logs, specs, API contracts), it offloads that content here instead of holding it in the active context window. This prevents context window exhaustion on long sessions.

**Sub-folders**:
| Folder | Content type |
|---|---|
| `text/` | Research notes, free-form analysis, general notes |
| `specs/` | Feature specs, requirements, design decisions |
| `errors/` | Error logs, debugging sessions, stack traces |
| `api/` | API contracts, DTO shapes, endpoint inventories |

**Naming**: `context_<kebab-case-description>.md`

**How to use**: If Claude Code references a file here, read it with your editor or ask Claude Code to re-read it before continuing the relevant task.

---

## `features/` — Feature Documentation

**Purpose**: Every completed feature gets a human-readable document here. These are written by Claude Code immediately after implementation and serve as the primary reference for future developers.

**What each doc contains**:
- Overview (what the feature does and why)
- Business logic (rules, validations, constraints — with file references)
- Architecture table (which file in which layer does what)
- API contract (endpoint, request, response, error codes)
- Known limitations / TODOs

**Naming**: `<feature-name>.md` in kebab-case — e.g., `activity-export.md`, `user-registration.md`

**When to read**: Before modifying an existing feature, read its doc to understand the business rules. If the rules change, update the doc.

---

## Conventions

- All files in `docs/` are committed to git — they are part of the project, not temporary scratch space
- `logs/` files are written during sessions and must not be edited manually while Claude Code is active
- `context/` files may be read freely; only Claude Code writes them during active sessions
- `features/` files should be kept up to date — if you change a feature's behaviour, update its doc
