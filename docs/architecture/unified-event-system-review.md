# Unified Event System Architecture - Review & Recommendations

## 📋 Executive Summary

This document reviews the proposed Unified Event System architecture and provides recommendations for implementation based on the existing codebase.

## ✅ What's Good

### 1. **Solid Foundation Already Exists**
The current codebase already has many of the pieces we need:
- ✅ `SeasonEvent` entity with course, tee, date
- ✅ `HolesPlayed` enum (None, Nine, Eighteen)
- ✅ `ScoringFormat` enum with StrokePlay, MatchPlay, Stableford, Scramble, BestBall
- ✅ `Round` and `RoundHole` entities for scoring
- ✅ Multi-tenant architecture with `ITenantEntity`

### 2. **Clear Separation of Concerns**
The architecture properly separates:
- One-time events (standalone)
- League events (season-based)
- Shared scoring logic

### 3. **Comprehensive Coverage**
The design covers:
- Multiple tournament formats
- Team and individual play
- Payment integration
- Real-time features
- Mobile-first scoring

## 🔍 Key Observations & Recommendations

### Observation 1: Enum Naming Inconsistency

**Current State:**
- Architecture doc uses: `TournamentFormat` enum
- Existing code uses: `ScoringFormat` enum

**Recommendation:**
**Keep `ScoringFormat`** - It's already implemented and used in `SeasonEvent`. The architecture doc should be updated to use `ScoringFormat` instead of `TournamentFormat`.

**Rationale:**
- Less breaking changes
- `ScoringFormat` is actually more accurate (describes HOW scoring works)
- Already has the formats we need (StrokePlay, MatchPlay, Stableford, Scramble, BestBall)

**Action Items:**
1. Update architecture doc to use `ScoringFormat` instead of `TournamentFormat`
2. Add missing formats to `ScoringFormat` enum: Chapman, Shamble, Skins, Nassau, Vegas
3. Keep the existing values (0-5) and add new ones starting at 6

---

### Observation 2: HolesPlayed Enum Missing Front/Back

**Current State:**
- Architecture doc includes: `Front` and `Back` (for front 9 or back 9)
- Existing enum only has: `None`, `Nine`, `Eighteen`

**Recommendation:**
**Add `Front` and `Back` to existing enum** - This is useful for leagues that play only front or back 9.

**Action Items:**
1. Add `Front = 91` to `HolesPlayed` enum
2. Add `Back = 92` to `HolesPlayed` enum
3. Update any scoring logic to handle these cases

---

### Observation 3: SeasonEvent Already Has Most Properties

**Current State:**
`SeasonEvent` already has:
- ✅ EventDate
- ✅ CourseId, TeeId
- ✅ HolesPlayed
- ✅ ScoringFormat
- ✅ Name, Description
- ✅ IsLocked
- ❌ Missing: TeamSize, UseHandicaps, Status

**Recommendation:**
**Enhance `SeasonEvent` with missing properties** instead of creating a new interface.

**Action Items:**
1. Add `TeamSize` (int, default 1 for individual)
2. Add `UseHandicaps` (bool, default true)
3. Add `Status` (EventStatus enum - new)
4. Consider adding `IsActive` (bool) for soft delete

---

### Observation 4: Round Entity Needs Enhancement

**Current State:**
The `Round` entity likely needs to support:
- Team rounds (for scrambles)
- One-time event rounds
- Multiple event contexts

**Recommendation:**
**Add optional foreign keys to Round** for flexibility.

**Action Items:**
1. Add `OneTimeEventId` (string?, nullable)
2. Add `OneTimeEventTeamId` (string?, nullable)
3. Add `IsTeamRound` (bool, default false)
4. Add `Format` (ScoringFormat?, nullable) to override event format if needed

---

### Observation 5: Payment & Pricing Model

**Current State:**
The architecture includes detailed pricing tiers for one-time events.

**Recommendation:**
**Start simple, add complexity later**.

**Phase 1 (MVP):**
- Single flat fee ($49) for all one-time events
- No team limits
- Basic features only

