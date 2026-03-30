# Unified Event System Architecture

## 🎯 Overview

The Unified Event System supports **both one-time tournaments and league events** with shared scoring logic, tournament formats, and team management.

## 🏗️ Core Principle

**Build once, use everywhere** - All scoring, tournament formats, and team management work for both:
- **One-Time Events**: Standalone tournaments (scrambles, charity events, corporate outings)
- **League Events**: Season-based events within leagues

## 📊 Entity Hierarchy

```
BaseEvent (Abstract)
├── OneTimeEvent (Standalone tournament)
│   ├── OneTimeEventTeam
│   └── OneTimeEventPlayer
└── SeasonEvent (League event)
    ├── SeasonTeam (reused from league)
    └── SeasonGolfer (reused from league)

Shared Components:
├── TournamentFormat (Scramble, BestBall, etc.)
├── Round (Individual or team scoring)
├── RoundHole (Hole-by-hole scores)
├── Scorecard (Scoring logic)
└── Course/Tee (Venue information)
```

## 🎮 Scoring Formats (Shared)

> **Note:** We use `ScoringFormat` enum (existing in codebase) instead of `TournamentFormat` to align with existing code.

All formats work for both event types:

### Individual Formats
- **Stroke Play**: Traditional individual scoring (EXISTING)
- **Match Play**: Head-to-head matches (EXISTING)
- **Stableford**: Points-based scoring (EXISTING)

### Team Formats
- **Scramble**: Best shot per hole (most popular) (EXISTING)
- **Best Ball**: Best individual score per hole (EXISTING)
- **Chapman**: Alternate shot after drives (NEW)
- **Shamble**: Scramble off tee, then individual (NEW)

### Advanced Formats (Future)
- **Skins**: Skin game (NEW)
- **Nassau**: Front 9, Back 9, Total (NEW)
- **Vegas**: Vegas scoring (NEW)

## 🔄 Event Type Comparison

| Feature | OneTimeEvent | SeasonEvent |
|---------|-------------|-------------|
| **Parent** | None | Season → League |
| **Cost Model** | Pay-per-event ($49-$199) | Included in subscription |
| **Duration** | Single day | Part of season |
| **Participants** | Anyone (public/private) | League members only |
| **Teams** | Event-specific | Season-long |
| **Handicaps** | Manual or none | Tracked over time |
| **Leaderboard** | Event only | Season standings |
| **Registration** | Open/invite-based | Automatic (league members) |

## 📐 Data Model

### Base Event Properties (Shared)

```csharp
public interface IEvent
{
    string Id { get; set; }
    string Name { get; set; }
    string? Description { get; set; }
    DateTime EventDate { get; set; }

    // Venue
    string? CourseId { get; set; }
    string? TeeId { get; set; }
    HolesPlayed HolesPlayed { get; set; }  // None, Nine, Front, Back, Eighteen

    // Tournament Settings
    ScoringFormat Format { get; set; }  // NOTE: Using ScoringFormat (existing enum)
    int TeamSize { get; set; }
    bool UseHandicaps { get; set; }

    // Status
    EventStatus Status { get; set; }
    bool IsLocked { get; set; }
}
```

### OneTimeEvent (Standalone)

```csharp
public class OneTimeEvent : BaseEntity, IEvent
{
    // IEvent properties
    public string Id { get; set; }
    public string Key { get; set; }  // URL-friendly slug
    public string Name { get; set; }
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    
    // Organizer
    public string OrganizerId { get; set; }
    public string? OrganizationName { get; set; }
    
    // Venue
    public string? CourseId { get; set; }
    public string? TeeId { get; set; }
    public HolesPlayed HolesPlayed { get; set; }
    
    // Tournament Settings
    public ScoringFormat Format { get; set; }  // NOTE: Using ScoringFormat (existing enum)
    public int TeamSize { get; set; }
    public bool UseHandicaps { get; set; }
    public int? MaxTeams { get; set; }
    
    // Access Control
    public EventAccessType AccessType { get; set; }  // Public, Private, InviteOnly
    public string? RegistrationCode { get; set; }
    public DateTime? RegistrationDeadline { get; set; }
    
    // Pricing
    public EventTier Tier { get; set; }  // Basic ($49), Premium ($99), Enterprise ($199)
    public decimal? EntryFee { get; set; }  // Optional per-team entry fee
    public bool IsPaid { get; set; }
    public DateTime? PaidAt { get; set; }
    
    // Status
    public EventStatus Status { get; set; }
    public bool IsLocked { get; set; }
    
    // Navigation
    public User Organizer { get; set; }
    public Course? Course { get; set; }
    public Tee? Tee { get; set; }
    public ICollection<OneTimeEventTeam> Teams { get; set; }
}
```

