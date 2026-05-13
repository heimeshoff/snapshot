---
id: 0003
title: Stack, language, framework, form factor — C# .NET 9 WPF + WPF-UI, summoned foreground app
status: accepted
date: 2026-04-27
scope: global
---

# 0003 — Stack and form factor

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
