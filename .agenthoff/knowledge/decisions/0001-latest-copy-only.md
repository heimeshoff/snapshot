---
id: 0001
title: Latest copy only — no version history of backups
status: accepted
date: 2026-04-27
scope: global
---

# 0001 — Latest copy only, no version history

## Context
Each Source's backup ends up in the Drive folder. Two design options were on the table:

- **Option A (latest only):** Each Backup run overwrites the previous file. Snapshot keeps a metadata log of operations (when, how big) but the Drive folder only ever holds the most recent copy.
- **Option B (versioning):** Each Backup run produces a separate dated artifact. Snapshot would have to manage retention, pruning, deduplication, and a restore flow.

## Decision
**Option A.** Latest copy only. The "history" exposed in the UI is purely a log of backup operations and their sizes — not a tree of past data.

## Consequences
- Storage footprint stays small. Design stays simple.
- No restore-from-Tuesday capability. If corruption happens upstream and propagates to the Drive folder before the next Sync run, the Vault inherits the corruption too. Marco accepts this risk for a personal tool.
- Google Drive's own file-version retention (≈30 days for owned files) is the only built-in safety net for accidental overwrites; Snapshot does not add another.
- The Vault is a redundant copy of "now," not a time machine.
- If Marco ever wants versioning, it would be a major redesign — not a swappable detail. We are explicitly choosing not to leave that door open.
