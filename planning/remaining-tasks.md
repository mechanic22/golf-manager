# Golf Manager - Remaining Tasks Plan

Based on a thorough analysis of the golf-manager project (including code, documentation, planning files, and tests), here's a structured plan of what's left to complete. The project is well-architected with solid foundations, but overall progress is estimated at ~35% of the full feature set. I've organized this by priority phases, focusing on actionable next steps.

## Current Status Summary
- **Strengths**: Multi-tenant architecture, authentication, core API controllers, database schema, integration tests, and comprehensive planning docs are complete or near-complete.
- **Progress by Phase**:
  - Phase 1 (Foundation): ~75% complete ✅
   - Phase 2 (League & Season Management): ~80% complete ⏳ (custom domain verification + multitenancy auth hardening completed)
  - Phase 3 (Player & Scoring): ~30% complete ⏳
  - Phases 4-7 (Course Management, Real-time, Advanced Features, Clients): 0-15% complete 🚫
- **Key Gaps**: Real-time updates (SignalR), background job processing, full handicap algorithms, payment integration, mobile app, and importer hardening/validation are not started or partial.
- **Recent Completions**:
   - ✅ Custom domain verification system (backend validation, frontend UI, middleware resolution, authorization controls)
   - ✅ HolyGrail importer expanded to include rounds, round holes, scorecard creation, and tee/hole schema compatibility updates
   - ✅ Import verification reporting (`LogImportSummary` — logs per-entity counts and post-import DB row counts)
   - ✅ Course CRUD API — `ICourseService`/`CourseService` + `CoursesController` with full CRUD, tee management, `?includeTees` and `?includeHoles` query params, key-based lookup
   - ✅ Handicap calculation algorithms — `CalculateHandicapAsync` added to `IHandicapService`/`HandicapService` with WHS (best-N differentials × 0.96), Bob's League (avg-over-par × 0.80), and Scratch methods; `POST /api/v1/golfers/{id}/handicap/calculate` endpoint added
   - ✅ Multitenancy/auth flow hardening — auth endpoints bypass tenant-context enforcement (`/api/v1/auth/*`) to prevent stale context blocking login/session flows
   - ✅ Tenant access-denied UX — dedicated `/access-denied` page added; league and season pages route forbidden (`403`) responses to explicit tenant-denied messaging
   - ✅ Season-scoped deny routing — season paths now prefer `scope=season` with `leagueKey` + `seasonKey` in access-denied redirects for accurate user context
   - ✅ Live verification rerun — authenticated/anonymous redirects and cross-tenant deny flows revalidated in browser
   - ✅ Role-flow hardening (May 13, 2026): global admins can access league settings UI; owner role assignment/removal now restricted to league owner or global admin; owner membership actions are protected in both API service logic and member-management UI

## Role Visibility Matrix (Current)
- **Global Admin**: Can view/manage all leagues, settings, members, custom domains, and owner-level member role actions across leagues.
- **League Owner**: Can manage league settings, members, and owner role assignments within their league.
- **League Admin**: Can manage league settings and members, but cannot assign/remove/modify owner memberships.
- **Member / Viewer / Player**: Can view league/member content permitted by membership and tenant context; cannot access management actions.

## Immediate Tasks (Next 1-2 Weeks)
These are critical fixes and completions to stabilize the current foundation:

1. **Fix Code TODOs**:
   - ✅ Update `Dashboard.razor` to load and display upcoming events from the API.
   - ✅ Implement audit trail in `GolfManagerDbContext.cs` to populate `CreatedBy`/`UpdatedBy` from current user context.
   - ✅ Import verification reporting — post-import summary + DB row-count spot-check added to `HolyGrailImporter`.

2. **Complete Phase 2 (League Management)**:
   - ✅ ~~Finish custom domain verification endpoints~~ (COMPLETED)
   - ✅ ~~Implement domain resolution middleware~~ (COMPLETED)
   - ✅ ~~Add custom domain display UI with authorization controls~~ (COMPLETED)
   - ✅ Implement Season endpoints and management UI.
   - ✅ Add League member role management (Owner, Admin, Member, Viewer).

3. **Enhance Testing**:
   - Add unit tests for service layers (e.g., HandicapService, EventService).
   - Expand integration tests for handicap calculations and multi-tenant isolation.
   - Add regression tests for tenant-forbidden routing behavior (league scope vs season scope) and auth endpoint tenant-context bypass.
   - Add regression tests for owner-role restrictions (only owner/global admin can assign/remove owner role).

4. **Documentation Updates**:
   - Complete OpenAPI/Swagger docs with response examples.
   - Add entity relationship diagrams.

## Short-Term Goals (Next 1-3 Months)
Focus on core functionality to make the platform usable for basic league operations:

1. **Complete Phase 3 (Player & Scoring)**:
   - ✅ Handicap calculation service — WHS, Bob's League, and Scratch algorithms implemented.
   - Implement background job queue (e.g., Hangfire) for async handicap recalculations.
   - Complete match play and team scoring logic.

2. **Phase 4 (Course Management)**:
   - ✅ Course CRUD endpoints built (`GET`, `POST`, `PUT`, `DELETE /api/v1/courses`, tee sub-resources, key-based lookup).
   - Add course rating data management (bulk hole-tee import/update UI).

3. **One-Time Events System**:
   - Complete public event discovery pages.
   - Finish event organizer dashboard and live leaderboards.
   - Integrate payment processing for entry fees (Stripe/Square).

4. **Infrastructure Improvements**:
   - Add caching layer (Redis) for leaderboards and sessions.
   - Implement structured logging (Serilog).
   - Add API rate limiting and harden CORS configuration.

## Medium-Term Goals (3-6 Months)
Expand to advanced features for a production-ready platform:

1. **Phase 5 (Real-time Features)**:
   - Implement SignalR hubs (ScoreHub, EventHub, LeagueHub) for live score updates and notifications.

2. **Phase 6 (Advanced Features)**:
   - Complete match scoring calculations and team algorithms.
   - Build statistics/analytics APIs and reporting endpoints.

3. **Security & Performance**:
   - Implement data encryption for sensitive fields.
   - Add monitoring/observability tools.
   - Optimize queries with materialized views and indexing.

## Long-Term Goals (6+ Months)
Future enhancements for full platform maturity:

1. **Phase 7 (Client Applications)**:
   - Complete Blazor WebAssembly admin dashboards.
   - Develop MAUI mobile app (deferred but planned).

2. **Additional Modules**:
   - Financial management (subscriptions, pay-per-event).
   - GPS features (hole detection, distance to green).
   - HolyGrail v1 migration hardening (idempotency, edge-case mapping, and post-import validation reports).
   - Email/SMS notifications and image storage.

3. **Scalability & Integrations**:
   - Event bus for domain events.
   - Analytics and performance tracking.

## Risks & Recommendations
- **High-Risk Gaps**: Lack of real-time features and background processing could limit user experience; prioritize SignalR and job queues.
- **Testing**: Current coverage is ~30-40%; aim for 70%+ before production.
- **Dependencies**: No blockers, but payment integration requires external APIs.
- **Team Size**: This roadmap assumes 1-2 developers; adjust timelines accordingly.
- **Next Steps**: Start with immediate tasks, then tackle Phase 3. Reassess progress after each phase.

## Next Priority Slice (Suggested)
1. Add integration tests for league member role transitions and owner-protection rules.
2. Add season management regression tests for admin/member visibility and forbidden routing.
3. Begin Hangfire/background-job spike for async handicap recalculation.

This plan aligns with the project's 7-phase roadmap in the planning docs.
