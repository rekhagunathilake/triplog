# ADR-005: Saga orchestration in entries-api

## Status
Accepted — 2026-06-21

## Context
The publish-entry workflow involves both services:
1. entries-api transitions Entry → Publishing
2. media-api finalises associated media (thumbnails, EXIF, immutability marking)
3. entries-api transitions Entry → Published (or back to Draft on failure)

This is a multi-step coordinated workflow with explicit compensation on failure — a classic saga. Two patterns apply:
- **Orchestration** — a single component (the saga state machine) drives the workflow, sending commands and reacting to results
- **Choreography** — each service publishes events; other services react autonomously; no single owner of the workflow state

## Decision
**Orchestration**, with the `PublishEntrySaga` state machine living in `entries-api`. The saga is implemented using MassTransit's saga state
machine pattern.

entries-api is the orchestrator because Entry owns the publish state. media-api remains stateless about the saga — it receives
`FinalizeMediaCommand`, processes it, and publishes `MediaFinalized` or `MediaFinalizationFailed`. It has no knowledge of what the saga does next.

## Consequences
**Positive:**
- Single source of truth for the publish workflow state
- Compensation logic is in one place, easy to reason about
- Failure-recovery paths are explicit in the state machine
- Easy to extend (additional steps land in the saga, not scattered across services)

**Negative:**
- entries-api carries more responsibility — must run the saga, manage its state, and react to media-api's events
- Tighter coupling: entries-api needs to know media-api exists and what commands it accepts (but message contracts are versioned in `Triplog.Contracts`)

## Alternatives considered
- **Choreography** — rejected because the workflow state would be scattered across both services. Failure recovery would require inferring intent from event history. Debugging would be painful.
- **Saga in media-api** — rejected because media-api is the worker, not the workflow owner. The publish state belongs to Entry.
- **Saga in a third "orchestrator" service** — rejected as over-engineering for two participants.

## Edge case to know
An Entry can be archived while a saga is in flight. The saga may attempt `CompletePublish` or `FailPublish` on an archived Entry, which the aggregate will reject (`EntryErrors.IsArchived`). The
saga handler must treat this as a benign terminal state.

## Illustration

```mermaid
sequenceDiagram
    participant U as User (Next.js)
    participant M as media-api
    participant E as entries-api
    participant R as RabbitMQ
    
    U->>M: POST /media (upload photo)
    M->>M: Store blob in MinIO (provisional)
    M-->>U: 201 { mediaReferenceId }
    U->>E: POST /trips/{id}/entries (with media refs)
    E->>E: Entry.Create() → Draft
    E-->>U: 201 { entryId }
    U->>E: POST /entries/{id}/publish
    E->>E: Entry.BeginPublish() → Publishing
    E->>R: Publish EntryPublishBegan
    R->>R: Saga starts; sends FinalizeMediaCommand
    R->>M: FinalizeMediaCommand
    M->>M: Generate thumbnails, extract EXIF
    M->>R: Publish MediaFinalized
    R->>E: MediaFinalized → saga continues
    E->>E: Entry.CompletePublish() → Published
    E->>R: Publish EntryPublished
```