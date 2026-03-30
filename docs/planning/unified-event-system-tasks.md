# Unified Event System - Detailed Task Breakdown

## 📋 Overview

This document provides a detailed task breakdown for implementing the Unified Event System that supports both one-time tournaments and league events.

**Timeline:** 6-8 weeks  
**MVP Target:** Week 4 (Phases 1-3)  
**Full Feature Set:** Week 8 (All phases)

---

## 🎯 Phase 1: Foundation (Week 1-2)

**Goal:** Update core entities and enums to support unified event system

### Task 1.1: Update ScoringFormat Enum ⏳
**File:** `GolfManager/src/GolfManager.Core/Enums/ScoringFormat.cs`

**Changes:**
- Add `Chapman = 6` (Chapman/Pinehurst format)
- Add `Shamble = 7` (Scramble off tee, then individual)
- Add `Skins = 8` (Skin game)
- Add `Nassau = 9` (Front 9, Back 9, Total)
- Add `Vegas = 10` (Vegas scoring)

**Acceptance Criteria:**
- [ ] Enum compiles without errors
- [ ] XML documentation added for new values
- [ ] No breaking changes to existing values (0-5)

---

### Task 1.2: Update HolesPlayed Enum ⏳
**File:** `GolfManager/src/GolfManager.Core/Enums/HolesPlayed.cs`

**Changes:**
- Add `Front = 91` (Front 9 only)
- Add `Back = 92` (Back 9 only)

**Acceptance Criteria:**
- [ ] Enum compiles without errors
- [ ] XML documentation added for new values
- [ ] No breaking changes to existing values

---

### Task 1.3: Create EventStatus Enum ⏳
**File:** `GolfManager/src/GolfManager.Core/Enums/EventStatus.cs` (NEW)

**Content:**
```csharp
public enum EventStatus
{
    Draft = 0,              // Being created
    Published = 1,          // Open for registration
    RegistrationClosed = 2, // No more registrations
    InProgress = 3,         // Event is happening
    Completed = 4,          // Event finished
    Cancelled = 5           // Event cancelled
}
```

**Acceptance Criteria:**
- [ ] File created in correct location
- [ ] XML documentation added
- [ ] Follows existing enum patterns

---

### Task 1.4: Create EventAccessType Enum ⏳
**File:** `GolfManager/src/GolfManager.Core/Enums/EventAccessType.cs` (NEW)

**Content:**
```csharp
public enum EventAccessType
{
    Public = 0,      // Anyone can register
    Private = 1,     // Requires registration code
    InviteOnly = 2   // Organizer must invite
}
```

**Acceptance Criteria:**
- [ ] File created in correct location
- [ ] XML documentation added
- [ ] Follows existing enum patterns

---

### Task 1.5: Enhance SeasonEvent Entity ⏳
**File:** `GolfManager/src/GolfManager.Core/Entities/SeasonEvent.cs`

**Changes:**
- Add `public int TeamSize { get; set; } = 1;` (default to individual)
- Add `public bool UseHandicaps { get; set; } = true;` (default to true)
- Add `public EventStatus Status { get; set; } = EventStatus.Draft;`
- Add `public bool IsActive { get; set; } = true;` (for soft delete)

**Acceptance Criteria:**
- [ ] Properties added with correct types
- [ ] Default values set appropriately
- [ ] XML documentation added
- [ ] No breaking changes to existing code

---

### Task 1.6: Create OneTimeEvent Entity ⏳
**File:** `GolfManager/src/GolfManager.Core/Entities/OneTimeEvent.cs` (NEW)

**Content:** See architecture document for full entity definition

**Key Properties:**
- Id, Key, Name, Description, EventDate
- OrganizerId, OrganizationName
- CourseId, TeeId, HolesPlayed
- Format (ScoringFormat), TeamSize, UseHandicaps
- AccessType, RegistrationCode, RegistrationDeadline
- MaxTeams, Status, IsLocked
- Navigation properties

**Acceptance Criteria:**
- [ ] Entity created with all properties
- [ ] Inherits from BaseEntity
- [ ] XML documentation added
- [ ] Navigation properties defined

