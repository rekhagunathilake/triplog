# ADR-002: Two services for the travel journal

## Status
Accepted — 2026-06-20

## Context
A travel journal is a small domain. A single ASP.NET Core service with one database would be the right choice for the actual product. The
decision to split into two services is not driven by product requirements — it is driven by the project's purpose, which is to demonstrate distributed-system patterns (Aspire orchestration, saga
coordination, cross-service tracing, schema-per-service boundaries).

A naive split would be arbitrary and reveal as such on inspection. A defensible split needs a real seam where two services genuinely earn their keep.

## Decision
Two services:
- **entries-api** — owns `Trip` and `Entry` aggregates. Small relational metadata, transactional, high read frequency
- **media-api** — owns `MediaItem` aggregate. Blob storage in MinIO, asynchronous post-processing (thumbnails, EXIF extraction, immutability), large payload sizes

The seam is the natural one between **transactional metadata** and **large-binary async processing** — different storage profiles, different scaling characteristics, different failure modes.

## Consequences
**Positive:**
- Demonstrates real distributed-system coordination via the publish saga (ADR-005)
- Storage profiles are genuinely different — defensible at every scale
- Failure modes are isolated — media processing failures don't crash entries

**Negative:**
- Over-engineered for the actual product
- Requires cross-service contracts, message versioning, and saga state management for what would otherwise be in-process method calls
- Two services means two deployment units, two test surfaces, two observability targets

## Honest framing for the README
This split is documented in the README's "Why microservices for a travel journal" section as a deliberate over-engineering, not a business need. The project's goal is *patterns*, not *product*.
Calling this out earns more credibility than pretending the split was necessary.

## What we explicitly rejected
- Splitting by HTTP method (read service vs. write service) — fashionable but produces fake CQRS without the costs being motivated
- Splitting per aggregate (Trip service, Entry service, Media service) — three services for a small domain creates more coordination than insight