**Phase 2 (Growth):**
- Add tiered pricing (Basic, Premium, Enterprise)
- Add team limits
- Add advanced features

**Rationale:**
- Faster time to market
- Validate demand before building complex pricing
- Easier to test and iterate

---

### Observation 6: Multi-Tenancy for One-Time Events

**Current State:**
- League events use `ITenantEntity` with `LeagueId` for isolation
- One-time events are standalone (no league)

**Question:**
How do we handle multi-tenancy for one-time events?

**Recommendation:**
**Use `OrganizerId` as the tenant boundary** for one-time events.

**Approach:**
1. One-time events belong to the user who created them (organizer)
2. Organizer has full control (edit, delete, view registrations)
3. Public events are viewable by anyone
4. Private events require registration code
5. No global query filter needed (events are naturally isolated by organizer)

**Security:**
- Only organizer can edit/delete event
- Only organizer can see payment details
- Anyone can view public events
- Only registered teams can submit scores

---

### Observation 7: Team Management Complexity

**Current State:**
- League events use `SeasonTeam` (season-long teams)
- One-time events need `OneTimeEventTeam` (event-specific teams)

**Recommendation:**
**Keep them separate** - Don't try to unify team entities.

**Rationale:**
- Different lifecycles (season-long vs single event)
- Different registration flows
- Different payment models
- Different permissions

**Shared Logic:**
- Scoring calculations (same)
- Leaderboard display (same)
- Scorecard entry (same)

---

### Observation 8: Real-Time Features (SignalR)

**Current State:**
Architecture includes SignalR for live leaderboards.

**Recommendation:**
**Phase this in after core functionality works**.

**Phase 1 (MVP):**
- Manual refresh for leaderboards
- Simple polling (every 30 seconds)

**Phase 2 (Enhancement):**
- SignalR for live updates
- Real-time score notifications
- Live position changes

**Rationale:**
- SignalR adds complexity
- Core functionality is more important
- Can validate demand first

---

## 🎯 Recommended Implementation Order

### Phase 1: Foundation (Week 1-2)

**Priority: HIGH**

1. **Update Existing Enums**
   - Add missing formats to `ScoringFormat` (Chapman, Shamble, Skins, Nassau, Vegas)
   - Add `Front` and `Back` to `HolesPlayed`
   - Create `EventStatus` enum (Draft, Published, InProgress, Completed, Cancelled)
   - Create `EventAccessType` enum (Public, Private, InviteOnly)

2. **Enhance SeasonEvent**
   - Add `TeamSize` property
   - Add `UseHandicaps` property
   - Add `Status` property (EventStatus)
   - Update database migration

3. **Create One-Time Event Entities**
   - `OneTimeEvent` (with all properties from architecture)
   - `OneTimeEventTeam` (simplified - no payment initially)
   - `OneTimeEventPlayer`
   - Database migrations

4. **Enhance Round Entity**
   - Add `OneTimeEventId` (nullable)
   - Add `OneTimeEventTeamId` (nullable)
   - Add `IsTeamRound` (bool)
   - Add `Format` (ScoringFormat?, nullable)
   - Update database migration

### Phase 2: One-Time Event CRUD (Week 3)

**Priority: HIGH**

5. **Create DTOs**
   - `CreateOneTimeEventRequest`
   - `UpdateOneTimeEventRequest`
   - `OneTimeEventResponse`
   - `OneTimeEventTeamResponse`
   - `RegisterTeamRequest`

6. **Create Services**
   - `IOneTimeEventService`
   - `OneTimeEventService` (CRUD operations)
   - Team registration logic
   - Event publishing logic

7. **Create API Endpoints**
   - `OneTimeEventsController`
   - CRUD endpoints
   - Team registration endpoint
   - Public event listing

### Phase 3: Scoring System (Week 4)

**Priority: HIGH**

8. **Create Shared Scoring Service**
   - `IEventScoringService`
   - `EventScoringService`
   - Support for StrokePlay (individual)
   - Support for Scramble (team)
   - Leaderboard calculation

