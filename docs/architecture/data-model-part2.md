# GolfManager v2 - Data Model (Part 2)

## Scoring Entities (Continued)

#### RoundHole
```csharp
public class RoundHole
{
    public string Id { get; set; }
    public string RoundId { get; set; }
    public string LeagueId { get; set; }
    public int HoleNumber { get; set; }
    
    // Hole reference data (denormalized for performance)
    public int Par { get; set; }
    public int Handicap { get; set; }
    public int Yardage { get; set; }
    
    // Score data
    public int? RawScore { get; set; }
    public int? Putts { get; set; }
    public int? Chips { get; set; }
    public int? PenaltyStrokes { get; set; }
    public int? SandShots { get; set; }
    public TeeShotPosition? TeeShotPosition { get; set; }
    
    // Club tracking
    public string? TeeClubId { get; set; }
    public int? TeeDistance { get; set; }
    
    // Navigation
    public Round Round { get; set; }
}
```

#### Scorecard (Group scoring session)
```csharp
public class Scorecard
{
    public string Id { get; set; }
    public string LeagueId { get; set; }
    public string OwnerGolferId { get; set; }
    public string? SeasonEventId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsComplete { get; set; }
    
    // Navigation
    public Golfer Owner { get; set; }
    public SeasonEvent? SeasonEvent { get; set; }
    public ICollection<Round> Rounds { get; set; }
}
```

#### SeasonEventGolfer (Golfer participation in an event)
```csharp
public class SeasonEventGolfer
{
    public string Id { get; set; }
    public string SeasonEventId { get; set; }
    public string LeagueId { get; set; }
    public string LeagueGolferId { get; set; }         // References LeagueGolfer
    public string GolferId { get; set; }               // Denormalized for queries
    public string? RoundId { get; set; }
    public string? TeamId { get; set; }
    public double? EventHandicap { get; set; }         // Handicap at time of event
    public int? RawScore { get; set; }                 // Computed from round
    public int? NetScore { get; set; }                 // Computed
    public double? Points { get; set; }                // Individual points
    public int? Position { get; set; }                 // Finishing position
    public DateTime? CheckInTime { get; set; }

    // Navigation
    public SeasonEvent SeasonEvent { get; set; }
    public LeagueGolfer LeagueGolfer { get; set; }
    public Golfer Golfer { get; set; }
    public Round? Round { get; set; }
    public SeasonTeam? Team { get; set; }
}
```

#### SeasonEventMatch (Team match within an event)
```csharp
public class SeasonEventMatch
{
    public string Id { get; set; }
    public string SeasonEventId { get; set; }
    public string LeagueId { get; set; }
    public string? ScorecardId { get; set; }
    public string? HomeTeamId { get; set; }
    public string? AwayTeamId { get; set; }
    public double? HomePoints { get; set; }
    public double? AwayPoints { get; set; }
    public int? StartingHole { get; set; }
    public int? StartingFlight { get; set; }
    public bool IsComplete { get; set; }
    
    // Navigation
    public SeasonEvent SeasonEvent { get; set; }
    public Scorecard? Scorecard { get; set; }
    public SeasonTeam? HomeTeam { get; set; }
    public SeasonTeam? AwayTeam { get; set; }
}
```

### Equipment Tracking (Global - Not League-Specific)

#### GolferClub
```csharp
public class GolferClub
{
    public string Id { get; set; }
    public string GolferId { get; set; }               // Global golfer
    public string Key { get; set; }                    // e.g., "driver", "3w"
    public string Name { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public int? AverageDistance { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Golfer Golfer { get; set; }
}
```

### Authentication & Security

#### RefreshToken
```csharp
public class RefreshToken
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string Token { get; set; }                  // Hashed
    public string DeviceId { get; set; }
    public string? DeviceName { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedReason { get; set; }
    public bool IsActive => RevokedAt == null && ExpiresAt > DateTime.UtcNow;
    
    // Navigation
    public User User { get; set; }
}
```

## 🔍 Indexes Strategy

### Critical Indexes

