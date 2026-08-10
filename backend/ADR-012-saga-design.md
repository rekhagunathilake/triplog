# ADR-012: Saga design — retention, idempotency, retry

## Status
Accepted — 2026-07-30

## Context
The `PublishEntrySaga` state machine coordinates the publish workflow between entries-api (orchestrator) and media-api. Three design questions surfaced
during implementation:

1. **When should completed saga rows be deleted?**
2. **How should the saga handle duplicate events?**
3. **How should transient database failures be retried?**

Each was addressed separately in code; this ADR captures the reasoning for all three because they're interrelated.

## Decision

### Retention — saga rows persist in terminal states
`SetCompletedWhenFinalized()` is called but custom terminal states (`Completed`, `Failed`) are **not** finalized via `.Finalize()`. Saga rows
stay in `publish_entry_saga_state` after completion as an audit trail — recording when each publish attempt started, its outcome, and (on failure)
the reason.

### Idempotency — explicit `Ignore` on duplicate events
Every non-Initial state has an explicit handler (or `Ignore`) for every event it might receive:

```csharp
During(Publishing,
    Ignore(EntryPublishBegan),       // duplicate — already publishing
    When(MediaFinalized)  // ...
    When(MediaFinalizationFailed) // ...
);

During(Completed, Failed,
    Ignore(EntryPublishBegan),
    Ignore(MediaFinalized),
    Ignore(MediaFinalizationFailed));

During(Failed,
    When(EntryPublishBegan)          // legitimate retry — reset and re-run
        .Then(ResetForRetry)
        .ThenAsync(SendFinalizeCommandsAsync)
        .TransitionTo(Publishing));
```

### Retry — MassTransit message-level retry for transient errors
Bus configuration includes:

cfg.UseMessageRetry(r =>
    r.Exponential(
        retryLimit: 5,
        minInterval: TimeSpan.FromMilliseconds(100),
        maxInterval: TimeSpan.FromSeconds(2),
        intervalDelta: TimeSpan.FromMilliseconds(200)));
Transient failures (Postgres 40001 serialization conflicts on the saga row, RabbitMQ connection blips, DB deadlocks) are retried automatically. Permanent
failures move to the _error queue after retries exhausted.

## Consequences
Positive:

- Audit trail available for debugging and portfolio demos — every saga run visible in publish_entry_saga_state
- Sagas survive duplicate delivery from MassTransit retries, RabbitMQ redelivery on restart, and accidental double-clicks
- Transient DB conflicts self-heal without operator intervention
- All three patterns demonstrated together are portfolio-worthy signal of saga-design fluency

Negative:

- publish_entry_saga_state grows unbounded — production would need a periodic cleanup job (e.g., delete rows in terminal states older than 30
days)
- Retries can mask underlying issues if treated as a band-aid; team must monitor _error queues to catch permanent failures
- Explicit Ignore per state × event costs code size — traded for clarity over automatic-swallow behavior

## Alternatives considered
- Delete sagas on completion (default MassTransit behavior) — loses audit trail; rejected for portfolio scope where the row IS the evidence
- Optimistic concurrency instead of pessimistic — swaps 40001 conflicts for DbUpdateConcurrencyException; the same retry pattern handles either. Pessimistic chosen because MassTransit's UsePostgres()
helper defaults to it and it's more predictable under load 
- Fail loudly on unhandled events instead of Ignore — chosen against because retries and redelivery make duplicate arrival routine; loud failure fills the _error queue with noise from normal operation
- EF Core EnableRetryOnFailure instead of MassTransit retry — rejected because EF Core's per-command retry can't correctly re-execute
the saga's transaction boundary; MassTransit's message-level retry is the right layer