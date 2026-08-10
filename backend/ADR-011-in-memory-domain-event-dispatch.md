# ADR-011: In-memory domain event dispatch (transactional outbox deferred)

## Status
Accepted — 2026-07-30

## Context
Domain events raised by aggregates (e.g., `EntryPublishBeganDomainEvent`, `MediaItemFinalizedDomainEvent`) must reach subscribers — some in-process
(MediatR notification handlers running side effects), some cross-service (integration handlers translating to MassTransit messages published to
RabbitMQ).

Two mainstream dispatch strategies:

1. **Immediate in-memory** — a `SaveChangesInterceptor` collects tracked aggregates' events after the DB commit and calls `IPublisher.Publish` for
   each
2. **Transactional outbox** — write events to an `outbox` table in the same transaction as the aggregate; a background dispatcher polls the table and
   publishes with at-least-once delivery guarantees

The outbox pattern is more robust but adds infrastructure: outbox table, polling worker, message dispatcher, and consumer-side deduplication.

## Decision
Use **in-memory dispatch** via a `SaveChangesInterceptor` overriding `SavedChangesAsync`. The interceptor iterates aggregates via a non-generic
`IAggregateRoot` marker interface, publishes each event via MediatR `IPublisher`, and clears the aggregate's event list.

The failure window is understood and accepted for v1: if the process crashes after `SaveChangesAsync` returns but before the interceptor completes
publishing, some subscribers miss the event.

## Consequences
**Positive:**
- Minimal infrastructure — one interceptor class + one marker interface
- No polling worker, no outbox table, no reconciliation job
- Straightforward mental model — after commit, events dispatch
- Fast — no additional DB write per event

**Negative:**
- **At-most-once integration event delivery** in the failure window between DB commit and event publish
- Not production-safe as-is for anything requiring guaranteed delivery
- The saga state and Entry state save atomically in one transaction, but *integration events* published to MassTransit aren't part of that atomicity

**Mitigations:**
- Saga-relevant events flow within the entries-api process (interceptor → integration handler → MassTransit publish all in-process), narrowing the
  failure window to microseconds
- The saga's idempotency design ([ADR-012](ADR-012-saga-design.md)) means duplicate events don't corrupt state on retry, so at-least-once behavior
  from the outbox would be safe to adopt later without breaking existing consumers

## v2 plan
Adopt MassTransit's `AddEntityFrameworkOutbox<TriplogEntriesDbContext>()`:
- `OutboxState`, `OutboxMessage`, `InboxState` tables (via a migration)
- Automatic outbox write in the same transaction as saga state
- Background dispatcher publishes outbox rows to RabbitMQ
- Consumer-side deduplication via inbox

Estimated effort: half a day. Documented as a v2 hardening item alongside real auth (ADR-006) and cloud deploy.

## Alternatives considered
- **Immediate in-transaction dispatch** (fire during `SavingChangesAsync`) — rejected because events would fire even if the transaction rolled back
- **Two-phase commit across DB and message broker** — rejected as generally unavailable and complex to configure
- **Skip integration events, use polling from downstream services** — rejected as forcing tight coupling on every consumer