---

### Task 1.7: Create OneTimeEventTeam Entity ⏳
**File:** `GolfManager/src/GolfManager.Core/Entities/OneTimeEventTeam.cs` (NEW)

**Key Properties:**
- Id, EventId, TeamName, TeamNumber
- CaptainUserId, CaptainName, CaptainEmail, CaptainPhone
- RegisteredAt, IsCheckedIn, CheckedInAt
- TotalScore, NetScore, Position
- Navigation properties

**Acceptance Criteria:**
- [ ] Entity created with all properties
- [ ] Inherits from BaseEntity
- [ ] XML documentation added
- [ ] Navigation properties defined

---

### Task 1.8: Create OneTimeEventPlayer Entity ⏳
**File:** `GolfManager/src/GolfManager.Core/Entities/OneTimeEventPlayer.cs` (NEW)

**Key Properties:**
- Id, TeamId, EventId
- UserId, PlayerName, Email, Handicap
- PlayerNumber, IsCaptain
- Navigation properties

**Acceptance Criteria:**
- [ ] Entity created with all properties
- [ ] Inherits from BaseEntity
- [ ] XML documentation added
- [ ] Navigation properties defined

---

### Task 1.9: Enhance Round Entity ⏳
**File:** `GolfManager/src/GolfManager.Core/Entities/Round.cs`

**Changes:**
- Add `public string? OneTimeEventId { get; set; }`
- Add `public string? OneTimeEventTeamId { get; set; }`
- Add `public bool IsTeamRound { get; set; }`
- Add `public ScoringFormat? Format { get; set; }`
- Add navigation properties for OneTimeEvent and OneTimeEventTeam

**Acceptance Criteria:**
- [ ] Properties added as nullable where appropriate
- [ ] XML documentation added
- [ ] Navigation properties defined
- [ ] No breaking changes

---

### Task 1.10: Create Entity Configurations ⏳
**Files:** 
- `GolfManager/src/GolfManager.Data/Configurations/OneTimeEventConfiguration.cs` (NEW)
- `GolfManager/src/GolfManager.Data/Configurations/OneTimeEventTeamConfiguration.cs` (NEW)
- `GolfManager/src/GolfManager.Data/Configurations/OneTimeEventPlayerConfiguration.cs` (NEW)

**Changes:**
- Update `SeasonEventConfiguration.cs` for new properties
- Update `RoundConfiguration.cs` for new properties

**Acceptance Criteria:**
- [ ] All configurations created
- [ ] Fluent API properly configured
- [ ] Foreign keys defined
- [ ] Indexes added where appropriate
- [ ] Max lengths set for strings

---

### Task 1.11: Update DbContext ⏳
**File:** `GolfManager/src/GolfManager.Data/GolfManagerDbContext.cs`

**Changes:**
- Add `public DbSet<OneTimeEvent> OneTimeEvents { get; set; }`
- Add `public DbSet<OneTimeEventTeam> OneTimeEventTeams { get; set; }`
- Add `public DbSet<OneTimeEventPlayer> OneTimeEventPlayers { get; set; }`
- Apply configurations in OnModelCreating

**Acceptance Criteria:**
- [ ] DbSets added
- [ ] Configurations applied
- [ ] No compilation errors

---

### Task 1.12: Create Database Migration ⏳
**Command:** `dotnet ef migrations add UnifiedEventSystem -p GolfManager/src/GolfManager.Data -s GolfManager/src/GolfManager.Api`

**Acceptance Criteria:**
- [ ] Migration created successfully
- [ ] Up migration includes all new tables and columns
- [ ] Down migration properly removes changes
- [ ] Migration reviewed for correctness

---

### Task 1.13: Apply Migration ⏳
**Command:** `dotnet ef database update -p GolfManager/src/GolfManager.Data -s GolfManager/src/GolfManager.Api`

**Acceptance Criteria:**
- [ ] Migration applied successfully
- [ ] Database schema updated
- [ ] No errors during migration
- [ ] Can query new tables

---

### Task 1.14: Build and Test ⏳
**Commands:**
- `dotnet build GolfManager`
- `dotnet test GolfManager`

