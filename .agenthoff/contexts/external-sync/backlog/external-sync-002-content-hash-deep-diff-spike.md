---
id: external-sync-002-content-hash-deep-diff-spike
title: Spike — content-hash deep-diff vs robocopy timestamps for Vault sync
type: spike
status: backlog
bc: external-sync
scope: external-sync
depends_on: [external-sync-001-veracrypt-and-robocopy]
created: 2026-04-27
---

# Spike: content-hash deep-diff for Vault sync

## Why
ADR-0008 picks `robocopy /MIR /L` (timestamp + size) as the v1 diff engine
for External Sync, with content-hash deep compare explicitly deferred. The
vision flagged this open question: timestamp-based diffs are fast but blind
to silent on-disk corruption that preserves size and mtime. Before
hardening v1 we should measure whether a hash-based pass is worth it for
Marco's actual data shapes (Notion HTML archive, Gmail mbox, Docker volume
tarballs, Drive folder mirrors).

## What
A time-boxed research task (~half day) producing:
- A measurement of robocopy `/MIR /L` runtime against a representative
  Drive-folder snapshot (multi-GB, mixed file sizes).
- A measurement of a content-hash pass (BLAKE3 or xxHash3 over the same
  tree) on the same data.
- A short writeup of the runtime delta, the failure modes each strategy
  catches/misses, and a recommendation: keep robocopy-only, add an
  opt-in deep-verify mode, or switch defaults.
- If the recommendation is to add anything, a follow-up implementation
  task captured in this BC's `backlog/`.

## Acceptance criteria
- [ ] Measurements recorded against a real or representative dataset
      (size, file count, runtime).
- [ ] Writeup committed under `.agenthoff/knowledge/research/` summarising
      findings and recommendation.
- [ ] Either an implementation follow-up task is filed, or the spike
      explicitly closes with "robocopy-only stays" and updates ADR-0008's
      status note.
- [ ] No production code change required by this spike itself.