### SeasonEvent (League Event)

```csharp
public class SeasonEvent : BaseEntity, IEvent
{
    // IEvent properties
    public string Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    
    // League Context
    public string LeagueId { get; set; }
    public string SeasonId { get; set; }
    
    // Venue
    public string? CourseId { get; set; }
    public string? TeeId { get; set; }
    public HolesPlayed HolesPlayed { get; set; }
    
    // Tournament Settings
    public ScoringFormat Format { get; set; }  // NOTE: Using ScoringFormat (existing enum)
    public int TeamSize { get; set; }  // NEW: Added for unified system
    public bool UseHandicaps { get; set; }  // NEW: Added for unified system
    
    // Status
    public EventStatus Status { get; set; }
    public bool IsLocked { get; set; }
    
    // Navigation
    public League League { get; set; }
    public Season Season { get; set; }
    public Course? Course { get; set; }
    public Tee? Tee { get; set; }
    // Teams come from SeasonTeam (already exists)
}
```

### Enums

> **Note:** `ScoringFormat` already exists in the codebase. We'll enhance it with new values.

```csharp
public enum ScoringFormat
{
    // EXISTING VALUES (0-5)
    StrokePlay = 0,     // Traditional stroke play
    MatchPlay = 1,      // Head-to-head match play
    Stableford = 2,     // Points-based scoring
    TwoPoint = 3,       // Two-point system (existing)
    Scramble = 4,       // Best shot per hole (most popular)
    BestBall = 5,       // Best individual score per hole

    // NEW VALUES (6+) - To be added
    Chapman = 6,        // Chapman/Pinehurst format
    Shamble = 7,        // Scramble off tee, then individual
    Skins = 8,          // Skin game
    Nassau = 9,         // Front 9, Back 9, Total
    Vegas = 10          // Vegas scoring
}

public enum EventAccessType
{
    Public,             // Anyone can register
    Private,            // Requires registration code
    InviteOnly          // Organizer must invite
}

public enum EventStatus
{
    Draft,              // Being created
    Published,          // Open for registration
    RegistrationClosed, // No more registrations
    InProgress,         // Event is happening
    Completed,          // Event finished
    Cancelled           // Event cancelled
}

public enum EventTier
{
    Basic,              // $49 - Up to 50 teams
    Premium,            // $99 - Up to 100 teams, advanced features
    Enterprise          // $199 - Unlimited teams, white-label
}

public enum HolesPlayed
{
    // EXISTING VALUES
    None = 0,           // Not specified
    Nine = 9,           // 9 holes
    Eighteen = 18,      // 18 holes

    // NEW VALUES - To be added
    Front = 91,         // Front 9 only
    Back = 92           // Back 9 only
}
```

### OneTimeEventTeam

```csharp
public class OneTimeEventTeam : BaseEntity
{
    public string Id { get; set; }
    public string EventId { get; set; }
    public string TeamName { get; set; }
    public int TeamNumber { get; set; }  // For display order

    // Captain Info
    public string? CaptainUserId { get; set; }  // If registered user
    public string? CaptainName { get; set; }
    public string? CaptainEmail { get; set; }
    public string? CaptainPhone { get; set; }

    // Registration
    public DateTime RegisteredAt { get; set; }
    public bool IsCheckedIn { get; set; }
    public DateTime? CheckedInAt { get; set; }

    // Scoring
    public int? TotalScore { get; set; }
    public int? NetScore { get; set; }
    public int Position { get; set; }

    // Payment (if entry fee)
    public bool HasPaid { get; set; }
    public decimal? AmountPaid { get; set; }

    // Navigation
    public OneTimeEvent Event { get; set; }
    public User? CaptainUser { get; set; }
    public ICollection<OneTimeEventPlayer> Players { get; set; }
    public Round? Round { get; set; }  // Team's scorecard
}
```

