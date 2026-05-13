# Index

Top-level catalog of this project's bounded contexts, global decisions, and research.
For BC-scoped artifacts, see each BC's `INDEX.md`.

> Updated by: `model` (BC creation), `work` (global ADRs), `research` (reports tagged global / cross-BC), backfill script.
> Hand-edits are fine but the skills will append at the section markers below.

---

## Bounded contexts

<!-- bc-list:start -->
- **archive-exploration** -- Let Marco read backed-up content directly, without restoring it. Two motives: -- `contexts/archive-exploration/INDEX.md`
- **design-system** -- The shared visual and interaction language for Snapshot's UI: tokens, core components, app-specific patterns, and the review process that keeps frontend coherent across all bounded contexts. -- `contexts/design-system/INDEX.md`
- **external-sync** -- Mirror the Drive folder onto the encrypted USB Vault on demand, with a confirmable preview before any bytes move. -- `contexts/external-sync/INDEX.md`
- **foundation** -- Cross-cutting tech-architecture concerns that don't belong to any single bounded context. Stack choice, persistence, inter-context transport, deployment topology, and cross-cutting policies (logging, secrets, error handling) live here. Also home to the walking-skeleton spike that proves the chosen architecture runs end-to-end. -- `contexts/foundation/INDEX.md`
- **sources** -- Track every backup target, its freshness, and execute the backup itself for the Sources Snapshot controls. -- `contexts/sources/INDEX.md`
<!-- bc-list:end -->

## Global ADRs (scope: global)

<!-- adr-global:start -->
- **0001** -- Latest copy only — no version history of backups -- 2026-04-27 -- `knowledge/decisions/0001-latest-copy-only.md`
- **0002** -- Notion and Gmail are witnessed, not orchestrated -- 2026-04-27 -- `knowledge/decisions/0002-witnessed-sources.md`
- **0003** -- Stack, language, framework, form factor — C# .NET 9 WPF + WPF-UI, summoned foreground app -- 2026-04-27 -- `knowledge/decisions/0003-stack-and-form-factor.md`
- **0004** -- Persistence and local settings layout — SQLite for operational data, settings.json for per-machine preferences -- 2026-04-27 -- `knowledge/decisions/0004-persistence-and-local-settings-layout.md`
- **0005** -- Inter-context transport — shared substrate, no bus -- 2026-04-27 -- `knowledge/decisions/0005-no-inter-context-bus.md`
- **0006** -- Cross-cutting concerns — secrets, logging, partial-failure semantics -- 2026-04-27 -- `knowledge/decisions/0006-cross-cutting-concerns.md`
<!-- adr-global:end -->

## Cross-BC research

Research reports relevant to more than one BC (or to the project as a whole). BC-specific
reports are listed in each BC's `INDEX.md`.

<!-- research-global:start -->
<!-- no cross-BC research yet -->
<!-- research-global:end -->

## Pointers

- Vision: `vision.md`
- Context map: `context-map.md` (if exists)
- Protocol (chronological log): `knowledge/protocol.md` -- newest entries on top
- All ADRs: `knowledge/decisions/`
- All research: `knowledge/research/`
