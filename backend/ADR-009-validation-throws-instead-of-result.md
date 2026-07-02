# ADR-009: ValidationBehavior throws instead of returning Result.Failure

## Status
Accepted — 2026-06-30

## Context
[ADR-008](ADR-008-result-pattern.md) established `Result<T>` as the pattern for expected domain failures. Validation failures are *also* expected — a client sending an invalid command is not an
exceptional condition, it is anticipated input we model and respond to.

The pure form of ADR-008 would say validation should return `Result.Failure(error)` too, not throw. But the MediatR pipeline-behavior
implementation faces a real technical obstacle:

A pipeline behavior is generic over `<TRequest, TResponse>`. When validation fails, the behavior must construct a `TResponse` that
represents failure. But:
- For `IRequest<Result<TripId>>`, the failure is `Result.Failure<TripId>(error)`
- For `IRequest<Result>`, the failure is `Result.Failure(error)`
- For `IRequest<SomeDto>`, there is no failure shape at all

Constructing the right failure value generically requires either:
1. Reflection to inspect TResponse and call the matching factory
2. A custom result-aware behavior with a marker interface
3. A third-party library like `Ardalis.Result` that ships its own
   FluentValidation integration

## Decision
**ValidationBehavior throws `FluentValidation.ValidationException`**
when validation fails. The API layer catches this exception centrally and maps it to RFC 7807 ProblemDetails (HTTP 400).

Validation failures are the **only** kind of failure that flows via exception. Domain failures continue to use `Result<T>` per ADR-008.

## Consequences
**Positive:**
- Pipeline behavior is simple — no generic Result-construction logic
- FluentValidation's `ValidationException` carries structured `ValidationFailure` records ready for ProblemDetails mapping
- Handlers don't need to handle validation failure at all — the pipeline short-circuits before the handler runs
- No third-party dependency beyond FluentValidation itself

**Negative:**
- Small philosophical inconsistency: validation failures throw, all other "expected" failures return `Result.Failure`
- Two failure paths in the API layer (one for exceptions, one for Result.Failure) — must be mapped consistently to ProblemDetails
- Less testable than pure Result — `Assert.Throws` instead of `result.IsFailure.Should().BeTrue()`

## Mitigations
The API layer applies a single mapping convention:
- `ValidationException` → 400 ProblemDetails with per-field errors
- `Result.Failure` with code matching `.NotFound` → 404
- `Result.Failure` with code matching `.AlreadyExists` / `.AlreadyArchived` → 409
- All other `Result.Failure` → 400

So while the underlying mechanism differs (throw vs return), the client-facing behavior is uniform.

## Alternatives considered
- **Pure Result<T> pipeline behavior** — rejected. The generic Result-construction logic is brittle, requires marker interfaces on every Result type, and gains little over the throw-based
  approach since the API layer normalises both paths anyway.
- **Inline validation in each handler** — rejected. Defeats the point of a pipeline behavior; spreads validation logic across every handler.
- **`Ardalis.Result` library** — considered. Its FluentValidation integration solves the generic problem cleanly, but adds a third-party dependency for a single behavior. Revisit if
  validation complexity grows.

## What this means in practice
- Handlers ignore validation failure — by the time `Handle` runs, the request has already passed validation
- Tests for handlers can assume valid input (one fewer setup concern)
- Tests for validators are separate — `new CreateTripCommandValidator().TestValidate(command)` style

## Revisit when
- Validation gets cross-cutting (e.g., must check Postgres before rejecting) — at which point an async, repository-aware validator starts looking like a domain check and may want to return `Result.Failure` for consistency
- An interviewer probes the inconsistency in code review