9. **Create Scoring Endpoints**
   - Submit score endpoint (works for both event types)
   - Get leaderboard endpoint (works for both event types)
   - Get round endpoint

### Phase 4: UI Implementation (Week 5-6)

**Priority: MEDIUM**

10. **Create Event Creation UI**
    - Event creation wizard (multi-step form)
    - Format selection
    - Course/tee selection
    - Access control settings

11. **Create Public Event Page**
    - Event details display
    - Team registration form
    - Current registrations list
    - Leaderboard (during event)

12. **Create Mobile Scorecard**
    - Hole-by-hole score entry
    - Works for both individual and team
    - Works for both event types
    - Simple, clean, mobile-first

### Phase 5: Payment Integration (Week 7)

**Priority: MEDIUM**

13. **Stripe Integration**
    - Checkout flow
    - Payment verification
    - Event activation after payment
    - Start with flat $49 fee

### Phase 6: Enhancement (Week 8+)

**Priority: LOW**

14. **Real-Time Features**
    - SignalR hub
    - Live leaderboard updates
    - Score notifications

15. **Advanced Formats**
    - Best Ball scoring
    - Match Play scoring
    - Stableford scoring

16. **Tiered Pricing**
    - Basic/Premium/Enterprise tiers
    - Team limits
    - Feature gating

---

## 🚨 Critical Decisions Needed

### Decision 1: Enum Naming
**Question:** Use `TournamentFormat` (architecture doc) or `ScoringFormat` (existing code)?

**Recommendation:** Use `ScoringFormat` (existing)

**Your Decision:** _______________

---

### Decision 2: Payment Timing
**Question:** When should we implement payment integration?

**Options:**
- A) Phase 1 (before any UI) - Blocks testing
- B) Phase 5 (after core features) - Can test without payment ✅ **RECOMMENDED**
- C) Phase 6+ (enhancement) - May forget to add it

**Your Decision:** _______________

---

### Decision 3: Real-Time Features
**Question:** When should we implement SignalR?

**Options:**
- A) Phase 1 (foundation) - Adds complexity early
- B) Phase 4 (with UI) - Good for demo
- C) Phase 6 (enhancement) - After core works ✅ **RECOMMENDED**

**Your Decision:** _______________

---

### Decision 4: MVP Scope
**Question:** What's the absolute minimum for a working one-time event?

**Recommendation:**
1. Create event (basic info only)
2. Register teams (no payment)
3. Enter scores (stroke play only)
4. View leaderboard (manual refresh)

**Your Decision:** _______________

---

## 📊 Updated Architecture Alignment

### Changes to Architecture Doc

1. **Rename `TournamentFormat` → `ScoringFormat`** throughout
2. **Add `Front` and `Back` to `HolesPlayed` enum**
3. **Simplify payment model** for MVP (flat $49)
4. **Move SignalR to Phase 6** (enhancement)
5. **Add multi-tenancy notes** for one-time events

### No Changes Needed

- Entity structure is solid
- API design is good
- UI component breakdown is clear
- Workflow descriptions are accurate

---

## ✅ Summary & Next Steps

### What We Learned

1. **Existing code is well-structured** - We can build on it
2. **Enum naming matters** - Need to align architecture with code
3. **MVP scope is critical** - Start simple, add complexity later
4. **Payment can wait** - Core functionality first

### Recommended Next Action

**Update the architecture document** with the observations from this review, then proceed with Phase 1 implementation.

**Specifically:**
1. Update `unified-event-system.md` to use `ScoringFormat` instead of `TournamentFormat`
2. Add implementation notes about existing entities
3. Clarify MVP scope vs future enhancements
4. Add decision log for key choices

---

**Would you like me to:**
- **A)** Update the architecture document with these recommendations
- **B)** Start implementing Phase 1 (enums and entity enhancements)
- **C)** Create a detailed task breakdown for the MVP
- **D)** Discuss any of the critical decisions above

Let me know! 🎯

