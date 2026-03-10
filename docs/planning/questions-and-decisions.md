# Questions & Decisions for GolfManager v2

## 🤔 Questions Before We Start

### 1. Multi-Tenancy & User Model

**Question**: How should we handle users who participate in multiple leagues?

**Options**:
- A) One User account, multiple Golfer profiles (one per league)
- B) Separate accounts per league
- C) Single User with league memberships

**Recommendation**: Option A - One User, multiple Golfer profiles
- User has global identity (email, password)
- Each league participation creates a GolferProfile
- Allows different display names, stats per league
- Maintains single sign-on experience

**Your Input Needed**: Does this align with your vision?

---

### 2. League Isolation

**Question**: Should leagues be completely isolated or allow cross-league features?

**Considerations**:
- Can users see their stats across all leagues?
- Can leagues share course databases?
- Can there be inter-league tournaments?

**Recommendation**: 
- Leagues are isolated for scoring/standings
- Courses are shared globally (not tenant-specific)
- User can view personal stats across leagues
- Future: Support for inter-league events

**Your Input Needed**: Any specific cross-league features you want?

---

### 3. Handicap System Priority

**Question**: Which handicap systems should we implement first?

From HolyGrail, we saw:
- Bob's Famous Method (custom)
- USGA
- Scratch
- None

**Recommendation**: Implement in this order:
1. None (raw scores only)
2. Bob's Method (since it's custom and used)
3. USGA (standard)
4. Scratch

**Your Input Needed**: Is Bob's Method well-documented? Do we have the algorithm?

---

### 4. Real-time Updates Scope

**Question**: What should trigger SignalR notifications?

**Potential Triggers**:
- Score entry/update
- Round completion
- Handicap recalculation
- Match completion
- Leaderboard changes
- Event start/end
- Team standings update
- Administrative announcements

**Recommendation**: Start with:
- Score updates (for live leaderboards)
- Round completion
- Event notifications

**Your Input Needed**: What's most important for real-time updates?

---

### 5. Mobile App Priority

**Question**: Should we build the API with mobile-first considerations?

**Considerations**:
- Offline support for score entry?
- Push notifications (beyond SignalR)?
- Camera integration for scorecards?
- GPS for course location?

**Recommendation**: 
- API should support offline-first patterns
- Include batch score submission endpoint
- Design for mobile constraints (data size, battery)

**Your Input Needed**: Any specific mobile features you envision?

---

### 6. Scoring Complexity

**Question**: How complex should the scoring system be initially?

From HolyGrail, we saw:
- Individual scoring (Two-Point, Stableford)
- Team scoring (Match Points)
- Missing player handling
- Max score caps for handicap

**Recommendation**: Phase approach:
- Phase 1: Basic raw scores, simple handicap
- Phase 2: Individual scoring types
- Phase 3: Team/match scoring
- Phase 4: Advanced rules (missing players, etc.)

**Your Input Needed**: Can we simplify initially or need full complexity?

---

### 7. Data Migration

**Question**: Will we need to migrate data from HolyGrail v1?

**Considerations**:
- Historical rounds and scores
- Golfer profiles
- Course data
- Season history

**Recommendation**: 
- Build migration tools as separate project
- Support import from HolyGrail database
- Provide data validation and cleanup

**Your Input Needed**: Is migration from v1 required?

---

### 8. Admin Features

**Question**: What admin capabilities are essential for v1?

**Potential Features**:
- League settings management
- Season creation and configuration
- Event scheduling
- Team management
- Golfer management
- Score corrections
- Handicap overrides
- Reporting and exports

**Recommendation**: Prioritize:
1. Season/Event management
2. Team/Golfer management
3. Score entry/correction
4. Basic reporting

**Your Input Needed**: Any critical admin features we're missing?

---

### 9. Course Database

**Question**: Should we build a comprehensive course database or keep it simple?

**Options**:
- A) Simple: Course name, tees, basic hole info
- B) Comprehensive: Full details, ratings, slopes, GPS coordinates
- C) Integrated: Use external course database API

**Recommendation**: Option B initially, with Option C for future
- Build our own database for leagues' regular courses
- Rich enough for handicap calculations
- Future: Integrate with external APIs for discovery

**Your Input Needed**: How many courses do you typically manage?

---

### 10. API Versioning Strategy

**Question**: How should we version the API?

