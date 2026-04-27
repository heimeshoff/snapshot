---
id: external-sync-001-veracrypt-and-robocopy
title: External Sync mechanics — VeraCrypt CLI + robocopy
type: decision
status: done
bc: external-sync
scope: external-sync
depends_on: [foundation-001-stack-and-form-factor]
created: 2026-04-27
completed: 2026-04-27
---

# Decision: External Sync mechanics

## Recommendation
- **VeraCrypt:** shell out to `veracrypt.exe` for mount/dismount; password collected from the UI, passed to the child process, never persisted, zeroed in memory after handoff.
- **Diff and copy:** for v1, use `robocopy /MIR /COPY:DAT /DCOPY:T /L` in dry-run for the preview, then `robocopy /MIR` for the run.
- **Content-hash deep compare** — **deferred** to a follow-up research spike. The vision flagged this as worth research; provisional v1 answer is timestamps + sizes via robocopy.

## Why
- VeraCrypt's GUI is not scriptable cleanly; CLI is the supported automation path on Windows.
- Robocopy is built into Windows, fast, and `/L` gives a parseable preview without copying. Good enough for v1.
- Hash-diff is more correct (catches silent corruption) but expensive on multi-GB archives and unnecessary if timestamps/sizes match. Worth measuring before committing.

## Alternatives considered
- **Mounting VeraCrypt programmatically via a library** — none reliable for .NET; the CLI is the contract.
- **rsync via WSL** — adds WSL dependency; robocopy is native and sufficient.
- **Always content-hash** — punishes a 50GB Notion archive on every sync.

## Acceptance criteria
- [x] ADR committed at `.agenthoff/knowledge/decisions/0008-external-sync-veracrypt-and-robocopy.md` with `scope: external-sync`.
- [x] ADR justification matches the draft below (or Marco's amended version).
- [x] Robocopy output parsing isolated in one module so a future engine swap touches one place. (Documented in ADR; no implementation required by this task.)
- [x] Follow-up research task captured for "content-hash deep-diff" — filed as `external-sync-002-content-hash-deep-diff-spike` in this BC's `backlog/`.
- [x] No code change required by this task.

## Notes (architect's ADR draft)

```markdown
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
- Content-hash deep compare is **deferred** to a future research task; if
  v1 reveals timestamp-based diffs miss real changes, revisit.

## Consequences
- VeraCrypt remains a separately-installed dependency Marco supplies; not
  bundled.
- Diff fidelity is timestamp- and size-based in v1. Silent on-disk
  corruption that preserves both will not be caught — accepted given
  ADR-0001's posture.
- Robocopy output parsing is brittle; isolate it in one module so a future
  engine swap touches one place.
```

## Outcome
- ADR written: `.agenthoff/knowledge/decisions/0008-external-sync-veracrypt-and-robocopy.md`.
- Follow-up spike filed: `external-sync-002-content-hash-deep-diff-spike` in `backlog/`.
- BC README already references this decision and the deferred deep-diff spike (no README change needed).
