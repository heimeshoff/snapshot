---
id: foundation-003-no-inter-context-bus
title: Inter-context transport — shared substrate, no bus
type: decision
status: done
bc: foundation
scope: global
depends_on: [foundation-001-stack-and-form-factor, foundation-002-persistence-sqlite]
created: 2026-04-27
completed: 2026-04-27
---

# Decision: Inter-context transport

## Recommendation
**No inter-context message bus, no in-process domain-event dispatch between BCs, no HTTP between contexts. The three contexts share two substrates — the Drive folder (filesystem) and the Snapshot SQLite database — and nothing else. Sources owns writes; External Sync and Archive Exploration are read-only consumers via a small shared query surface.**

## Why
- The contexts genuinely don't communicate behaviorally — they share *data*. None needs to *notify* another.
- One executable, one process, one database. Ceremony-free.
- Module boundaries in code still hold; "shared substrate" ≠ "shared model" — Sources types do not leak into the other BCs.

## Alternatives considered
- **MediatR / in-process event bus** — solves a problem we don't have.
- **Separate processes per BC** — adds IPC for no isolation gain.

## Acceptance criteria
- [x] ADR committed at `.agentheim/knowledge/decisions/0005-no-inter-context-bus.md` with `scope: global`.
- [x] ADR justification matches the draft below (or Marco's amended version).
- [x] No code change required by this task.

## Outcome
ADR 0005 written at `.agentheim/knowledge/decisions/0005-no-inter-context-bus.md` (`scope: global`), reproducing the architect's draft verbatim. Establishes that the three BCs share only the Drive folder and the SQLite database — Sources owns writes; External Sync and Archive Exploration are read-only consumers. No code changes; foundation README untouched (the substrate principle is captured in the global ADR rather than in the BC README, matching the pattern of ADRs 0001/0002).

## Notes (architect's ADR draft)

```markdown
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
```
