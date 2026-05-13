---
id: 0007
title: Linux integration — SSH transport and per-container backup recipes
status: accepted
date: 2026-04-27
scope: sources
---

# 0007 — Linux integration via SSH.NET with per-container recipes

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

## Alternatives considered
- **Shelling out to `ssh.exe`** — works, but parsing stdout for structured
  failure modes is lossy.
- **Recipes as YAML/JSON config** — premature DSL; C# functions are a
  better config language for one developer.

## Consequences
- Adding a new container family is a code change, not config — acceptable
  for a single-developer tool.
- Interactive SSH password auth is unsupported in v1 (key auth is required).
- Partial failures are first-class: each Source's run produces a result
  independently; the dashboard aggregates them.
- SSH.NET is pure managed code with no native dependencies; works fine on
  Windows.
- Storing only the path to the private key keeps secrets out of the
  database.
