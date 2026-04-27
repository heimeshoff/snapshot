---
id: design-system-001-styleguide
title: Design system — tokens, components, patterns, signoff
type: feature
status: todo
bc: design-system
depends_on: [foundation-001-stack-and-form-factor, foundation-005-walking-skeleton]
created: 2026-04-27
---

# Feature: Design system styleguide

## Goal
Build the shared visual and interaction language for Snapshot's WPF UI **before** any feature-level frontend work begins in Sources, External Sync, or Archive Exploration. The styleguide is the gate — no BC implements a UI feature until Marco signs off on the styleguide.

## Foundation
The styleguide is built on **WPF-UI** (Fluent Design, Mica backdrop, Segoe UI Variable, rounded corners) per `foundation-001-stack-and-form-factor`. Most "tokens" come from WPF-UI's theme; the design system's job is to (a) name the WPF-UI bits Snapshot uses, (b) define Snapshot-specific tokens not covered by WPF-UI (state colors, archive-viewer palette), and (c) document app-specific patterns and the components that compose them.

## Scope

### Tokens
- **Color palette**
  - **Inherit from WPF-UI Fluent theme** for chrome, accent, text, surface, border. Document which WPF-UI brushes are used for what.
  - **State colors (Snapshot-specific):** `Fresh`, `Stale`, `Failed`, `Partial`, `NeverRun` — defined as `SolidColorBrush` resources in a `Snapshot.Theme` ResourceDictionary, with light/dark variants matching Mica.
  - **Archive-viewer palette:** an explicitly distinct palette for the chrome around an embedded archive WebView2, so embedded archive content reads as "content, not UI." Defined as a separate ResourceDictionary.
- **Typography** — adopt WPF-UI's Segoe UI Variable scale; document the named text styles (Caption / Body / Subtitle / Title / DisplayLarge or whatever WPF-UI's set is) and which Snapshot patterns use which.
- **Spacing scale** — 4px-based (4, 8, 12, 16, 24, 32, 48), exposed as `Thickness` resources.
- **Radius and elevation** — match WPF-UI's Mica conventions; document the two corner radii and elevation levels used.

### Core components (compositions of WPF-UI primitives + Snapshot conventions)
- **Button** — primary, secondary, danger (using WPF-UI `Button` styles).
- **Badge / pill** — small rounded background with text and optional icon.
- **Source row item** — the `ListBoxItem` / `ItemsControl` template used to render a Source on the dashboard.
- **Tree node (expandable)** — used by the Sync preview tree.
- **Modal / confirm dialog** — using WPF-UI's `ContentDialog`.
- **Form input** — text input and password input (the latter using `PasswordBox` with a reveal toggle button).
- **Empty state**, **Error state**, **Loading state** — three standard placeholder layouts with title + caption + optional action.

### App-specific patterns
- **Staleness pill** — a `Border` + `TextBlock` + icon composition with five states: Fresh / Stale / Failed / Partial / NeverRun. Each state uses a state-color brush + matching iconography (Fluent icons).
- **Sync-diff tree** — collapsible `TreeView` with per-node change counts, three change kinds (add / remove / modify) shown as colored badges, bulk-confirm `Button` at the root, per-branch confirm action on hover/right-click.
- **Archive-viewer chrome** — the WPF page surrounding an embedded WebView2 archive: archive title, "from Source X, backed up at Y" subtitle, close affordance, no sidebar visible inside the viewer area.
- **Source row** — name, kind badge (Driven / Witnessed), last-backup timestamp, staleness pill, primary action (Back up now / Nudge), secondary action (Explore archive).
- **Confirm-before-destructive** — `ContentDialog` pattern for the Vault Sync confirm step; standardized title/body/button layout.

### Documentation
- Live as a **"Design System"** page in the app's left sidebar (alongside Sources / Archives / Sync / Settings), mirroring WhisperHeim's About-page convention of a self-documenting in-app reference.
- Every token, component, and pattern shown with at least one rendered example.
- Each app-specific pattern shown in at least two states side-by-side (e.g., the staleness pill in all five states).
- Each component/pattern accompanied by a copy-paste **XAML** snippet (and any code-behind needed for the WPF-UI configuration).

## Acceptance criteria
- [ ] A "Design System" page exists in the sidebar of the running app and renders all listed tokens, components, and patterns.
- [ ] Each app-specific pattern shows at least two states side-by-side.
- [ ] The page is keyboard-navigable.
- [ ] The page renders correctly in both Light and Dark Fluent themes (per WPF-UI defaults), and at typical window widths (1280, 1600, 1920).
- [ ] State-color brushes and the archive-viewer palette are exposed as `ResourceDictionary` entries that any BC's view can `StaticResource`-bind to — the design system is *consumable*, not just visible.
- [ ] **Signoff gate (hard):** Marco has reviewed the Design System page in the running app and explicitly signed off. The signoff is recorded in this task's closure note before the task moves to `done/`. **No frontend feature task in any BC is allowed to begin work until that signoff is recorded.**
- [ ] Any future feature task that introduces a new UI pattern not in the styleguide must first add it here and re-trigger signoff.

## Notes
- Built on the running walking skeleton — that's why this task `depends_on: foundation-005-walking-skeleton`. The styleguide isn't a paper artifact; it's a live page in the actual WPF app.
- This task being in `contexts/design-system/` (not `contexts/foundation/`) is deliberate: design is its own cross-cutting concern, separate from architecture/persistence/transport.
- WhisperHeim's About page (per `WhisperHeim/design.md`) is a useful structural reference for an in-app self-documenting page.
