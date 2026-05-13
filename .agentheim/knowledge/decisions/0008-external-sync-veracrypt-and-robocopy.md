---
id: 0008
title: External Sync mechanics — VeraCrypt CLI mount and robocopy diff/copy
status: accepted
date: 2026-04-27
scope: external-sync
---

# 0008 — External Sync mechanics: VeraCrypt CLI + robocopy

## Context
External Sync mirrors the Drive folder onto a VeraCrypt-encrypted USB Vault
on Windows. Two mechanics need decisions: (1) how Snapshot drives VeraCrypt,
(2) what diff and copy engine produces the sync preview and the sync run.

## Decision
- VeraCrypt: shell out to `veracrypt.exe` for mount, dismount, and status.
  Password is collected from the UI per session, passed to the child process,
  never persisted, and zeroed in memory after handoff.
- Diff and copy: use `robocopy` for v1.
  - Sync preview: `robocopy /MIR /L` parsed for added / removed / modified.
  - Sync run: `robocopy /MIR /COPY:DAT /DCOPY:T` after user confirmation.
- Content-hash deep compare is **deferred** to a future research spike
  (`external-sync-002-content-hash-deep-diff-spike`). If v1 reveals
  timestamp-based diffs miss real changes, revisit.

## Alternatives considered
- **Mounting VeraCrypt programmatically via a library** — none reliable for
  .NET; the CLI is the supported automation contract on Windows.
- **rsync via WSL** — adds a WSL dependency; robocopy is native and
  sufficient for v1.
- **Always content-hash** — punishes a multi-GB Notion archive on every
  sync; not justified until measured.

## Consequences
- VeraCrypt remains a separately-installed dependency Marco supplies; not
  bundled.
- Diff fidelity is timestamp- and size-based in v1. Silent on-disk
  corruption that preserves both will not be caught — accepted given
  ADR-0001's "latest copy only" posture.
- Robocopy output parsing is brittle; **isolate it in one module** so a
  future engine swap (e.g., content-hash diff, rsync, custom walker) touches
  one place. The exit-code semantics (0 = nothing copied, 1 = files copied,
  >=8 = error) and the per-file/summary output format are the seam.
- Password handling: the UI collects the Vault password per session, hands
  it to `veracrypt.exe` via the documented CLI mechanism, and zeros the
  in-memory buffer after the child process accepts it. No persistence, no
  config-file fallback.