```sql
-- User lookups
CREATE UNIQUE INDEX IX_User_Email ON Users(Email);
CREATE INDEX IX_User_IsActive ON Users(IsActive);

-- League isolation
CREATE INDEX IX_League_Key ON Leagues(Key);
CREATE INDEX IX_UserLeague_UserId_LeagueId ON UserLeagues(UserId, LeagueId);

-- Tenant isolation (on all tenant-specific tables)
CREATE INDEX IX_{Table}_LeagueId ON {Table}(LeagueId);

-- Season queries
CREATE INDEX IX_Season_LeagueId_IsActive ON Seasons(LeagueId, IsActive);
CREATE INDEX IX_SeasonEvent_SeasonId_EventDate ON SeasonEvents(SeasonId, EventDate);

-- Scoring queries
CREATE INDEX IX_Round_GolferId_PlayedDate ON Rounds(GolferId, PlayedDate);
CREATE INDEX IX_RoundHole_RoundId_HoleNumber ON RoundHoles(RoundId, HoleNumber);

-- Leaderboard queries
CREATE INDEX IX_SeasonGolfer_SeasonId_TotalPoints ON SeasonGolfers(SeasonId, TotalPoints DESC);
CREATE INDEX IX_SeasonTeam_SeasonId_TotalPoints ON SeasonTeams(SeasonId, TotalPoints DESC);
```

## 🎯 Computed Columns & Triggers

### Computed Values
- Round.TotalScore = SUM(RoundHoles.RawScore)
- SeasonGolfer.TotalEvents = COUNT(SeasonEventGolfers)
- SeasonGolfer.AverageScore = AVG(SeasonEventGolfers.RawScore)
- SeasonTeam.TotalPoints = SUM(SeasonEventMatches points)

### Update Triggers
- On RoundHole change → Update Round.TotalScore
- On SeasonEventGolfer change → Update SeasonGolfer stats
- On SeasonEventMatch change → Update SeasonTeam standings

## 📊 Key Relationships

```
# User & Golfer (Global)
User (1) ←→ (0..1) Golfer                 # User OPTIONALLY has golfer profile
                                          # (admins, scorekeepers may not be golfers)
Golfer (1) ←→ (1) User                    # Every golfer MUST have a user
Golfer (1) ←→ (N) GolferClub              # Golfer's equipment (global)
Golfer (1) ←→ (N) Round                   # All rounds (league + casual)

# Multi-Tenancy
User (1) ←→ (N) UserLeague (N) ←→ (1) League
Golfer (1) ←→ (N) LeagueGolfer (N) ←→ (1) League
LeagueGolfer = Golfer's profile in a specific league

# League Structure
League (1) ←→ (N) Season
Season (1) ←→ (N) SeasonEvent
Season (1) ←→ (N) SeasonTeam
Season (1) ←→ (N) SeasonGolfer

# Season Participation
LeagueGolfer (1) ←→ (N) SeasonGolfer      # Golfer in seasons
SeasonEvent (1) ←→ (N) SeasonEventGolfer
SeasonEvent (1) ←→ (N) SeasonEventMatch

# Scoring
Round (1) ←→ (N) RoundHole
Round (N) ←→ (1) Golfer (global)
Round (N) ←→ (0..1) LeagueGolfer (if league round)

# Courses (Global, shared)
Course (1) ←→ (N) Tee
Tee (1) ←→ (N) HoleTee
Course (1) ←→ (N) Hole
```

## 🔐 Row-Level Security

All tenant-specific queries must include LeagueId filter:
```csharp
// Example query filter
modelBuilder.Entity<Season>()
    .HasQueryFilter(s => s.LeagueId == _currentLeagueId);
```

## 📝 Audit Fields Pattern

All entities should include:
```csharp
public DateTime CreatedAt { get; set; }
public string? CreatedBy { get; set; }
public DateTime? UpdatedAt { get; set; }
public string? UpdatedBy { get; set; }
public DateTime? DeletedAt { get; set; }  // Soft delete
public string? DeletedBy { get; set; }
```

