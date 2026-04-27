---
id: sources-001-linux-ssh-and-recipes
title: Linux integration — SSH transport and per-container backup recipes
type: decision
status: todo
bc: sources
scope: sources
depends_on: [foundation-001-stack-and-form-factor]
created: 2026-04-27
---

# Decision: Linux integration mechanism

## Recommendation
**SSH.NET (Renci.SshNet) for SSH/SFTP from C#. Auth via SSH key file referenced by path in the Source registry; password and interactive auth not supported in v1. Per-container "backup recipes" live as small C# functions in the Sources BC, registered in a typed map keyed by container family.**

## Why
- SSH.NET is the de-facto .NET SSH library, pure managed code, no native deps. Works fine on Windows.
- Key auth is what Marco already does; storing a path (not the key) keeps secrets out of the database.
- Recipes-as-code lets each container family encode its own "stop / dump volume / restart" sequence with proper types, rather than a YAML DSL we'd have to evolve.

## Alternatives considered
- **Shelling out to `ssh.exe`** — works, but parsing stdout for structured failure modes is lossy.
- **Recipes as YAML/JSON config** — premature DSL; C# functions are a better config language for one developer.

## Acceptance criteria
- [ ] ADR committed at `.agenthoff/knowledge/decisions/0007-sources-linux-ssh-and-recipes.md` with `scope: sources`.
- [ ] ADR justification matches the draft below (or Marco's amended version).
- [ ] No code change required by this task.

## Notes (architect's ADR draft)

```markdown
## Context
The Sources BC needs to back up Docker volumes on a Linux host. Each
container family has its own "stop / snapshot / restart" sequence. Snapshot
needs to reach the host, run commands, transfer the resulting artifact
into the Drive folder, and surface partial-failure results.

## Decision
- SSH transport via `SSH.NET` (Renci.SshNet).
- Authentication: SSH key. The Source registry stores the path to the
  private key on disk; the key itself is never read into the database.
- Per-container "backup recipes" are C# functions in the Sources BC,
  registered in a typed map keyed by container family. Each recipe
  receives an SSH session and returns a `Result<BackupArtifact, BackupError>`.
- Artifact transfer uses SFTP from the same SSH.NET session into the
  Drive folder.

## Consequences
- Adding a new container family is a code change, not config — acceptable
  for a single-developer tool.
- Interactive SSH password auth is unsupported in v1 (key auth is required).
- Partial failures are first-class: each Source's run produces a result
  independently; the dashboard aggregates them.
```
