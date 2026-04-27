---
id: foundation-002-persistence-sqlite
title: Persistence and local settings layout
type: decision
status: todo
bc: foundation
scope: global
depends_on: [foundation-001-stack-and-form-factor]
created: 2026-04-27
---

# Decision: Persistence and local settings layout

## Recommendation
**Two storage locations with distinct responsibilities.**

- **Operational data** — Sources registry, backup-run log, sync-run log — in **SQLite** via `Microsoft.Data.Sqlite`. Single database file at a **user-configurable path**, defaulting to `%USERPROFILE%\Documents\Snapshot\snapshot.db`. No ORM; hand-rolled queries in a thin repository module.
- **Per-machine UI / runtime preferences** — chosen DB path, window position, theme — in a flat JSON file at `%LOCALAPPDATA%\Snapshot\settings.json`, read/written via `System.Text.Json` with atomic write (write-to-`.tmp`, rename-over).

First-run is silent: defaults are written, the DB folder is created, schema is initialized, the main window opens. The DB path is editable on the Settings page; changes take effect on the next restart.

## Why
- Tiny dataset, but "size history as a small log" is already a relational shape (one row per run per Source). A flat JSON file becomes painful when you want "show me last N runs of container X."
- SQLite is single-file, zero-install, transactional, ships in .NET. Fits the personal-tool ethos.
- Hand-rolled queries over an ORM because the schema is small (~3 tables); a thin reader is enough.
- **Splitting operational data from machine-local UI preferences** keeps each in the right place: the DB is data Marco may want to back up, move between machines, or relocate to a non-system drive; the settings file is per-machine configuration he should never need to think about.
- Default DB path stays out of `%LOCALAPPDATA%` so the file is visible in Explorer and trivially relocatable.
- Default DB path stays out of any Google Drive folder because live SQLite inside a sync client is a known corruption vector (Drive client mishandles `-wal`/`-shm` sidecar files and races SQLite's locking). Marco can opt in via Settings; the app warns but does not block.

## Alternatives considered
- **Single fixed `%LOCALAPPDATA%\Snapshot\snapshot.db`** (the previous draft) — invisible in Explorer, awkward to relocate, mixes operational data with disposable cache-style content. Rejected: Marco explicitly wants the DB movable.
- **DB inside the Google Drive folder by default** — convenient for "backed up automatically" but causes SQLite lock conflicts and `-wal`/`-shm` corruption when the sync client races the database engine. Rejected as default; available as a user override with warning.
- **Settings stored inside the SQLite DB** — chicken-and-egg: you need the DB path to open the DB, so the path can't live in the DB. Co-locating window/theme with the path config in `settings.json` keeps the layout coherent.
- **Roaming `%APPDATA%`** — wrong for `databasePath` (machines have different drive layouts), wrong for window geometry (multi-monitor differences). Per-machine `%LOCALAPPDATA%` is the right home.
- **JSON file for run log** — fine for the registry (~10 rows), but the run log will grow and want indexed reads; ends up reinventing SQLite badly.
- **LiteDB** — embedded NoSQL alternative, but introduces a less-known engine for no win over SQLite on this shape of data.
- **EF Core** — overshoot for this size; migration ceremony Marco doesn't need.

## Acceptance criteria
- [ ] ADR committed at `.agenthoff/knowledge/decisions/0004-persistence-and-local-settings-layout.md` with `scope: global`.
- [ ] ADR justification matches the draft below (or Marco's amended version).
- [ ] ADR explicitly documents: default DB path, `settings.json` location and v1 schema, missing/corrupt-settings handling, first-run flow, and the Drive-folder corruption footgun.
- [ ] No code change required by this task — implementation lands with `foundation-005`.

## Notes (architect's ADR draft)

```markdown
## Context
Snapshot needs to persist two distinct kinds of state:

1. **Operational data** — a small registry of Sources (id, kind, driven /
   witnessed, staleness window, last-backup metadata) plus an append-only
   log of backup runs and sync runs. Relational, queried by the dashboard.
2. **Per-machine UI / runtime preferences** — where the operational
   database lives on disk, the main window's position and size, the user's
   theme preference. Tiny, flat, machine-local.

The app is single-user, single-process, on Windows. The user has explicitly
asked that the operational database be **relocatable** — he wants to choose
where it lives (e.g., a folder that is itself backed up, a non-system
drive) rather than have it buried in `%LOCALAPPDATA%`.

## Decision

### Operational data — SQLite
Use SQLite via `Microsoft.Data.Sqlite`. Single database file at a path
chosen by the user, defaulting to
`%USERPROFILE%\Documents\Snapshot\snapshot.db` on first run. Schema kept
minimal: `sources`, `backup_runs`, `sync_runs`. No ORM; thin repository
module with hand-written SQL. Migrations are manual SQL scripts shipped
with the app, applied on startup, idempotent.

The default deliberately avoids:
- `%LOCALAPPDATA%`, because the user wants the file visible and movable;
- inside the Google Drive sync folder, because Google Drive's sync client
  is known to corrupt actively-written SQLite databases (locks against
  the DB file, mishandles `-wal` / `-shm` sidecar files). Users may opt
  into a Drive location via Settings; the app should warn but not block.

### Per-machine preferences — settings.json
Use a JSON file at `%LOCALAPPDATA%\Snapshot\settings.json`, read and
written via `System.Text.Json`. Atomic write (write to `.tmp`, rename
over). Schema (v1):

    {
      "schemaVersion": 1,
      "databasePath": "<absolute path>",
      "window": { "left": 0, "top": 0, "width": 0, "height": 0, "maximized": false },
      "theme": "light" | "dark" | "system"
    }

If the file is missing, treat as first run: instantiate defaults, create
the DB folder, initialize the DB schema, write the settings file. If the
file is corrupt or has a higher `schemaVersion` than the binary
understands, rename it to `settings.json.broken-<utc>`, log a warning,
and proceed as first run.

### First-run flow
Silent. No dialog. The default DB path is created on disk and persisted
to settings.json without prompting. The user can change the DB path on
the Settings page at any time; changes take effect on next restart.

If the default DB folder cannot be created (e.g., `Documents\` is on a
removed network drive), THEN — and only then — fall back to a folder-
picker dialog. The happy path stays silent.

### Settings access
A singleton `ISettingsService` is loaded in `App.OnStartup` before any
DB connection or main window. It exposes a current snapshot and a
`Save(SnapshotSettings)` method. `MainWindow` persists window geometry on
close; the Settings page persists `databasePath` and `theme` on commit.
Changing `databasePath` does not move the existing DB file in v1; the
user is prompted to restart for the new path to take effect.

### Drive-folder warning
The Settings page DB-path editor surfaces a static informational note:
"Avoid placing the database inside a Google Drive (or other cloud-sync)
folder — sync clients can corrupt live SQLite files." A heuristic active
detection (walking parents for known Drive markers) is a v2 nice-to-have.

## Consequences
- Operational data lives where the user expects user-data to live; he can
  relocate it to a backed-up drive, a different physical disk, or
  (against advice) into Google Drive.
- The settings file remains per-machine and never roams; window geometry
  for one monitor doesn't follow the user to a different machine.
- First-run UX is zero-friction — open the .exe, see the dashboard.
- Changing the DB path is a one-restart operation. v1 does not move the
  existing DB file automatically; the user can copy it manually.
- No multi-process concurrency story is needed.
- Footgun: the user can move the DB into a Drive-synced folder and shoot
  himself in the foot. The Settings page surfaces a warning; the choice
  is his.
- `%LOCALAPPDATA%\Snapshot\` will hold `settings.json` plus the existing
  `logs\` subdirectory (per ADR-0006). Clean layout, no overlap with the
  operational DB.
```
