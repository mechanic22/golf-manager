# Multitenancy Deployment Checklist

## Goal
Deploy with tenant-safe auth/session behavior and strict league data isolation.

## Required Behavior
- Anonymous user on `/` sees public home.
- Authenticated user hitting `/` is redirected to app home (`/dashboard` or tenant context home).
- Authenticated user hitting `/login` is redirected to app home.
- Logout clears app state and server session, then protected pages redirect to `/login`.
- Tenant membership is enforced for league/season resources.
- Auth endpoints (`/api/v1/auth/*`) are not blocked by stale tenant context headers.

## Verified In This Session
- Non-member user is blocked from cross-tenant league access with `403`.
- Authenticated `/` and `/login` redirects were implemented in web pages.
- Logout client request shape was updated to avoid invalid payload.
- League context header is skipped for auth endpoints in web client handler.
- API middleware now bypasses strict league-membership blocking for auth endpoints.

## Pre-Deploy Validation
1. Build and run API + Web from clean processes.
2. Verify login/logout flow for:
   - Existing league member
   - New user with zero leagues
3. Verify tenant isolation:
   - Member can access own league routes
   - Non-member cannot access another league ID/key resources
4. Verify role gates:
   - Non-global-admin blocked from `/admin/*`
   - Non-league-admin blocked from season settings/management pages
5. Smoke test browser flows:
   - `/` anonymous
   - `/` authenticated
   - `/login` authenticated
   - `/dashboard` after logout

## Deployment Configuration
- API base URL configured for web client.
- CORS origin set to deployed web host.
- Cookie auth security in production:
  - `Secure` enabled
  - `HttpOnly` enabled
  - `SameSite` set appropriately for your host/domain topology
- DataProtection keys persisted across restarts.
- Logging enabled for auth and tenant-middleware warnings.

## Production Monitoring (First 24h)
- Track `403` counts by endpoint and league key.
- Track auth endpoint failures (`/auth/me`, `/auth/login`, `/auth/logout`).
- Alert on unusual cross-tenant access attempts.

## Rollback Trigger
Rollback if any of the following are observed:
- Authenticated users intermittently redirected to `/login` while active.
- Non-members can read league data from another league.
- Logout leaves protected pages accessible without re-authentication.