### OneTimeEventPlayer

```csharp
public class OneTimeEventPlayer : BaseEntity
{
    public string Id { get; set; }
    public string TeamId { get; set; }
    public string EventId { get; set; }

    // Player Info
    public string? UserId { get; set; }  // If registered user
    public string PlayerName { get; set; }
    public string? Email { get; set; }
    public double? Handicap { get; set; }  // Manual entry

    // Team Role
    public int PlayerNumber { get; set; }  // 1-4 on team
    public bool IsCaptain { get; set; }

    // Navigation
    public OneTimeEventTeam Team { get; set; }
    public OneTimeEvent Event { get; set; }
    public User? User { get; set; }
}
```

## 🎯 Unified Scoring System

### Key Principle
**The same Round/RoundHole entities work for both event types**

### Round Entity (Already Exists - Enhanced)

```csharp
public class Round : BaseEntity
{
    public string Id { get; set; }
    public string GolferId { get; set; }

    // Event Context (ONE of these will be set)
    public string? LeagueId { get; set; }
    public string? SeasonEventId { get; set; }      // For league events
    public string? OneTimeEventId { get; set; }     // For one-time events
    public string? OneTimeEventTeamId { get; set; } // For team events

    // Venue
    public string CourseId { get; set; }
    public string TeeId { get; set; }
    public DateTime RoundDate { get; set; }
    public HolesPlayed HolesPlayed { get; set; }

    // Scoring
    public int? TotalScore { get; set; }
    public int? NetScore { get; set; }
    public double? HandicapUsed { get; set; }
    public bool IsComplete { get; set; }

    // Tournament Format
    public ScoringFormat? Format { get; set; }  // NEW: Override event format if needed
    public bool IsTeamRound { get; set; }  // NEW: True for scrambles, best ball, etc.

    // Navigation
    public Golfer Golfer { get; set; }
    public Course Course { get; set; }
    public Tee Tee { get; set; }
    public SeasonEvent? SeasonEvent { get; set; }
    public OneTimeEvent? OneTimeEvent { get; set; }
    public OneTimeEventTeam? OneTimeEventTeam { get; set; }
    public ICollection<RoundHole> Holes { get; set; }
}
```

### Scoring Strategies

Different tournament formats require different scoring logic:

#### Stroke Play (Individual)
- Each player records their own score
- Total strokes for the round
- Net score = Gross - Handicap strokes

#### Scramble (Team)
- Team selects best shot after each stroke
- All players play from that spot
- One score per hole for the team
- Team handicap = sum of individual handicaps × percentage (e.g., 20% for 4-man)

#### Best Ball (Team)
- Each player plays their own ball
- Team score = best individual score on each hole
- Can use individual handicaps

#### Match Play (Individual or Team)
- Hole-by-hole competition
- Win/Loss/Tie per hole
- Overall match result

## 🔄 Workflow Comparison

### One-Time Event Flow

1. **Create Event**
   - Organizer creates tournament
   - Sets format, team size, access type
   - Pays platform fee ($49-$199)
   - Event published

2. **Registration**
   - Teams register (public or invite)
   - Captain enters team info
   - Players added to team
   - Optional entry fee payment

3. **Event Day**
   - Teams check in
   - Scorecards assigned
   - Live scoring begins
   - Real-time leaderboard updates

4. **Completion**
   - Final scores submitted
   - Leaderboard finalized
   - Results exported
   - Winners announced

### League Event Flow

