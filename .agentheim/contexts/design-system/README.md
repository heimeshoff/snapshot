# Design System

## Purpose
The shared visual and interaction language for Snapshot's UI: tokens, core components, app-specific patterns, and the review process that keeps frontend coherent across all bounded contexts.

## Classification
Cross-cutting (not a domain BC — a shared substrate for every BC that renders UI).

## Scope
- **Tokens** — color palette (state colors for fresh / stale / failed / partial; chrome neutrals; archive-viewer palette deliberately distinct from app chrome), typography scale, spacing scale, radius/elevation.
- **Core components** — button, badge/pill, table row, expandable tree node, modal, form input, password input, empty/error/loading states.
- **App-specific patterns** — staleness pill (5 states), sync-diff tree (collapsible, per-branch confirm), archive-viewer chrome, Source row, confirm-before-destructive.
- **Documentation** — every token, component, and pattern shown live with code snippets at `/styleguide` in the running app.

## The styleguide gate
This BC owns a hard process gate: **no frontend feature task in any BC begins work until the styleguide is reviewed and signed off by Marco.** New UI patterns introduced by feature tasks must be added to the styleguide first and re-trigger signoff before the feature task can proceed.

Every frontend-bearing task in any BC must `depends_on: design-system-001-styleguide`.

## Ubiquitous language
- **Token** — a named, themable design value (a color, a spacing step, a font size).
- **Component** — a reusable UI primitive with defined states and interaction semantics.
- **Pattern** — a domain-specific composition of components solving a recurring Snapshot need.
- **Signoff** — Marco's recorded approval of the styleguide as it stands; required before any feature-level frontend work begins.
