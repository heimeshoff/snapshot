# Protocol

Chronological log of everything that happens in this project.
Newest entries on top.

---

## 2026-04-27 15:55 -- Task completed: archive-exploration-001-archive-viewers - Archive viewers — Notion zip rendering and Gmail mbox browser

**Type:** Work / Task completion
**Task:** archive-exploration-001-archive-viewers - Archive viewers — Notion zip rendering and Gmail mbox browser
**Summary:** Published ADR-0009 selecting WebView2 (JS disabled, navigation locked, external requests blocked) for Notion zip rendering, and MimeKit + per-archive SQLite FTS5 sidecar with WPF master-detail UI for Gmail mbox browsing.
**Commit:** TBD
**Files changed:** 1
**ADRs written:** 0009-archive-viewers.md

---

## 2026-04-27 15:50 -- Task completed: foundation-003-no-inter-context-bus - Inter-context transport — shared substrate, no bus

**Type:** Work / Task completion
**Task:** foundation-003-no-inter-context-bus - Inter-context transport — shared substrate, no bus
**Summary:** Published ADR-0005 establishing that the three BCs share only the Drive folder and the SQLite database — no message bus, no in-process event dispatch, no HTTP between contexts.
**Commit:** ac61130
**Files changed:** 1
**ADRs written:** 0005-no-inter-context-bus.md

---

## 2026-04-27 15:40 -- Batch started: [foundation-003-no-inter-context-bus, archive-exploration-001-archive-viewers]

**Type:** Work / Batch start
**Tasks:** foundation-003-no-inter-context-bus - Inter-context transport — shared substrate, no bus; archive-exploration-001-archive-viewers - Archive viewers — Notion zip rendering and Gmail mbox browser
**Parallel:** yes (2 workers)
**Note:** foundation-004 demoted to next batch — both 003 and 004 target the foundation BC README, protocol forbids parallel BC-README updates.

---

## 2026-04-27 15:35 -- Task completed: external-sync-001-veracrypt-and-robocopy - External Sync mechanics — VeraCrypt CLI + robocopy

**Type:** Work / Task completion
**Task:** external-sync-001-veracrypt-and-robocopy - External Sync mechanics — VeraCrypt CLI + robocopy
**Summary:** Published ADR-0008 selecting `veracrypt.exe` CLI for mount/dismount and `robocopy /MIR` (with `/L` preview) as the v1 diff/copy engine; filed a content-hash deep-diff spike as a follow-up backlog item.
**Commit:** cc0abf5
**Files changed:** 2
**ADRs written:** 0008-external-sync-veracrypt-and-robocopy.md
**New backlog items:** external-sync-002-content-hash-deep-diff-spike

---

## 2026-04-27 15:30 -- Task completed: sources-001-linux-ssh-and-recipes - Linux integration — SSH transport and per-container backup recipes

**Type:** Work / Task completion
**Task:** sources-001-linux-ssh-and-recipes - Linux integration — SSH transport and per-container backup recipes
**Summary:** Published ADR-0007 selecting SSH.NET as the Linux transport with key-file auth (path stored, key never read into DB) and per-container backup recipes as typed C# functions in the Sources BC.
**Commit:** 26ee91a
**Files changed:** 1
**ADRs written:** 0007-sources-linux-ssh-and-recipes.md

---

## 2026-04-27 15:25 -- Task completed: foundation-002-persistence-sqlite - Persistence and local settings layout

**Type:** Work / Task completion
**Task:** foundation-002-persistence-sqlite - Persistence and local settings layout
**Summary:** Published ADR-0004 documenting the two-storage-location layout — SQLite via Microsoft.Data.Sqlite at a user-relocatable default path, settings.json under %LOCALAPPDATA% for per-machine preferences, with silent first-run flow and Google-Drive corruption footgun called out.
**Commit:** f54c103
**Files changed:** 1
**ADRs written:** 0004-persistence-and-local-settings-layout.md

---

## 2026-04-27 15:10 -- Batch started: [foundation-002-persistence-sqlite, sources-001-linux-ssh-and-recipes, external-sync-001-veracrypt-and-robocopy]

**Type:** Work / Batch start
**Tasks:** foundation-002-persistence-sqlite - Persistence and local settings layout; sources-001-linux-ssh-and-recipes - Linux integration — SSH transport and per-container backup recipes; external-sync-001-veracrypt-and-robocopy - External Sync mechanics — VeraCrypt CLI + robocopy
**Parallel:** yes (3 workers)

---

## 2026-04-27 15:05 -- Task completed: foundation-001-stack-and-form-factor - Stack, language, framework, form factor

**Type:** Work / Task completion
**Task:** foundation-001-stack-and-form-factor - Stack, language, framework, form factor
**Summary:** Recorded the stack-and-form-factor decision as ADR 0003 (global scope): C# .NET 9 WPF + WPF-UI summoned foreground app mirroring WhisperHeim, no tray, no auto-start.
**Commit:** a40f394
**Files changed:** 1
**ADRs written:** 0003-stack-and-form-factor.md

---

## 2026-04-27 15:00 -- Batch started: [foundation-001-stack-and-form-factor]

**Type:** Work / Batch start
**Tasks:** foundation-001-stack-and-form-factor - Stack, language, framework, form factor
**Parallel:** no (1 worker)

