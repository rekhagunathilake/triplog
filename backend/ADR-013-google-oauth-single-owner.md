# ADR-013: Google OAuth with single-owner authorization

## Status
Accepted — 2026-09-04. Supersedes [ADR-006](ADR-006-no-auth-v1.md).

## Context
The v1.x roadmap targets a public deployment URL on the portfolio. A publicly reachable URL with unauthenticated writes turns the demo into a spam target and a moderation problem — the deploy would be a liability, not an asset. Auth needs to land before deploy.

At the same time, triplog is a solo-owner portfolio project — Rekha's travel journal, publicly readable. It is not a multi-tenant SaaS. Adding sign-up, per-user data isolation, and forgot-password flows would repeat every intro-to-web tutorial and add infrastructure unrelated to the distributed-systems patterns this repo exists to demonstrate.

## Decision
**Google OAuth with a single-owner authorization model.**

- **Reads are public.** All GET endpoints scope to a configured `PublicOwnerId` and require no authentication. Anonymous visitors browse as read-only viewers.
- **Writes require ownership.** POST/PUT/DELETE require a signed HS256 JWT bearer token whose `email` claim matches a configured `OwnerEmail`.
- **Identity via Google OAuth.** The frontend uses NextAuth v5 (Auth.js) with the Google provider. On sign-in, a callback issues a separate signed HS256 JWT (`session.apiToken`) that the frontend attaches to backend calls as `Authorization: Bearer`.
- **Shared HS256 secret.** `Auth:JwtSecret` (backend) and `AUTH_SECRET` (frontend) hold the same value. Symmetric because signer and verifier are the same party.
- **Backend never talks to Google.** Google is involved only in the OAuth handshake between browser and frontend. The backend has no OIDC discovery, no dependency on Google being reachable — just a shared secret it uses to verify tokens.

## Consequences

**Positive:**
- Public read + private write matches the actual product intent — no scope creep for multi-tenancy the project does not need
- No user management infrastructure (no user table, no sign-up flow, no admin surface)
- Backend auth is small: JWT bearer + one authorization policy against a configured email
- Deploy is safe — visitors can browse but not write
- OAuth via a real provider demonstrates the pattern without inheriting its full complexity
- Symmetric secret is honest — this project is one team, not federated

**Negative:**
- Model does not scale beyond a single owner. Adding a second author would require domain scoping changes and adapter changes (email allowlist → tenant lookup)
- Two secrets to manage: `AUTH_GOOGLE_SECRET` (OAuth handshake) and `AUTH_SECRET` (JWT signing, present in three locations that must stay in sync)
- Two token concepts coexist — the NextAuth session cookie (JWE, opaque to the backend) and the API token (JWS, verifiable by the backend). Worth understanding, worth explaining to a reviewer

## Alternatives considered

- **Full OIDC with token validation against Google's JWKS.** Backend fetches Google's public keys, verifies Google-issued ID tokens directly. More "standard" but couples backend to Google, requires internet access at API startup, and adds a dependency the demo scope does not earn.
- **Server-only proxy pattern.** All backend calls go through Next.js server routes; backend accepts requests only from the proxy via a shared internal API key. Simpler backend auth, but couples the deployment (backend cannot serve directly) and forfeits an interview-visible JWT flow.
- **Password + JWT.** Classic full-stack auth demo. Adds password storage, hashing, sign-in endpoint, forgot-password UX — more code, no more portfolio signal than OAuth.

## v3 plan
For a hypothetical multi-writer version:
- Domain layer needs no changes — `OwnerId` is already strongly-typed
- Application layer would need per-user query scoping (currently defaults to `PublicOwnerId`)
- Authorization policy becomes claim-based (JWT `email` → user row lookup, not a configured constant)
- Sign-up, admin, moderation, quotas, etc. — all separate concerns

Not planned. Portfolio scope is single-owner.