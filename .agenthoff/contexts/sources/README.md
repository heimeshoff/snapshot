# Sources

## Purpose
Track every backup target, its freshness, and execute the backup itself for the Sources Snapshot controls.

## Classification
Core

## Ubiquitous language
- **Source** — a single thing Snapshot tracks for backup. Each Docker container is its own Source. Notion, Gmail, and other Google Drive subjects can each be Sources.
- **Driven Source** — Snapshot performs the backup itself (Docker containers).
- **Witnessed Source** — Snapshot only reads file timestamps to detect freshness; the backup is produced by something Snapshot can't trigger (Notion, Gmail).
- **Staleness window** — per-Source maximum age before the Source surfaces as stale.
- **Backup run** — a single execution of pushing a Driven Source's data into the Drive folder.
- **The Drive folder** — the local Google Drive folder on the Snapshot host; central junction for all data movement.
- **Stale / Fresh** — derived state per Source from `(now − last backup) vs staleness window`.

## Notes
- The "back up stale sources" action only acts on Driven Sources. Stale Witnessed Sources surface as "go do this manually" prompts.
- For Witnessed Sources, freshness comes from reading file timestamps in the Drive folder.
- This BC owns the dashboard.

## Frontend gate
This BC has UI (the dashboard, Source rows, the staleness pill, the "back up stale sources" action). **Every frontend task captured for this BC must `depends_on: design-system-001-styleguide`.** No feature task that introduces or alters UI in this BC may begin work until the styleguide signoff is recorded.
