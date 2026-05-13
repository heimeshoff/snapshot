---
id: archive-exploration-001-archive-viewers
title: Archive viewers — Notion zip rendering and Gmail mbox browser
type: decision
status: done
bc: archive-exploration
scope: archive-exploration
depends_on: [foundation-001-stack-and-form-factor, foundation-002-persistence-sqlite]
created: 2026-04-27
completed: 2026-04-27
---

# Decision: Archive viewer rendering strategy

## Recommendation
- **Notion zip:** unzip lazily into a per-archive cache directory under `%LOCALAPPDATA%\Snapshot\archives\<archiveId>\`. Render the entry-point HTML inside a **WebView2** control (`Microsoft.Web.WebView2.Wpf`) embedded in a WPF Archive Viewer page. Restrict navigation to local files within the archive root via the `NavigationStarting` event; disable JavaScript via `CoreWebView2Settings.IsScriptEnabled = false`; block external network access via `CoreWebView2.WebResourceRequested` for non-local schemes.
- **Gmail mbox:** stream-parse with **MimeKit** (the .NET MIME library); build a per-archive **SQLite FTS5** sidecar index at `%LOCALAPPDATA%\Snapshot\archives\<archiveId>\index.db`. Render in a WPF master-detail layout (master: search box + result list with date/sender/subject; detail: email body). Plain-text bodies render in a styled `TextBlock`/`RichTextBox`; HTML bodies render in a second WebView2 instance with the same security restrictions as the Notion viewer.

## Why
- WebView2 is the standard way to embed web content in WPF apps (a managed Edge Chromium component) and is what Microsoft recommends for mixing native shell with HTML rendering. Notion's HTML export renders with near-native fidelity for free.
- Disabling JS + blocking external network requests on WebView2 is the same security posture as a sandboxed iframe + CSP, expressed through the WebView2 API instead of headers.
- MimeKit is mature and handles MIME edge cases (multipart, encoded headers, attachments) correctly. FTS5 gives Gmail-like search without bringing in a separate index.
- Master-detail in WPF is idiomatic (Marco uses it on WhisperHeim's Templates and Transcripts pages); the Gmail viewer should feel familiar.

## Alternatives considered
- **CefSharp (embedded Chromium)** — heavier, less native than WebView2. WebView2 is the better Microsoft-recommended path on modern Windows.
- **Custom WPF rendering of Notion HTML** — vast amount of work, would never reach Notion's fidelity. Rejected.
- **ASP.NET serving extracted static files into an iframe** — only relevant if Snapshot were a localhost web app (it isn't, per `foundation-001`). Rejected.
- **Parse mbox into the main SQLite DB** — pollutes the registry DB with arbitrary email content; per-archive sidecar databases keep concerns separate.
- **Render HTML emails as text only** — loses fidelity for the trust-verification use case (Marco wants to *see* what's there).

## Acceptance criteria
- [x] ADR committed at `.agentheim/knowledge/decisions/0009-archive-viewers.md` with `scope: archive-exploration`.
- [x] ADR justification matches the draft below (or Marco's amended version).
- [x] No code change required by this task.

## Outcome
ADR-0009 written at `.agentheim/knowledge/decisions/0009-archive-viewers.md` capturing the decision: WebView2 (JS disabled, navigation locked, external requests blocked) for Notion zip rendering; MimeKit + per-archive SQLite FTS5 sidecar with WPF master-detail UI for Gmail mbox browsing. No code changes — pure decision task. The BC README already cited this ADR (line 27); content is consistent, no README update needed.

## Notes (architect's ADR draft, corrected)

```markdown
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
```
