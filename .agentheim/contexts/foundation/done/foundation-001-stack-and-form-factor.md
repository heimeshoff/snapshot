---
id: foundation-001-stack-and-form-factor
title: Stack, language, framework, form factor
type: decision
status: done
bc: foundation
scope: global
depends_on: []
created: 2026-04-27
completed: 2026-04-27
commit: 390584a
---

# Decision: Stack / language / framework / form factor

## Recommendation
**C# .NET 9 (`net9.0-windows`) WPF application using WPF-UI (Fluent Design with Mica backdrop), WinExe target, single-file self-contained publish. Single window with left sidebar navigation + content area. No system tray, no auto-start at Windows login — Snapshot is summoned from the Start menu / Explorer when needed and exits when the window is closed. Mirrors the WhisperHeim project structure (`Models/`, `Services/`, `Views/`, `Converters/`, `Assets/`).**

## Why
- This is Marco's actual personal-Windows-tool stack, established and proven by WhisperHeim. Same idioms, same NuGet packages (with the exception of `WPF-UI.Tray`, which Snapshot does not need), same conventions — zero learning tax.
- Native WPF gives full control over the dashboard, sync-diff tree, and master-detail Gmail viewer without iframe ceremony or browser-tab UX.
- WPF-UI provides Fluent design tokens (Mica, Segoe UI Variable, accent colors, rounded corners) and a base component set out of the box — the design system gets a strong foundation rather than building from scratch.
- Snapshot is summoned weekly, not background-resident. A normal foreground app — open from Start menu, do the work, close — fits the rhythm; tray-and-autostart would be ceremony for a tool the user only wants when he wants it.
- For embedding rendered HTML (the Notion archive viewer), WPF integrates with **WebView2** as the standard mechanism — far more native than running a localhost HTTP server.

## Alternatives considered
- **F# / .NET (any UI framework)** — F# is in Marco's portfolio (e.g., F# Full Stack Blueprint) but it is *not* his stack for personal Windows desktop tools; WhisperHeim is the canonical reference and it's C# WPF. Rejected: corrects an earlier wrong assumption.
- **ASP.NET Core localhost web app + HTMX (originally proposed)** — clever, but contradicts Marco's established Windows desktop convention and feels foreign on Win11. Rejected.
- **Avalonia / WinUI 3** — cross-platform / newer, but Snapshot is Windows-only and WhisperHeim's WPF + WPF-UI combination is the proven path. No win.
- **Electron / Tauri** — abandons .NET strength, adds a runtime, no benefit.
- **Pure CLI** — fails the dashboard-at-a-glance, sync-preview-tree, and archive-viewer requirements.
- **Tray-resident background app (mirroring WhisperHeim's tray-and-hotkey model)** — explicitly rejected. Snapshot is a weekly-summoned foreground tool, not an always-on hotkey-triggered service.

## Acceptance criteria
- [x] ADR committed at `.agenthoff/knowledge/decisions/0003-stack-and-form-factor.md` with `scope: global`.
- [x] ADR justification matches the draft below (or Marco's amended version).
- [x] No code change required by this task.

## Notes (architect's ADR draft, corrected)

```markdown
## Context
Snapshot is a single-user Windows tool requiring a dashboard, a diff-tree
preview, and rich archive viewers (Notion HTML, Gmail mbox). The user has
an established personal-tool stack (WhisperHeim:
`C:\src\heimeshoff\tooling\WhisperHeim`) — C# .NET 9 + WPF + WPF-UI with
Fluent / Mica design, sidebar-nav single-window app. WhisperHeim is also
tray-resident with hotkey triggers and an auto-start option, but Snapshot
explicitly is not — it is a weekly-summoned foreground app.

An earlier draft of this ADR proposed F# + ASP.NET Core + HTMX, which was
explicitly rejected because it does not match the user's Windows desktop
conventions.

## Decision
Implement Snapshot as a C# .NET 9 WPF application targeting `net9.0-windows`,
with `WinExe` output and `<UseWPF>true</UseWPF>`. Use the `WPF-UI` NuGet
package for Fluent design (Mica backdrop, Segoe UI Variable, rounded
corners). Follow the WhisperHeim project structure: `Models/`, `Services/`,
`Views/`, `Converters/`, `Assets/`. The app is a single window with left
sidebar navigation and a content area. Publish as a single-file
self-contained executable.

The app does **not** run in the system tray and does **not** launch at
Windows startup. It is summoned by the user when needed and exits cleanly
when the main window is closed. `WPF-UI.Tray` is therefore not added as a
dependency.

## Consequences
- Reuses Marco's established conventions; the WhisperHeim repo is a working
  reference for project structure, NuGet packaging, publish flags, and UI
  patterns — minus the tray / auto-start pieces, which do not apply here.
- Native Win11 feel — Mica, Fluent. Matches "first-party Windows app"
  aesthetic Marco values.
- Embedded HTML rendering (Notion archive viewer) uses WebView2, not an
  iframe-into-localhost scheme. See ADR for `archive-exploration-001`.
- No browser, no HTTP server, no localhost binding, no CORS/CSP concerns at
  the *app shell* level (CSP-equivalent restrictions for the WebView2
  archive viewers are a per-BC concern).
- Closing the main window terminates the process cleanly. No background
  presence, no tray icon, no auto-launch at login.
```

## Outcome
ADR 0003 written at `.agenthoff/knowledge/decisions/0003-stack-and-form-factor.md`. Stack is fixed: C# .NET 9 + WPF + WPF-UI, single-window summoned foreground app, no tray, no auto-start. Mirrors WhisperHeim project layout. No code changed by this task — downstream tasks (notably `foundation-005-walking-skeleton`) will scaffold the actual project.