**Acceptance Criteria:**
- [ ] All projects build successfully
- [ ] All existing tests pass
- [ ] No new warnings introduced

---

## 📊 Phase 1 Summary

**Total Tasks:** 14  
**Estimated Time:** 1-2 weeks  
**Dependencies:** None (foundation work)

**Deliverables:**
- ✅ Enhanced enums (ScoringFormat, HolesPlayed, EventStatus, EventAccessType)
- ✅ Enhanced SeasonEvent entity
- ✅ New OneTimeEvent entities (Event, Team, Player)
- ✅ Enhanced Round entity
- ✅ Database migration applied
- ✅ All builds passing

---

## 🎯 Phase 2: One-Time Event CRUD (Week 3)

**Goal:** Implement API for creating and managing one-time events

### Task 2.1: Create DTOs ⏳
**Files:** (All in `GolfManager/src/GolfManager.Shared/DTOs/OneTimeEvent/`)
- `CreateOneTimeEventRequest.cs`
- `UpdateOneTimeEventRequest.cs`
- `OneTimeEventResponse.cs`
- `OneTimeEventTeamResponse.cs`
- `OneTimeEventPlayerResponse.cs`
- `RegisterTeamRequest.cs`

**Acceptance Criteria:**
- [ ] All DTOs created with proper validation attributes
- [ ] XML documentation added
- [ ] Follows existing DTO patterns

---

### Task 2.2: Create IOneTimeEventService Interface ⏳
**File:** `GolfManager/src/GolfManager.Services/Interfaces/IOneTimeEventService.cs`

**Methods:**
- `Task<OneTimeEventResponse> CreateEventAsync(CreateOneTimeEventRequest request, string organizerId)`
- `Task<OneTimeEventResponse> GetEventAsync(string eventKey)`
- `Task<OneTimeEventResponse> UpdateEventAsync(string eventKey, UpdateOneTimeEventRequest request, string userId)`
- `Task DeleteEventAsync(string eventKey, string userId)`
- `Task<List<OneTimeEventResponse>> GetUserEventsAsync(string userId)`
- `Task<List<OneTimeEventResponse>> GetPublicEventsAsync()`
- `Task PublishEventAsync(string eventKey, string userId)`

**Acceptance Criteria:**
- [ ] Interface created with all methods
- [ ] XML documentation added
- [ ] Return types use DTOs

---

### Task 2.3: Implement OneTimeEventService ⏳
**File:** `GolfManager/src/GolfManager.Services/OneTimeEventService.cs`

**Implementation:**
- CRUD operations for events
- Authorization checks (only organizer can edit/delete)
- Event publishing logic
- Public event listing

**Acceptance Criteria:**
- [ ] All interface methods implemented
- [ ] Authorization logic included
- [ ] Error handling implemented
- [ ] Logging added

---

### Task 2.4: Create Team Registration Service Methods ⏳
**File:** `GolfManager/src/GolfManager.Services/OneTimeEventService.cs` (extend)

**Methods:**
- `Task<OneTimeEventTeamResponse> RegisterTeamAsync(string eventKey, RegisterTeamRequest request)`
- `Task<List<OneTimeEventTeamResponse>> GetEventTeamsAsync(string eventKey)`
- `Task CheckInTeamAsync(string eventKey, string teamId, string userId)`

**Acceptance Criteria:**
- [ ] Team registration implemented
- [ ] Validation for max teams
- [ ] Check-in logic implemented

---

### Task 2.5: Create OneTimeEventsController ⏳
**File:** `GolfManager/src/GolfManager.Api/Controllers/OneTimeEventsController.cs`

**Endpoints:**
- `POST /api/v1/events/one-time` - Create event
- `GET /api/v1/events/one-time/{key}` - Get event
- `PUT /api/v1/events/one-time/{key}` - Update event
- `DELETE /api/v1/events/one-time/{key}` - Delete event
- `POST /api/v1/events/one-time/{key}/publish` - Publish event
- `GET /api/v1/events/one-time` - List public events
- `GET /api/v1/events/one-time/my-events` - List user's events
- `POST /api/v1/events/one-time/{key}/teams` - Register team
- `GET /api/v1/events/one-time/{key}/teams` - List teams
- `POST /api/v1/events/one-time/{key}/teams/{teamId}/check-in` - Check in team

