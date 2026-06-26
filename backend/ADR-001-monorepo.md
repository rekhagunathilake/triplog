# ADR-001: Monorepo with single solution

## Status
Accepted — 2026-06-20

## Context
triplog has two .NET services (`entries-api`, `media-api`), a Next.js frontend, shared MassTransit message contracts, and Aspire orchestration.
This work could be organised as one repository or split across several (per service, plus a frontend repo, plus a contracts repo). The project's primary purpose is portfolio demonstration — a reader
should be able to clone once and see the full system at a glance.

## Decision
Single repository with a single `.slnx` solution file containing all backend projects (services, AppHost, ServiceDefaults, shared Contracts, test projects). The Next.js frontend lives in the same
repository under `frontend/web/` but outside the .NET solution.

## Consequences
**Positive:**
- One `git clone` reveals the entire system architecture
- `Triplog.Contracts` is consumed by both services via project references — no NuGet packaging, versioning, or feed plumbing
- A reviewer can navigate from API endpoint to saga handler to message contract in a single IDE session
- CI can build and test everything in one workflow

**Negative:**
- Doesn't reflect production reality, where each service typically lives in its own repository with independent deploy cadence
- All-or-nothing CI — a frontend change runs backend builds and vice versa (acceptable at this scale)
- Cross-service refactors cannot be reviewed independently per service

## Alternatives considered
- **One repo per service plus a frontend repo plus a contracts repo** —
  rejected because the reader has to traverse multiple repositories to understand a single saga flow. Defeats the portfolio purpose.
- **Two repos: backend monorepo + frontend repo** — rejected as partial; the frontend is a thin consumer of the backend APIs and benefits from co-location.

## Migration path if needed
If triplog were ever taken to production, the natural split would be one repository per service, with `Triplog.Contracts` published as a versioned NuGet package consumed by both. The current project
references would become package references.