**Options**:
- A) URL versioning: `/api/v1/leagues`
- B) Header versioning: `Accept: application/vnd.golfmanager.v1+json`
- C) Query parameter: `/api/leagues?version=1`

**Recommendation**: Option A (URL versioning)
- Most explicit and discoverable
- Easy to test and document
- Clear deprecation path

**Your Input Needed**: Any preference?

---

### 11. Authentication Flow

**Question**: What authentication methods should we support?

From fifthbox-appbase, we saw:
- Username/Password
- OAuth (Google, Microsoft)
- Mobile tokens
- Refresh tokens

**Recommendation**: Support all from day one:
- Email/Password for web
- OAuth for easy sign-up
- Mobile-specific tokens for MAUI app
- Refresh token rotation for security

**Your Input Needed**: Any other auth providers (Apple, Facebook)?

---

### 12. Deployment Target

**Question**: Where will this be deployed?

**Options**:
- Self-hosted (on-premises)
- Azure
- AWS
- Other cloud provider

**Recommendation**: Design for cloud but support self-hosted
- Use cloud-agnostic patterns
- Docker containers
- Environment-based configuration

**Your Input Needed**: What's your deployment preference?

---

## 📝 Decisions Made

### ✅ Decision 1: Golfer Architecture (2026-03-10)

**Question**: Should golfers be global entities or league-specific? Should User and Golfer be separate?

**Decision**: **Golfers are GLOBAL entities with league-specific profiles, separate from User**

**Rationale**:
- **User** = Authentication & identity (required for everyone)
- **Golfer** = Player profile (optional, only for those who play)
- User (1) ←→ (0..1) Golfer (user can optionally be a golfer)
- Golfer (1) ←→ (1) User (every golfer must have a user)
- Golfer can join multiple leagues via `LeagueGolfer` entity
- Stats can be tracked globally (across all leagues) AND per-league
- Handicaps are maintained both globally and per-league
- Equipment (clubs) is global, not league-specific
- Rounds can be casual (no league) or league-specific

**User Types Supported**:
- **Golfer Users**: Have Golfer profile, can play
- **Admin Users**: League admins who don't play
- **Scorekeeper Users**: Enter scores but don't play
- **Viewer Users**: Family/friends who view scores
- **Future**: Sponsors, media, etc.

**Implementation**:
```
User (1:1) Golfer (global profile)
  ↓
Golfer (1:N) LeagueGolfer (league-specific profile)
  ↓
LeagueGolfer (1:N) SeasonGolfer (season participation)
```

**API Structure**:
- `/api/v1/golfers/me` - Global golfer profile & stats
- `/api/v1/golfers/me/stats` - Stats across ALL leagues
- `/api/v1/golfers/me/handicap` - Overall personal handicap
- `/api/v1/leagues/{key}/golfers/{id}/stats` - League-specific stats
- `/api/v1/leagues/{key}/golfers/{id}/handicap` - League handicap

**Benefits**:
- Single sign-on across all leagues
- Comprehensive personal statistics
- League-specific customization (display name, avatar per league)
- Support for casual rounds outside of league play
- Better data portability and user experience

---

### ✅ Decision 2: Custom Domain Support (2026-03-10)

**Question**: Should leagues be able to use custom domains for branding?

**Decision**: **YES - Support custom domains with DNS verification**

**Implementation**:
- League can set custom domain (e.g., `digikeygolf.com`)
- DNS verification via TXT record
- Automatic SSL certificate provisioning
- Domain resolution middleware for routing
- Support both subdomain (`league.golfmanager.app`) and custom domain patterns

**Benefits**:
- Professional branding for leagues
- Better user experience
- SEO benefits for leagues
- White-label potential

**See**: [Custom Domain Architecture](../architecture/custom-domains.md)

---

### ✅ Decision 3: GPS-Based Hole Detection (2026-03-10)

**Question**: Should we support GPS-based hole detection for mobile app?

**Decision**: **YES - Full GPS support for future mobile app**

**Implementation**:
- Store GPS coordinates for each hole (tee box, green, fairway)
- Geofence-based hole detection
- Distance calculation using Haversine formula
- Auto-populate tee club based on distance
- Club suggestions based on golfer's average distances
- Course mapping tools for admins

