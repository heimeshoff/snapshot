---
id: 0005
title: Inter-context transport — shared substrate, no bus
status: accepted
date: 2026-04-27
scope: global
---

# 0005 — No inter-context message bus

## Context
Snapshot has three bounded contexts (Sources, External Sync, Archive
Exploration) that share data but not behavior. The temptation in DDD
toolings is to introduce a message bus between contexts even when the
contexts coexist in one process.

## Decision
No inter-context message bus, no in-process domain-event dispatch between
contexts, no HTTP between contexts. The contexts share two substrates:
the Drive folder (filesystem) and the Snapshot SQLite database. Sources
owns writes to both; External Sync and Archive Exploration are read-only
consumers of a small shared query surface.

## Consequences
- One executable, one process, one database. Simple deployment, simple
  reasoning.
- If a future context genuinely needs reactive notifications (e.g., "auto
  re-render archive when artifact changes"), revisit. Not a v1 concern.
- Each BC still owns its own module boundary in code; "shared substrate"
  is not "shared model" — Sources types do not leak into the other BCs.