**Acceptance Criteria:**
- [ ] All endpoints implemented
- [ ] Authorization applied where needed
- [ ] API responses wrapped in ApiResponse<T>
- [ ] Error handling implemented

---

### Task 2.6: Register Service in DI ⏳
**File:** `GolfManager/src/GolfManager.Api/Program.cs`

**Changes:**
- Add `builder.Services.AddScoped<IOneTimeEventService, OneTimeEventService>();`

**Acceptance Criteria:**
- [ ] Service registered
- [ ] No compilation errors

---

### Task 2.7: Create Integration Tests ⏳
**File:** `GolfManager/tests/GolfManager.IntegrationTests/OneTimeEventTests.cs` (NEW)

**Tests:**
- Create event
- Get event
- Update event
- Delete event
- Publish event
- Register team
- List teams
- Authorization tests

**Acceptance Criteria:**
- [ ] All tests created
- [ ] All tests pass
- [ ] Edge cases covered

---

## 📊 Phase 2 Summary

**Total Tasks:** 7
**Estimated Time:** 1 week
**Dependencies:** Phase 1 complete

**Deliverables:**
- ✅ Complete DTO set for one-time events
- ✅ IOneTimeEventService and implementation
- ✅ OneTimeEventsController with all endpoints
- ✅ Integration tests passing
- ✅ API ready for UI consumption

---

## 🎯 Phase 3: Scoring System (Week 4)

**Goal:** Implement shared scoring service that works for both event types

### Task 3.1: Create Scoring DTOs ⏳
**Files:** (In `GolfManager/src/GolfManager.Shared/DTOs/Scoring/`)
- `ScoreEntryRequest.cs`
- `ScoreEntryResponse.cs`
- `LeaderboardResponse.cs`
- `LeaderboardEntryResponse.cs`

**Acceptance Criteria:**
- [ ] All DTOs created
- [ ] Validation attributes added
- [ ] XML documentation added

---

### Task 3.2: Create IEventScoringService Interface ⏳
**File:** `GolfManager/src/GolfManager.Services/Interfaces/IEventScoringService.cs`

**Methods:**
- `Task<ScoreEntryResponse> SubmitScoreAsync(ScoreEntryRequest request, string userId)`
- `Task<LeaderboardResponse> GetLeaderboardAsync(string eventId, string eventType)`
- `Task<RoundResponse> GetRoundAsync(string roundId)`
- `Task CalculateStandingsAsync(string eventId, string eventType)`

**Acceptance Criteria:**
- [ ] Interface created
- [ ] XML documentation added
- [ ] Supports both event types

---

### Task 3.3: Implement EventScoringService ⏳
**File:** `GolfManager/src/GolfManager.Services/EventScoringService.cs`

**Implementation:**
- Score submission for individual rounds
- Score submission for team rounds (scramble)
- Leaderboard calculation
- Standings calculation
- Support for both OneTimeEvent and SeasonEvent

**Acceptance Criteria:**
- [ ] All methods implemented
- [ ] Stroke play scoring works
- [ ] Scramble scoring works
- [ ] Leaderboard calculation correct
- [ ] Works for both event types

---

### Task 3.4: Create Scoring Endpoints ⏳
**File:** `GolfManager/src/GolfManager.Api/Controllers/ScoringController.cs` (NEW)

**Endpoints:**
- `POST /api/v1/scoring/submit` - Submit score
- `GET /api/v1/scoring/leaderboard/{eventId}?type={OneTime|Season}` - Get leaderboard
- `GET /api/v1/scoring/round/{roundId}` - Get round details

**Acceptance Criteria:**
- [ ] All endpoints implemented
- [ ] Authorization applied
- [ ] Works for both event types

---

### Task 3.5: Create Scoring Integration Tests ⏳
**File:** `GolfManager/tests/GolfManager.IntegrationTests/ScoringTests.cs` (NEW)

