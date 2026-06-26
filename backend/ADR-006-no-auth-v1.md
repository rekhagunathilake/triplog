# ADR-006: No authentication in v1

## Status
Accepted — 2026-06-21

## Context
A production travel journal needs authentication. The implementation choices (OIDC, session cookies, JWT, social login, magic links)
significantly affect the API surface, the test setup, the frontend flow, the saga's identity propagation, and the infrastructure footprint.

The project's purpose is to demonstrate distributed-system patterns (Aspire, saga, OpenTelemetry, Clean Architecture). Authentication
work would compete for time without adding to that goal.

## Decision
**No real authentication in v1.** Requests carry a fake `X-User-Id` header containing a `Guid`. The Application layer extracts this header
and constructs an `OwnerId` from it. No validation of the header beyond "parses as Guid" — there is no trust boundary.

Frontend hardcodes a single dev user's Guid into requests.

## Consequences
**Positive:**
- Domain modelling, saga implementation, and distributed-systems patterns can be the focus of v1
- Tests don't need to handle auth fixtures or token mocking
- No infrastructure footprint for an identity provider
- The `OwnerId` plumbing is in place from day one — v2 just swaps the source of the header for a verified claim

**Negative:**
- The README must be explicit that this is deliberately out of scope, not an oversight
- Anyone running the project locally has the same identity
- Cannot demonstrate authorisation patterns

## v2 plan
v2 adds authentication. Likely shape: an OIDC provider (Auth0 or a self-hosted Keycloak), JWT validation middleware on each service,
`OwnerId` extracted from the verified `sub` claim, frontend uses PKCE flow. The saga propagates the claim across the message boundary
via MassTransit message headers.

The Domain layer requires no changes — `OwnerId` is already a strongly-typed value passed in by the Application layer.