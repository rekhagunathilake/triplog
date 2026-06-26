# ADR-003: Entry as separate aggregate root

## Status
Accepted — 2026-06-20

## Context
A Trip contains many Entries. Domain-Driven Design offers two natural modelings of this containment:

1. **Trip as the aggregate root, Entry as a child entity** — all Entry mutations flow through Trip; only `ITripRepository` exists
2. **Trip and Entry as separate aggregate roots** — Entry holds a `TripId` reference but is loaded/saved independently; both have their own repositories

A travel journal typically grows to dozens of entries per trip. Read patterns favour entry-level access (timeline, per-day, feed). The
publish saga (ADR-005) targets a single Entry, not the whole Trip.

## Decision
Trip and Entry are **separate aggregate roots**. Entry holds a `TripId` reference; it is never navigated as a Trip child.

The cross-aggregate creation rule ("Entry must reference a valid, non-archived Trip") is enforced at the **Application layer**, not
inside the Domain. An application command handler loads the Trip, verifies its existence and status, then calls `Entry.Create`.

## Consequences
**Positive:**
- Loading a Trip is constant-time regardless of entry count
- Entry mutations don't require loading the full Trip aggregate
- The publish saga can operate on a single Entry in isolation
- Each aggregate has a focused, small surface

**Negative:**
- Cross-aggregate invariants require explicit application-layer code
- Two repositories instead of one
- Eventual consistency required if a Trip is archived while Entries are being created (rare in practice; acceptable trade-off)

## Alternatives considered
- **Trip-as-aggregate** (Entry as child entity) — rejected because:
  - Loading a Trip would pull every entry, degrading as entries grow
  - The publish saga would need to mutate a child entity through the Trip aggregate root, awkward for state-machine clarity
  - Modifying any entry would require write-locking the whole trip

## Reference
Follows Vaughn Vernon's "small aggregates" guidance from *Implementing Domain-Driven Design*.