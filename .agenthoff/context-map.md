# Context map

## Contexts

### Sources
- **Purpose:** Track every backup target, its freshness, and — where Snapshot has authority — execute the backup itself. The dashboard lives here.
- **Core language:** Source, Driven / Witnessed, Staleness window, Backup run, The Drive folder, Last backup, Stale / Fresh.
- **Classification:** core
- **Key actors:** Marco (opens dashboard, clicks "back up stale sources"); the Linux Docker host (target for backup runs).

### External Sync
- **Purpose:** Mirror the Drive folder to the VeraCrypt Vault on demand, with a confirmable preview before any bytes move.
- **Core language:** Vault, Mount, Decrypt, Sync preview, Change (added / removed / modified), Sync run.
- **Classification:** supporting
- **Key actors:** Marco (supplies VeraCrypt password, confirms diff); the Vault (a removable VeraCrypt volume).

### Archive Exploration
- **Purpose:** Make backed-up content directly readable without restore — for recovery (lost live access) and for trust verification (does this backup actually contain what I think it does?).
- **Core language:** Archive, Notion archive, Gmail archive, Page, Email, Search, Render.
- **Classification:** supporting
- **Key actors:** Marco (browses, searches, reads).

## Relationships

- **Sources → External Sync** (customer-supplier; Sources upstream). External Sync only needs to know "what's in the Drive folder right now" — it has no concept of staleness or backup runs. Sources is the supplier of the substrate; External Sync is the customer that mirrors it outward.
- **Sources → Archive Exploration** (customer-supplier; Sources upstream). Archive Exploration is invoked from a Source row ("explore this archive") and just needs the path to the artifact. It does not care about staleness or backup runs.
- **External Sync ↔ Archive Exploration** (separate ways). They share the Drive folder as substrate but never talk to each other.

## Why this isn't a single context
Three different motions, three different moods, three different vocabularies:

- **Sources** speaks of *Sources, freshness, runs* — operational and recurring.
- **External Sync** speaks of *Vaults, mounts, diffs* — opportunistic and confirm-first.
- **Archive Exploration** speaks of *pages, emails, search* — read-only and human-paced.

Trying to share a model across all three would force unhelpful coupling (e.g., a "Source" would need to know about Notion-page rendering, or the Vault diff would have to understand staleness). They share data, not concepts.
