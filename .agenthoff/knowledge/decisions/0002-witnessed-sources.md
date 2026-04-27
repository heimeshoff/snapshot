---
id: 0002
title: Notion and Gmail are witnessed, not orchestrated
status: accepted
date: 2026-04-27
scope: global
---

# 0002 — Notion and Gmail backups are witnessed, not triggered

## Context
The original wish was for Snapshot to fully orchestrate every Source — including kicking off Notion exports and Google Takeout. However:

- **Google Takeout** runs on a fixed bimonthly schedule and exposes no public API to start a download on demand.
- **Notion** has no API endpoint to trigger a workspace export.

The only ways to fake "Snapshot triggers a Notion / Gmail backup" would be browser automation or scraping — both fragile and disproportionate for a personal tool.

## Decision
Snapshot treats Notion and Gmail as **Witnessed Sources**. Their freshness is determined by reading file timestamps in the Drive folder where the exports land. When they go stale, the dashboard nudges Marco to manually trigger the export elsewhere; Snapshot itself does nothing automated.

## Consequences
- The Source concept must support two modes from day one: **Driven** (Snapshot runs the backup) and **Witnessed** (Snapshot only reads timestamps).
- The "back up stale sources" action only operates on Driven Sources. Stale Witnessed Sources surface as manual-action nudges.
- If Notion or Google ever expose APIs that change this, a Witnessed Source can be flipped to Driven without restructuring the model.
- We will not invest in browser automation or scraping to close the gap. If the constraint changes, the API is the trigger; otherwise Marco is the trigger.
