# Vision: Snapshot

## Purpose
Snapshot is a personal weekly backup cockpit for one user (Marco) on Windows. It gives a single place to see what's backed up, how stale each backup is, push backups for the data sources it can control, monitor the ones it can't, and propagate everything to an encrypted external drive. It also lets Marco read his backed-up Notion and Gmail directly — both for recovery and to verify the backups are real.

## Users
A single user — Marco — running Snapshot on his Windows machine. No other actors. No multi-tenancy. No remote access.

## The problem
Marco's data lives in three places:

- **Google Drive**, synced locally on the Snapshot host.
- **A Linux Docker host** on his home network, running multiple containers whose volumes need backing up.
- **Cloud services he can't fully automate** — Notion (no export API) and Gmail (Google Takeout has no on-demand trigger).

Today there is no single view of "is everything backed up and recent?" — staleness is felt as guilt, not seen as data. Pushing backups to the VeraCrypt-encrypted USB drive happens by hand, with no preview of what's about to change. And once data is backed up, there is no easy way to actually read what's in it without restoring — which makes it impossible to *trust* the backups.

## What success looks like
- **Sunday morning:** open Snapshot, see at a glance which Sources are stale, click "back up stale sources". Containers get pushed into the Drive folder. Dashboard goes green.
- **Witnessed sources** (Notion, Gmail) show as stale when their file timestamps in the Drive folder are old → Snapshot nudges Marco to run those exports manually elsewhere.
- **External sync:** when Marco plugs in the USB drive, Snapshot prompts for the VeraCrypt password, shows a collapsible tree of what would change / add / delete, he confirms, the copy runs.
- **Trust & recovery:** when Marco loses access to Notion or wonders whether a backup is real, he opens Snapshot's archive explorer and reads the content directly — Notion pages render like Notion, Gmail mbox is searchable and readable.

## Non-goals
- **No version history.** Each backup overwrites the previous file in the Drive folder. Historical view is an operations log, not a tree of past data. No "restore Tuesday's version." (See ADR-0001.)
- **No automated triggering of Notion or Gmail exports.** External APIs make this impossible today; Snapshot witnesses those Sources, not orchestrates them. No browser automation or scraping to fake it. (See ADR-0002.)
- **Not a continuous-sync daemon.** Summoned weekly, not running in the background.
- **Not multi-user, not commercial, not a product.** Personal tool, one machine, one user.
- **Not a general cloud-backup tool.** Scoped to Marco's specific topology (Google Drive + Linux Docker host + VeraCrypt USB).
- **Not a Notion or Gmail editor.** Archive explorers are read-only.

## Ubiquitous language (seed)
- **Source** — anything Snapshot tracks for backup purposes. Each individual Docker container is its own Source. Notion, Gmail, and other Google Drive subjects can each be Sources too.
- **Driven Source** — a Source where Snapshot performs the backup itself (e.g., a Docker container).
- **Witnessed Source** — a Source where Snapshot only reads file timestamps to determine freshness; the backup is produced elsewhere (e.g., Notion, Gmail).
- **Staleness window** — per-Source maximum age before its backup counts as stale and surfaces in the dashboard.
- **Backup run** — a single execution of pushing a Driven Source's data into the Drive folder.
- **The Drive folder** — the local Google Drive folder on the Snapshot host. The central junction: data flows into it from Driven Sources, lands in it from Witnessed Sources, and flows out of it to the Vault.
- **Vault** — the VeraCrypt-encrypted external USB drive. Mounted on demand.
- **Sync preview** — collapsible diff tree shown before any bytes move to the Vault.
- **Archive** — a Source's backed-up artifact in the Drive folder, viewable through a format-specific explorer (Notion zip, Gmail mbox).

## Open questions
- What is the exact mechanism for backing up a Docker container's data? (Stop → copy volume → restart? Per-container hooks? A volume-aware tool?) Likely needs a small spike per container family.
- How does Snapshot authenticate to the Linux host? SSH per backup run, presumably — auth model still open.
- Form factor — desktop app, local web app, CLI? Deferred to the model phase.
- Diff strategy on Windows for the Vault sync — robocopy /MIR with timestamps, or content-hash deep comparison? Worth a small research task before committing.
- Are there other Archive viewers worth building beyond Notion and Gmail (e.g., for container backups)? Not for v1.