**Tests:**
- Submit individual score
- Submit team score (scramble)
- Get leaderboard for one-time event
- Get leaderboard for season event
- Calculate standings

**Acceptance Criteria:**
- [ ] All tests created
- [ ] All tests pass
- [ ] Both event types tested

---

## 📊 Phase 3 Summary

**Total Tasks:** 5
**Estimated Time:** 1 week
**Dependencies:** Phase 2 complete

**Deliverables:**
- ✅ Unified scoring service
- ✅ Scoring API endpoints
- ✅ Leaderboard calculation
- ✅ Works for both event types
- ✅ Integration tests passing

**🎉 MVP COMPLETE at end of Phase 3!**

---

## 🎯 Phase 4: UI Implementation (Week 5-6)

**Goal:** Build user-facing UI for creating events and entering scores

### Task 4.1: Create OneTimeEventService (Web) ⏳
**File:** `GolfManager/src/GolfManager.Web/Services/IOneTimeEventService.cs`

**Methods:** Mirror API endpoints

**Acceptance Criteria:**
- [ ] Service created
- [ ] All API methods wrapped
- [ ] Registered in DI

---

### Task 4.2: Create Event Creation Page ⏳
**File:** `GolfManager/src/GolfManager.Web/Pages/CreateEvent.razor`

**Features:**
- Multi-step wizard
- Event details form
- Format selection
- Course/tee selection
- Access control settings

**Acceptance Criteria:**
- [ ] Page created
- [ ] Form validation works
- [ ] Can create events
- [ ] Professional UI

---

### Task 4.3: Create Public Event Page ⏳
**File:** `GolfManager/src/GolfManager.Web/Pages/EventDetail.razor`

**Features:**
- Event details display
- Team registration form
- Current registrations list
- Leaderboard (during event)

**Acceptance Criteria:**
- [ ] Page created
- [ ] Registration works
- [ ] Leaderboard displays
- [ ] Responsive design

---

### Task 4.4: Create Mobile Scorecard ⏳
**File:** `GolfManager/src/GolfManager.Web/Pages/Scorecard.razor`

**Features:**
- Hole-by-hole score entry
- Works for individual and team
- Works for both event types
- Mobile-first design

**Acceptance Criteria:**
- [ ] Page created
- [ ] Score entry works
- [ ] Mobile-friendly
- [ ] Clean UX

---

## 📊 Phase 4 Summary

**Total Tasks:** 4
**Estimated Time:** 2 weeks
**Dependencies:** Phase 3 complete

**Deliverables:**
- ✅ Event creation UI
- ✅ Public event page
- ✅ Mobile scorecard
- ✅ Professional, responsive design

---

## 🎯 Phase 5: Payment Integration (Week 7)

**Goal:** Integrate Stripe for event payments

### Task 5.1: Install Stripe SDK ⏳
### Task 5.2: Create Payment Service ⏳
### Task 5.3: Create Checkout Flow ⏳
### Task 5.4: Verify Payment Webhook ⏳

**Deliverables:**
- ✅ Stripe integration
- ✅ Payment checkout
- ✅ Event activation after payment

---

## 🎯 Phase 6: Enhancements (Week 8+)

**Goal:** Add advanced features

### Task 6.1: SignalR Integration ⏳
### Task 6.2: Advanced Scoring Formats ⏳
### Task 6.3: Tiered Pricing ⏳

**Deliverables:**
- ✅ Real-time leaderboards
- ✅ Best Ball, Match Play, Stableford
- ✅ Basic/Premium/Enterprise tiers

---

## 📈 Progress Tracking

**Phase 1:** ⏳ In Progress (0/14 tasks complete)
**Phase 2:** ⏳ Not Started (0/7 tasks complete)
**Phase 3:** ⏳ Not Started (0/5 tasks complete)
**Phase 4:** ⏳ Not Started (0/4 tasks complete)
**Phase 5:** ⏳ Not Started
**Phase 6:** ⏳ Not Started

**Overall Progress:** 0/30 MVP tasks complete (0%)

---

**Next Action:** Start Task 1.1 (Update ScoringFormat Enum)

