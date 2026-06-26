# ADR-004: Schema per service on shared Postgres

## Status
Accepted — 2026-06-20

## Context
Each microservice should own its data — a service must not read or write another service's tables. The strictest form of this is one
database server per service. In production this means real separation; in local development it means running multiple Postgres containers.

For a portfolio project, running two Postgres containers via Aspire adds friction without adding signal to the demonstration.

## Decision
One Aspire-managed Postgres container, with two **named databases** on it:
- `entries` — owned exclusively by entries-api
- `media` — owned exclusively by media-api

Each service connects only to its own database. No cross-database joins, no shared tables, no foreign keys across the schema boundary.

## Consequences
**Positive:**
- Service boundaries remain truthful — neither service can see the other's tables
- Faster local startup than two separate Postgres containers
- Lower local resource usage
- The boundary is enforced in code (each service has its own connection string and DbContext)

**Negative:**
- Not how this would be deployed to production (each service would have its own server or its own logical database with separate credentials)
- A misconfigured connection string could let a service connect to the wrong database — guarded by Aspire wiring + integration tests

## Migration path if needed
For production, each service gets its own Postgres instance (or its own logical database with isolated credentials). The application code
needs no changes — only the connection strings differ.

## What this is not
This is not a "shared database between services." It is two databases that happen to be hosted on the same server during local development. The schema boundary is real and enforced.