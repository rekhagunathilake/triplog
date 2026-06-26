### ADR-008: Result<T> pattern over thrown exceptions

## Status
Accepted — 2026-06-21

## Context
Domain operations can fail for predictable reasons:
- A `TripTitle` can be empty or too long
- A status transition can be illegal (`Activate` on a Completed trip)
- An invariant can be violated (archive an already-archived entry)

These are not exceptional in the computer-science sense — they are expected outcomes the caller must handle. .NET's default for failure
is to throw exceptions; the alternative is to return failures as values.

## Decision
**Domain failures are returned as values** via a `Result` / `Result<T>` type. Every Domain factory and state-transition method
returns one of these. Callers explicitly check `IsSuccess` / `IsFailure` and inspect `.Error` for the failure reason.

Exceptions are reserved for:
- Programmer errors (calling `.Value` on a failed `Result`)
- Infrastructure failures (database unreachable, message broker down)
- Truly unexpected conditions

## Consequences
**Positive:**
- Failure modes are visible in method signatures — a reader sees `Result<Trip>` and immediately knows the operation can fail
- No hidden control flow via exceptions — easier to reason about
- The compiler enforces handling failures (you must read `.Error`)
- Error codes (e.g., `Trip.InvalidStatusTransition`) flow cleanly from Domain to Application to API layer for HTTP status mapping
- Stack traces are not used as error-handling control flow

**Negative:**
- Callers write slightly more code (`if (result.IsFailure)` blocks)
- The pattern is not idiomatic .NET — developers coming from other ASP.NET codebases may find it unfamiliar
- Requires a small abstraction (`Result`, `Result<T>`, `Error`) in every Domain project

## API-layer mapping
Errors carry a `Code` (e.g., `"Trip.NotFound"`, `"Entry.IsArchived"`) and the API layer maps code patterns to HTTP status:

| Code pattern    | HTTP status |
|-----------------|-------------|
| `.NotFound`     | 404         |
| `.AlreadyExists`, `.AlreadyArchived` | 409 |
| All others      | 400         |

Mapped centrally in the API layer via an `IExceptionHandler` / result-extension. Domain remains transport-agnostic.

## Alternatives considered
- **Throw `DomainException` subclasses** — rejected because failure becomes invisible in signatures and the catching layer must know every exception type
- **Throw a single generic exception with an error code** — rejected as it hides failure modes and conflates infrastructure errors with domain failures
- **Return `(Trip?, Error?)` tuples** — rejected as less ergonomic than a `Result<T>` with explicit `IsSuccess` semantics

## Reference
The pattern mirrors the `Result` types in Rust, Kotlin's `Result`, F#'s `Result`, and the implementation used in `SaasContentLibrary`
(triplog's predecessor portfolio project).