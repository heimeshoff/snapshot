---
id: 0006
title: Cross-cutting concerns — secrets, logging, partial-failure semantics
status: accepted
date: 2026-04-27
scope: global
---

# 0006 — Cross-cutting concerns

## Context
Snapshot has three cross-cutting concerns no single BC owns: handling of
in-memory secrets (VeraCrypt password, SSH key paths), application and
domain logging, and behavior on partial failures across multiple Driven
Source backups.

## Decision
**Secrets.** The VeraCrypt password is prompted per Sync run, passed to
`veracrypt.exe` as a child-process argument, and zeroed from memory
immediately after. It is never written to disk or to the database. SSH
keys are referenced by absolute path in the `sources` table; key material
is read by SSH.NET on demand and not cached. No Google credentials are
held by Snapshot — the Drive folder is a passive directory synced by
Google's official client.

**Logging.** Two channels:
- Application log: Serilog, daily-rolling files at
  `%LOCALAPPDATA%\Snapshot\logs\snapshot-YYYYMMDD.log`.
- Domain log: structured rows in SQLite (`backup_runs`, `sync_runs`).
  The dashboard reads the domain log only.

**Partial failures.** Each Driven Source backup is an independent unit of
work. Results are collected per Source and recorded with
`outcome ∈ {ok, failed}` plus an error summary. A run that fails after
producing a partial artifact writes the artifact as `<source>.partial`
and leaves the prior good copy in place. No automatic retries in v1;
the dashboard exposes a per-Source "retry" action.

## Consequences
- Marco re-enters the VeraCrypt password every time. Acceptable trade-off
  for a weekly-or-less ritual; consistent with the vision.
- Domain log being in SQLite means dashboard queries are fast and the log
  participates in the same backup story as the registry (one DB to back
  up if Marco ever wants to back up Snapshot itself).
- Partial-failure semantics keep the Drive folder always-valid: the latest
  *complete* copy is always present, and partial debris is clearly named.
