---
id: foundation-004-cross-cutting-concerns
title: Cross-cutting — secrets, logging, partial-failure semantics
type: decision
status: done
bc: foundation
scope: global
depends_on: [foundation-002-persistence-sqlite]
created: 2026-04-27
completed: 2026-04-27
---

# Decision: Cross-cutting concerns

## Recommendation
- **Secrets:** VeraCrypt password collected per-session via UI, passed to the child process, never persisted, wiped from memory after handoff. SSH key paths stored in the SQLite registry; key material never read into DB. No Google credentials handled — Drive folder is a passive synced directory.
- **Logging:** Two layers. (a) Operational app log to `%LOCALAPPDATA%\Snapshot\logs\snapshot-YYYYMMDD.log` via Serilog with daily rolling — for debugging Snapshot itself. (b) Domain log of backup-run / sync-run outcomes lives in SQLite (`backup_runs`, `sync_runs`). Dashboard reads from the database, not log files.
- **Partial failures:** Each Driven Source's backup run is its own independent transaction. "Back up stale sources" runs them sequentially (or with bounded concurrency), persists each as its own row with `outcome ∈ {ok, failed}`. A run that fails after producing a partial artifact writes it as `<source>.partial` and leaves the prior good copy in place. No automatic retries in v1; per-Source "retry" button on the dashboard.

## Why
- Two-layer logging keeps "did Snapshot crash?" separate from "what happened to my backups?" — diagnostic vs. domain history.
- Per-Source isolation matches the domain (each Source is its own thing); one container failure doesn't poison the rest.
- `.partial` naming preserves the ADR-0001 guarantee that the Drive folder always holds a valid latest copy.

## Alternatives considered
- **Backup-run log in flat files** — loses queryability the dashboard needs.
- **Automatic retry with backoff** — overshoot for a weekly-summoned tool; manual retry is clearer.
- **VeraCrypt password in Windows Credential Manager** — convenient but contradicts the vision's "password is never stored."

## Acceptance criteria
- [x] ADR committed at `.agentheim/knowledge/decisions/0006-cross-cutting-concerns.md` with `scope: global`.
- [x] ADR justification matches the draft below (or Marco's amended version).
- [x] No code change required by this task.

## Notes (architect's ADR draft)

```markdown
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
```

## Outcome
ADR-0006 written at `.agentheim/knowledge/decisions/0006-cross-cutting-concerns.md` capturing the three cross-cutting decisions: per-session in-memory VeraCrypt password handling, two-channel logging (Serilog app log + SQLite domain log), and per-Source partial-failure isolation with `.partial` debris naming. No code changes; pure decision task.
