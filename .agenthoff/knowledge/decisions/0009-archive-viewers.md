---
id: 0009
title: Archive viewers — Notion zip rendering and Gmail mbox browser
status: accepted
date: 2026-04-27
scope: archive-exploration
---

# 0009 — Archive viewers: WebView2 for Notion, MimeKit + FTS5 for Gmail

## Context
Archive Exploration must render two formats: Notion's zip export
(HTML/CSS + assets) and Gmail's mbox (RFC 5322 message stream). The
viewer is the trust mechanism for the entire backup chain; fidelity
matters. The host is a C# WPF application (per `foundation-001`).

## Decision
**Notion.** On first open, extract the zip into
`%LOCALAPPDATA%\Snapshot\archives\<archiveId>\`. Render the entry-point
HTML in a `Microsoft.Web.WebView2.Wpf.WebView2` control embedded in the
Archive Viewer page. Security restrictions on the WebView2 instance:
- `CoreWebView2Settings.IsScriptEnabled = false`
- `CoreWebView2Settings.AreDevToolsEnabled = false`
- Subscribe to `NavigationStarting` and cancel any navigation outside
  the archive root or to non-`file://` schemes
- Subscribe to `WebResourceRequested` and block all external URL schemes
- Use a dedicated user-data folder per archive to isolate cookies/
  storage from any other WebView2 usage

**Gmail.** Stream-parse the mbox with MimeKit. Build a per-archive
SQLite FTS5 sidecar at
`%LOCALAPPDATA%\Snapshot\archives\<archiveId>\index.db` indexing
date, sender, recipients, subject, and body text. Render the viewer
as a WPF master-detail page:
- Master: search box (full-text via FTS5) + virtualized list of
  matching messages.
- Detail: headers + body. Plain-text bodies render in WPF text
  controls; HTML bodies render in a WebView2 instance with the same
  security restrictions as the Notion viewer.

Both viewers are read-only.

## Alternatives considered
- **CefSharp (embedded Chromium)** — heavier, less native than WebView2.
  WebView2 is the better Microsoft-recommended path on modern Windows.
- **Custom WPF rendering of Notion HTML** — vast amount of work, would
  never reach Notion's fidelity. Rejected.
- **ASP.NET serving extracted static files into an iframe** — only
  relevant if Snapshot were a localhost web app (it isn't, per
  `foundation-001`). Rejected.
- **Parse mbox into the main SQLite DB** — pollutes the registry DB
  with arbitrary email content; per-archive sidecar databases keep
  concerns separate.
- **Render HTML emails as text only** — loses fidelity for the
  trust-verification use case (Marco wants to *see* what's there).

## Consequences
- Notion fidelity is "whatever Notion exports" — high, with no
  rendering work on our side.
- Disk footprint grows by ~2x per Notion archive (zip + extracted).
  Cache is invalidatable: deleting
  `%LOCALAPPDATA%\Snapshot\archives\<id>\` forces re-extraction.
- mbox indexing is a one-time per-archive cost; subsequent reads are
  fast via FTS5.
- WebView2 requires the WebView2 Runtime on the host; modern Windows
  11 ships with it preinstalled, but the installer must check.
- Disabling JavaScript and blocking external requests is a defensive
  default. Notion's exports today are static HTML/CSS that renders
  fine without JS; if a future Notion export depends on JS, this
  will degrade and we revisit.
