# Architecture Decision Records

Records of significant architectural decisions made during the design and implementation of triplog. Each ADR captures the context that motivated a
decision and the trade-offs accepted.

Format follows [Michael Nygard's template](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions).

| #   | Title                                                                  | Status   | Date       |
|-----|------------------------------------------------------------------------|----------|------------|
| 001 | [Monorepo with single solution](ADR-001-monorepo.md)                   | Accepted | 2026-06-20 |
| 002 | [Two services for the travel journal](ADR-002-two-services.md)         | Accepted | 2026-06-20 |
| 003 | [Entry as separate aggregate root](ADR-003-entry-aggregate.md)         | Accepted | 2026-06-20 |
| 004 | [Schema per service on shared Postgres](ADR-004-schema-per-service.md) | Accepted | 2026-06-20 |
| 005 | [Saga orchestration in entries-api](ADR-005-saga-orchestration.md)     | Accepted | 2026-06-21 |
| 006 | [No authentication in v1](ADR-006-no-auth-v1.md)                       | Accepted | 2026-06-21 |
| 007 | [Time injection in the Domain layer](ADR-007-time-injection.md)        | Accepted | 2026-06-21 |
| 008 | [Result<T> pattern over thrown exceptions](ADR-008-result-pattern.md)  | Accepted | 2026-06-21 |
| 009 | [ValidationBehavior throws instead of Result](ADR-009-validation-throws-instead-of-result.md) | Accepted | 2026-06-30 |
| 010 | [Design-time DbContext factory for EF migrations under Aspire](ADR-010-design-time-dbcontext-factory.md) | Accepted | 2026-07-13 |
| 011 | [In-memory domain event dispatch (outbox deferred)](ADR-011-in-memory-domain-event-dispatch.md) | Accepted | 2026-07-30 |
| 012 | [Saga design — retention, idempotency, retry](ADR-012-saga-design.md) | Accepted | 2026-07-30 |