1. **Create Event**
   - Admin creates event in season
   - Sets format, course, date
   - No payment (included in subscription)
   - Event published

2. **Participation**
   - League members automatically eligible
   - Teams from season roster
   - No registration needed

3. **Event Day**
   - Same as one-time event
   - Scores tracked for season standings
   - Handicaps updated

4. **Completion**
   - Scores contribute to season stats
   - Season leaderboard updated
   - Handicaps recalculated

## 🎯 Shared Services

### IEventScoringService

```csharp
public interface IEventScoringService
{
    // Works for both event types
    Task<LeaderboardResponse> GetLeaderboardAsync(string eventId, EventType eventType);
    Task<ScoreEntryResponse> SubmitScoreAsync(string eventId, ScoreEntryRequest request);
    Task<RoundResponse> GetRoundAsync(string roundId);
    Task CalculateStandingsAsync(string eventId, EventType eventType);
}
```

### ITournamentFormatService

```csharp
public interface ITournamentFormatService
{
    // Calculate scores based on format
    Task<int> CalculateTeamScoreAsync(TournamentFormat format, List<int> playerScores);
    Task<int> CalculateNetScoreAsync(int grossScore, double handicap, TournamentFormat format);
    Task<List<int>> GetHandicapStrokesAsync(string teeId, double handicap);
}
```

## 📊 API Design

### Unified Endpoints

```
# Get leaderboard (works for both)
GET /api/v1/events/{eventId}/leaderboard?type={OneTime|Season}

# Submit score (works for both)
POST /api/v1/events/{eventId}/scores

# Get event details
GET /api/v1/events/one-time/{key}
GET /api/v1/events/season/{leagueKey}/{seasonKey}/{eventKey}
```

### One-Time Event Specific

```
POST   /api/v1/events/one-time
PUT    /api/v1/events/one-time/{key}
DELETE /api/v1/events/one-time/{key}
POST   /api/v1/events/one-time/{key}/publish
POST   /api/v1/events/one-time/{key}/teams/register
GET    /api/v1/events/one-time/{key}/teams
```

### Season Event Specific

```
POST   /api/v1/leagues/{leagueKey}/seasons/{seasonKey}/events
PUT    /api/v1/leagues/{leagueKey}/seasons/{seasonKey}/events/{eventKey}
DELETE /api/v1/leagues/{leagueKey}/seasons/{seasonKey}/events/{eventKey}
GET    /api/v1/leagues/{leagueKey}/seasons/{seasonKey}/events
```

## 🎨 UI Components (Shared)

### Reusable Components

- **EventCard**: Display event summary (works for both)
- **Leaderboard**: Real-time standings (works for both)
- **ScoreEntry**: Hole-by-hole input (works for both)
- **TeamRoster**: Display team members (works for both)
- **EventSettings**: Configure tournament format (works for both)

### One-Time Event Specific

- **EventCreationWizard**: Multi-step event creation
- **TeamRegistration**: Public registration form
- **PaymentCheckout**: Stripe integration
- **PublicEventPage**: Shareable event landing page

### League Event Specific

- **SeasonEventList**: Events within a season
- **SeasonStandings**: Season-long leaderboard
- **HandicapTracker**: Historical handicap changes

## 💡 Implementation Strategy

### Phase 1: Foundation (Week 1-2)

1. **Update Core Entities**
   - Add `TournamentFormat` enum
   - Add `EventStatus` enum
   - Update `Round` entity with event type support
   - Create `IEvent` interface

2. **Create One-Time Event Entities**
   - `OneTimeEvent`
   - `OneTimeEventTeam`
   - `OneTimeEventPlayer`
   - Database migrations

3. **Build Shared Scoring Service**
   - `EventScoringService` implementation
   - Support for Stroke Play and Scramble initially
   - Leaderboard calculation logic

### Phase 2: One-Time Events (Week 3-4)

4. **API Implementation**
   - Event CRUD endpoints
   - Team registration endpoints
   - Scoring endpoints
   - Leaderboard endpoints

