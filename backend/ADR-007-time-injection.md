# ADR-007: Time injection in the Domain layer

## Status
Accepted — 2026-06-21

## Context
Domain aggregates need timestamps — `CreatedAtUtc`, `ArchivedAtUtc`, `OccurredOnUtc` on domain events. The naïve implementation reads
`DateTime.UtcNow` directly inside aggregate methods.

This makes the Domain non-deterministic and untestable:
- Two test runs of the same code produce different timestamps
- A test that asserts "Entry was created at 12:00" must use loose comparisons (within N milliseconds) and still flakes occasionally
- The Domain is no longer a pure function of its inputs

## Decision
**All time-aware methods in the Domain accept `DateTime nowUtc` as a parameter.** The Domain never reads `DateTime.UtcNow` itself. The
Application layer is responsible for providing the time, typically via .NET 8+ `TimeProvider`.

Domain events carry `OccurredOnUtc` as a **positional record parameter** — the event constructor requires a timestamp, ensuring
every call site provides one explicitly. There is no `{ get; } = DateTime.UtcNow` default.

## Consequences
**Positive:**
- Domain tests are fully deterministic — `FixedNowUtc` constants produce exact-match assertions
- Time can be faked in tests via `FakeTimeProvider` (the .NET 8+ testing library)
- The "where does time come from" question is explicit at every call site
- Saga tests can fast-forward time without mocking the clock

**Negative:**
- Every command handler in the Application layer must thread `TimeProvider` through to the Domain call
- Slight verbosity at call sites (extra parameter)
- A developer might pass the wrong timestamp accidentally (e.g. passing `DateTime.Now` instead of `DateTime.UtcNow`) — mitigated by always using `TimeProvider.GetUtcNow()`

## Application-layer wiring
Command handlers inject `TimeProvider`:

```csharp
public sealed class CreateTripCommandHandler(
    ITripRepository repository,
    TimeProvider timeProvider)
    : IRequestHandler<CreateTripCommand, Result<TripId>>
{
    public async Task<Result<TripId>> Handle(CreateTripCommand command, CancellationToken ct)
    {
        var trip = Trip.Create(..., timeProvider.GetUtcNow().UtcDateTime);
        // ...
    }
}