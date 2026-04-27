# Foundation

## Purpose
Cross-cutting tech-architecture concerns that don't belong to any single bounded context. Stack choice, persistence, inter-context transport, deployment topology, and cross-cutting policies (logging, secrets, error handling) live here. Also home to the walking-skeleton spike that proves the chosen architecture runs end-to-end.

## Classification
Cross-cutting (not a domain BC — a place for architecture decisions whose ADRs are scoped `global`).

## Notes
- Decision tasks here produce ADRs in `.agenthoff/knowledge/decisions/` with `scope: global`.
- The walking-skeleton spike is the project's first working code. Every domain BC depends (transitively) on it.
- This directory exists *because* of the brainstorm architecture-foundation pass; future cross-cutting refinements land here too.