5. **UI Implementation**
   - Event creation wizard
   - Public event page
   - Team registration form
   - Mobile scorecard

6. **Payment Integration**
   - Stripe checkout
   - Event tier pricing
   - Payment verification

### Phase 3: Integration (Week 5)

7. **Update Season Events**
   - Migrate to use `TournamentFormat`
   - Use shared scoring service
   - Unified leaderboard component

8. **Real-Time Features**
   - SignalR hub for live updates
   - Live leaderboard updates
   - Score notifications

9. **Testing & Polish**
   - End-to-end testing
   - Mobile optimization
   - Performance tuning

## ✅ Benefits of Unified System

### For Development

✅ **Code Reuse**: Scoring logic written once
✅ **Consistency**: Same UX for all events
✅ **Maintainability**: Single codebase to maintain
✅ **Flexibility**: Easy to add new tournament formats
✅ **Testability**: Shared services are easier to test

### For Users

✅ **Familiar Interface**: Same experience everywhere
✅ **Easy Transition**: One-time users can easily create leagues
✅ **Powerful Features**: All formats available for all events
✅ **Mobile Friendly**: Same scorecard for all event types

### For Business

✅ **Market Flexibility**: Support both casual and serious users
✅ **Upsell Path**: One-time → League conversion
✅ **Viral Growth**: Public events drive awareness
✅ **Revenue Streams**: Both subscription and pay-per-event

## 🎯 Success Metrics

### Technical Metrics

- Code reuse percentage (target: >70%)
- API response time (target: <200ms)
- Real-time update latency (target: <1s)
- Mobile performance score (target: >90)

### Business Metrics

- One-time event conversion rate (target: 5-10%)
- Average events per organizer (target: 3+)
- User satisfaction score (target: 4.5+/5)
- Platform fee collection rate (target: >95%)

## 📝 Implementation Notes

### Alignment with Existing Code

This architecture has been reviewed against the existing codebase:

✅ **Uses existing enums:** `ScoringFormat` and `HolesPlayed` (with enhancements)
✅ **Builds on existing entities:** `SeasonEvent`, `Round`, `RoundHole`
✅ **Respects multi-tenancy:** `ITenantEntity` for league events, `OrganizerId` for one-time events
✅ **Minimal breaking changes:** Additive changes only

### MVP Scope (Phase 1-3)

**Minimum Viable Product includes:**
1. Create one-time event (basic info)
2. Register teams (no payment required)
3. Enter scores (stroke play only)
4. View leaderboard (manual refresh)

**Deferred to later phases:**
- Payment integration (Phase 5)
- Real-time updates via SignalR (Phase 6)
- Advanced scoring formats (Phase 6)
- Tiered pricing (Phase 6)

### Key Decisions

1. **Enum Naming:** Using `ScoringFormat` (existing) instead of `TournamentFormat`
2. **Payment Timing:** Phase 5 (after core features work)
3. **Real-Time Features:** Phase 6 (enhancement, not MVP)
4. **MVP Scope:** Simple and focused on core functionality

## 📝 Next Steps

1. ✅ **Architecture Design** - This document
2. ✅ **Architecture Review** - Completed (see unified-event-system-review.md)
3. ⏳ **Phase 1: Foundation** - Update enums and entities
4. ⏳ **Phase 2: One-Time Event CRUD** - API implementation
5. ⏳ **Phase 3: Scoring System** - Shared scoring service
6. ⏳ **Phase 4: UI Implementation** - Event creation and scorecard
7. ⏳ **Phase 5: Payment Integration** - Stripe checkout
8. ⏳ **Phase 6: Enhancements** - SignalR, advanced formats, tiered pricing

---

**This unified architecture provides the foundation for a flexible, scalable golf tournament management system that serves both casual one-time events and serious league play.** 🏌️‍♂️⛳

**See also:**
- [Architecture Review](./unified-event-system-review.md) - Detailed review and recommendations
- [Golfer Architecture](./golfer-architecture.md) - Player management system
- [One-Time Events](./one-time-events.md) - Original one-time event design