---

## 2026-04-27 14:30 -- Model / Refined: foundation-002 - Persistence and local settings layout

**Type:** Model / Refine
**BC:** foundation
**Status after:** todo
**Summary:** Marco asked for storage to split into two locations: machine-local UI/runtime preferences (window position, theme, *path to the DB*) in `%LOCALAPPDATA%\Snapshot\settings.json`; the SQLite operational DB at a **user-configurable path**, defaulting to `%USERPROFILE%\Documents\Snapshot\snapshot.db` rather than the previous `%LOCALAPPDATA%\Snapshot\snapshot.db`. Architect (via orchestrator) confirmed: silent first-run (no picker dialog on the happy path), `System.Text.Json` for settings with atomic write, `schemaVersion` field, corrupt-file handling renames to `settings.json.broken-<utc>`, default deliberately stays out of any Google-Drive-synced folder (live SQLite + Drive sync is a documented corruption vector). DB path is editable on the Settings page in v1 with a restart prompt; v1 does not move the existing DB file. Task title updated to "Persistence and local settings layout"; ADR target renamed to `0004-persistence-and-local-settings-layout.md`. Cascading edits applied to `foundation-005-walking-skeleton`: scope, settings page stub, acceptance criteria (5 new), and non-goals (3 added). `foundation-004-cross-cutting-concerns` not affected — log files at `%LOCALAPPDATA%\Snapshot\logs\` are unchanged and coexist cleanly with `settings.json`.
**Split into:** none (one ADR covers both halves; treating them as two ADRs would fragment a single layout decision).
**ADRs written:** none yet — drafts attached to task notes; will land via `work` per protocol.

---

## 2026-04-27 13:45 -- Correction: stack rewritten to C# WPF (mirror WhisperHeim)

**Type:** Foundation correction
**Outcome:** four task drafts rewritten; nothing committed yet
**BCs touched:** foundation, archive-exploration, design-system
**Summary:** Marco rejected the architect's stack recommendation (F# / .NET 9 / ASP.NET Core + HTMX). His actual personal-Windows-tool stack is **C# .NET 9 + WPF + WPF-UI** (Fluent / Mica), single-window app with sidebar nav and a system tray icon, mirroring WhisperHeim (`C:\src\heimeshoff\tooling\WhisperHeim`). Four affected task drafts were rewritten in place: `foundation-001` (stack flipped to WPF), `foundation-005` (walking skeleton now describes a WPF window with sidebar + tray, not a browser tab), `archive-exploration-001` (Notion + HTML emails now use WebView2 with JS disabled, not iframe + CSP), `design-system-001` (styleguide is now an in-app "Design System" sidebar page built on WPF-UI tokens, not a `/styleguide` web route). The other five task drafts are stack-neutral and untouched: persistence (SQLite), no-bus, cross-cutting concerns, SSH, VeraCrypt+robocopy. Memory entries saved so the F# guess doesn't recur.
**ADRs written:** none — drafts attached to corrected task notes; will land via `work` per the original protocol.
**Foundation tasks emitted:** unchanged set (foundation-001..005, sources-001, external-sync-001, archive-exploration-001, design-system-001). Walking-skeleton's `depends_on` trimmed to just the four cross-cutting decisions (BC-local decisions block their respective BC implementations, not the spike).

---

## 2026-04-27 13:30 -- Brainstorm: architecture foundation pass

**Type:** Brainstorm (foundation pass)
**Outcome:** vision unchanged; foundation queue created
**BCs identified:** + foundation (cross-cutting), + design-system (cross-cutting)
**Summary:** Re-ran brainstorm under the updated skill version that adds an architecture foundation pass. Vision was already locked from the prior session; only the closing pass executed. Architect (via orchestrator) returned recommendations for stack/form factor, persistence, transport, Linux integration, External Sync mechanics, archive viewers, and cross-cutting concerns. Stack: F# / .NET 9 / ASP.NET Core + HTMX, single console process serving localhost web UI. Persistence: SQLite. Inter-context transport: deliberately none — shared filesystem + DB. Diff strategy for Vault sync provisionally locked to robocopy for v1, with content-hash deep-diff deferred to a follow-up research spike. Two new cross-cutting BCs created (foundation, design-system) to host their respective tasks.
**ADRs written:** none directly — drafts attached to decision tasks instead, to land via `work` as their own commits.
**Foundation tasks emitted:** foundation-001..005, sources-001, external-sync-001, archive-exploration-001, design-system-001 (9 tasks total — 7 decisions, 1 spike, 1 feature).

---

## 2026-04-27 13:00 -- Brainstorm: initial vision

**Type:** Brainstorm
**Outcome:** vision created
**BCs identified:** Sources, External Sync, Archive Exploration
**Summary:** Snapshot is a personal weekly backup cockpit for Marco, on Windows. It tracks Sources (Docker containers as Driven, Notion / Gmail as Witnessed), shows their staleness, drives the backups it can control and witnesses the rest, mirrors the Drive folder to a VeraCrypt USB Vault on demand with a diff preview, and provides format-specific Archive explorers (Notion render, Gmail mbox) for recovery and trust verification. No version history; each backup overwrites the previous.
**ADRs written:** 0001 (latest copy only), 0002 (witnessed Sources)

---