**Data Model**:
- `Hole.TeeBoxLatitude/Longitude` - Tee box location
- `Hole.GreenLatitude/Longitude` - Green center
- `Hole.GeofenceRadius` - Detection radius (default 50m)
- `HoleTee.TeeBoxLatitude/Longitude` - Tee-specific overrides

**Mobile Features**:
- Auto-detect current hole
- Real-time distance to green
- Club suggestions
- Shot tracking (future)
- Offline course data (future)

**Benefits**:
- Seamless mobile experience
- Automatic score entry assistance
- Better data collection
- Competitive advantage

**See**: [GPS Hole Detection Architecture](../architecture/gps-hole-detection.md)

---

### ✅ Decision 4: Financial Management System (2026-03-10)

**Question**: Should we support financial management for leagues and platform subscriptions?

**Decision**: **YES - Full financial management with two-sided payments**

**Two-Sided Revenue Model**:

1. **Golfers → League** (Player Payments)
   - League dues (annual, seasonal, monthly)
   - Event entry fees
   - Skins game buy-ins
   - Guest fees
   - Merchandise

2. **League → GolfManager** (Platform Subscriptions)
   - Monthly/annual subscription tiers
   - Transaction fees on player payments
   - Premium features (custom domain, analytics)

**Implementation**:
- **Stripe Connect** for league payments (connected accounts)
- **Stripe Subscriptions** for platform fees
- League financial dashboard
- Golfer payment portal
- Automated invoicing and receipts
- Payout management for leagues

**Subscription Tiers**:
- **Free**: Up to 20 golfers, 3% transaction fee
- **Basic** ($29/mo): Up to 50 golfers, 2.5% fee
- **Pro** ($99/mo): Up to 200 golfers, custom domain, 2% fee
- **Enterprise** ($299/mo): Unlimited, white-label, 1.5% fee

**Data Model**:
- `LeagueSubscription` - Platform subscription
- `LeagueInvoice` - League pays GolfManager
- `GolferPayment` - Golfer pays league
- `GolferBalance` - Track who owes what
- `PaymentSchedule` - Recurring payments
- `Payout` - League withdraws funds

**Benefits**:
- Revenue stream for GolfManager platform
- Simplified finances for league admins
- Easy online payments for golfers
- Automated tracking and reporting
- Professional invoicing and receipts
- Reduced cash/check handling

**Timeline**: Phase 9 (after core league management is stable)

**See**: [Financial Management Architecture](../architecture/financial-management.md)

---

### ✅ Decision 5: One-Time Tournament Events (2026-03-10)

**Question**: Should we support standalone tournaments without requiring a league?

**Decision**: **YES - One-time tournament events with pay-per-event model**

**Concept**: Users can host standalone tournaments (scrambles, charity events, corporate outings) without creating a full league.

**Key Features**:
- Pay-per-event pricing ($49-$199 per tournament)
- No league/season required
- Team-based formats (scramble, best ball, etc.)
- Real-time scoring and leaderboards
- Public or private events
- Hole games (closest to pin, longest drive)
- Entry fee collection (optional)
- Mobile score entry
- Live SignalR updates

**Tournament Formats**:
- Scramble (2-man, 4-man)
- Best Ball
- Stroke Play
- Chapman/Pinehurst
- Shamble

**Pricing Tiers**:
- **Basic** ($49): Up to 50 teams, real-time scoring
- **Premium** ($99): Up to 100 teams, custom branding, hole games
- **Enterprise** ($199): Unlimited teams, white-label

**Data Model**:
- `OneTimeEvent` - Standalone tournament
- `OneTimeEventTeam` - Team registration
- `OneTimeEventPlayer` - Team members
- `OneTimeEventScore` - Hole-by-hole scores
- `OneTimeEventHoleGame` - Side games (closest to pin, etc.)

**Benefits**:
- Lower barrier to entry (no subscription needed)
- New revenue stream (pay-per-event)
- Viral growth potential (public events)
- Upsell path to league subscriptions
- Perfect for charity/corporate events

**Use Cases**:
- Charity scrambles
- Corporate outings
- One-time fundraisers
- Casual tournaments
- Golf course events

**Timeline**: Phase 7-8 (after core league management, before/alongside mobile features)

**See**: [One-Time Events Architecture](../architecture/one-time-events.md)

---

## 🎯 Action Items

1. Review these questions
2. Provide feedback and preferences
3. Make final decisions on critical items
4. Update project plan based on decisions
5. Begin detailed design phase

