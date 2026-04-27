---
id: foundation-005-walking-skeleton
title: Walking skeleton — end-to-end thin slice
type: spike
status: todo
bc: foundation
depends_on:
  - foundation-001-stack-and-form-factor
  - foundation-002-persistence-sqlite
  - foundation-003-no-inter-context-bus
  - foundation-004-cross-cutting-concerns
created: 2026-04-27
---

# Spike: Walking skeleton

## Goal
Prove the chosen stack runs end-to-end with all three BCs wired in, persistence reading and writing, and a fake Source round-tripping through register → mark stale → display in the dashboard. **Feature-thin, architecture-thick.** This is the project's first prototype.

## Scope
- C# .NET 9 WPF solution (`Snapshot.sln`) mirroring WhisperHeim's structure:
  - `src/Snapshot/Snapshot.csproj` — main WinExe project, WPF-UI package (no `WPF-UI.Tray`), `Models/`, `Services/`, `Views/`, `Converters/`, `Assets/`.
  - `tests/Snapshot.Tests/` — xUnit project (or whatever WhisperHeim uses, mirror it).
- Three BC-named folders inside the project:
  - `Sources/` — module owning the dashboard view, source registry queries.
  - `ExternalSync/` — module with placeholder Sync view.
  - `ArchiveExploration/` — module with placeholder Archive view.
- `MainWindow.xaml` with WPF-UI Mica backdrop, left sidebar `NavigationView` listing four pages — **Sources** (selected by default), **Archives**, **Sync**, **Settings** — and a content host.
- Settings file created on first run at `%LOCALAPPDATA%\Snapshot\settings.json` (via `System.Text.Json`, atomic write) holding `schemaVersion`, `databasePath`, `window`, and `theme`. Default `databasePath` is `%USERPROFILE%\Documents\Snapshot\snapshot.db`. SQLite database created on first run at the configured path with `sources`, `backup_runs`, `sync_runs` tables (per ADR for `foundation-002`). First-run flow is silent — no picker dialog (only a fallback dialog if the default DB folder cannot be created).
- A single fake Source seeded on first run: `kind = fake`, `mode = driven`, `staleness window = 1 minute`.
- **Sources page** lists the fake Source as a row with name, kind, last-backup, computed Stale/Fresh state, and a "Back up now" button.
- The "Back up now" button writes a `backup_runs` row with `outcome = ok`, updates `last_backup_at`, and touches a placeholder file at a configurable Drive folder path.
- **Archives page** stub: "no archives yet."
- **Sync page** stub: "no Vault mounted."
- **Settings page**: displays the current `databasePath` (read-only TextBox + disabled Browse button is acceptable for this spike; full path-editing UI lands as a follow-up task). Theme selector stub. Window geometry must persist to `settings.json` on close, but no other functional save is required for the spike.

## Acceptance criteria (observable behaviors)
- [ ] Running `Snapshot.exe` from a fresh checkout opens a WPF window with Mica backdrop, sidebar, and content area; the Sources page is selected.
- [ ] No system tray icon appears; the app is a normal foreground window.
- [ ] The dashboard lists the seeded fake Source with computed Stale/Fresh state.
- [ ] Waiting one minute after launch (or seeding the Source with `last_backup_at = null`) shows the Source as **Stale** with a visible badge.
- [ ] Clicking "Back up now" on the fake Source flips it to **Fresh** within 2 seconds, persists a new `backup_runs` row, and a placeholder file appears at the configured Drive folder path.
- [ ] Restarting `Snapshot.exe` shows the same Fresh state — i.e., persistence survives.
- [ ] Navigating to Archives shows the "no archives yet" stub.
- [ ] Navigating to Sync shows the "no Vault mounted" stub.
- [ ] Closing the main window terminates the process cleanly; the SQLite file is not corrupted. No background process remains. No auto-start at Windows login is configured.
- [ ] On first run, `%LOCALAPPDATA%\Snapshot\settings.json` is created with the v1 schema; the SQLite DB is created at the path stored in `databasePath` (default `%USERPROFILE%\Documents\Snapshot\snapshot.db`).
- [ ] Closing the window with a custom position and re-launching shows the window at the same position — `settings.json` round-trips window geometry.
- [ ] The Settings page displays the current `databasePath`.
- [ ] Deleting `settings.json` while the app is closed and re-launching reproduces the first-run flow without errors.
- [ ] Truncating `settings.json` to invalid JSON while the app is closed and re-launching renames it to `settings.json.broken-<utc>` and proceeds with defaults.

## Explicit non-goals for the spike
- No real SSH, no real VeraCrypt, no Notion zip parsing, no Gmail mbox parsing, no WebView2 yet.
- No system tray, no hotkeys, no auto-start at login.
- No Fluent styling beyond what comes for free with WPF-UI defaults. Real design language lands when `design-system-001-styleguide` runs.
- No real Drive-folder integration beyond touching a configurable file path.
- No editable DB path on the Settings page — the path is shown but not editable in the spike. Path-editing UI lands in a follow-up task.
- No "move database file" feature when the path changes.
- No active Drive-folder detection or warning. Static informational text on Settings is enough for v1.

## Notes
- Mirror WhisperHeim's `MainWindow.xaml` and `App.xaml` patterns where the shell shape applies; skip its `WPF-UI.Tray` setup and its "Launch at startup" Settings toggle — Snapshot has neither.
- This task is the gate between "decisions on paper" and "code that compiles." Once it lands, the design-system styleguide can be built on top of the running app, and feature tasks across BCs can begin once the styleguide signoff is recorded.
- BC-local decision tasks (`sources-001`, `external-sync-001`, `archive-exploration-001`) are *not* dependencies of this spike — the spike stubs each BC. Those decisions block real BC implementation, not the skeleton.
