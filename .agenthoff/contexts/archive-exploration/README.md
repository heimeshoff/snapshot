# Archive Exploration

## Purpose
Let Marco read backed-up content directly, without restoring it. Two motives:
1. **Recovery** — he lost access to the live Source.
2. **Trust verification** — he wants to confirm the backup is real and complete.

## Classification
Supporting

## Ubiquitous language
- **Archive** — a backed-up artifact stored in the Drive folder.
- **Notion archive** — the zip produced by Notion's bimonthly export.
- **Gmail archive** — the mbox file from Google Takeout.
- **Render** — display Notion content in something close to its native layout.
- **Search / Filter / Read** — Gmail-style browsing over the mbox.

## Notes
- Read-only. No editing, no syncing changes back upstream.
- Invoked from a Source row in the Sources dashboard.
- Format-specific viewers; for v1 the two formats are Notion zip and Gmail mbox.
- This is the *trust* mechanism for the whole backup chain — without it, Marco can't prove a backup is real.

## Frontend gate
This BC has UI (the Notion archive viewer chrome, the Gmail mbox browser with search/filter/read). **Every frontend task captured for this BC must `depends_on: design-system-001-styleguide`.** No feature task that introduces or alters UI in this BC may begin work until the styleguide signoff is recorded.

The Notion viewer also crosses a security boundary (rendering exported HTML inside a WebView2 with JavaScript disabled, navigation locked to the archive root, and external network requests blocked, per `archive-exploration-001-archive-viewers`). Frontend tasks must respect that boundary.
