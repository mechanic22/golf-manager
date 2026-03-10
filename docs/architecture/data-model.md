# GolfManager v2 - Data Model Design

## 🎯 Design Principles

1. **Multi-Tenancy**: League-based isolation with shared users
2. **Audit Trail**: Track all changes with timestamps and user info
3. **Soft Deletes**: Preserve historical data
4. **Performance**: Strategic indexing and computed columns
5. **Flexibility**: Support multiple scoring systems and configurations

## 📊 Core Entities

### User Management

#### User (Global Entity - Not Tenant-Specific)
```csharp
public class User
{
    public string Id { get; set; }                    // Primary key
    public string Email { get; set; }                 // Unique, login identifier
    public string PasswordHash { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool IsGlobalAdmin { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Navigation
    public Golfer? Golfer { get; set; }               // Optional - user may not be a golfer
    public ICollection<UserLeague> UserLeagues { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; }
}
```

#### Golfer (Global Player Profile)
```csharp
public class Golfer
{
    public string Id { get; set; }                    // Primary key (same as UserId)
    public string UserId { get; set; }                // One-to-one with User
    public string DisplayName { get; set; }           // Primary display name
    public string? Nickname { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? PhoneNumber { get; set; }
    public string? HomeCity { get; set; }
    public string? HomeState { get; set; }

    // Global Handicap (across all leagues)
    public double? GlobalHandicap { get; set; }
    public DateTime? GlobalHandicapUpdatedAt { get; set; }

    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public User User { get; set; }                    // Required - every golfer has a user
    public ICollection<LeagueGolfer> LeagueGolfers { get; set; }
    public ICollection<Round> Rounds { get; set; }
    public ICollection<GolferClub> Clubs { get; set; }
}
```

**Note**: User can exist without Golfer (admins, scorekeepers), but Golfer always requires User.

#### LeagueGolfer (Golfer's League Membership & League-Specific Profile)
```csharp
public class LeagueGolfer
{
    public string Id { get; set; }
    public string LeagueId { get; set; }
    public string GolferId { get; set; }
    public string UserId { get; set; }                // Denormalized for queries

    // League-specific profile (can override global)
    public string? LeagueDisplayName { get; set; }    // Override display name for this league
    public string? LeagueNickname { get; set; }
    public string? LeagueAvatarUrl { get; set; }

    // League-specific handicap
    public double? LeagueHandicap { get; set; }
    public DateTime? LeagueHandicapUpdatedAt { get; set; }

    // Membership info
    public bool IsLeagueAdmin { get; set; }
    public DateTime JoinedAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LeftAt { get; set; }

    // Navigation
    public League League { get; set; }
    public Golfer Golfer { get; set; }
    public User User { get; set; }
    public ICollection<SeasonGolfer> SeasonGolfers { get; set; }
}
```

#### UserLeague (User-League Membership - Simplified)
```csharp
public class UserLeague
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string LeagueId { get; set; }
    public string LeagueGolferId { get; set; }        // Link to LeagueGolfer
    public bool IsLeagueAdmin { get; set; }
    public DateTime JoinedAt { get; set; }
    public bool IsActive { get; set; }

    // Navigation
    public User User { get; set; }
    public League League { get; set; }
    public LeagueGolfer LeagueGolfer { get; set; }
}
```

### League & Tenant

#### League (Tenant Root)
```csharp
public class League
{
    public string Id { get; set; }                    // Primary key
    public string Key { get; set; }                   // URL-friendly slug (unique)
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? ActiveSeasonId { get; set; }

    // Custom Domain Support
    public string? CustomDomain { get; set; }         // e.g., "digikeygolf.com"
    public bool UseCustomDomain { get; set; }
    public string? CustomDomainVerificationToken { get; set; }
    public DateTime? CustomDomainVerifiedAt { get; set; }

    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation
    public ICollection<Season> Seasons { get; set; }
    public ICollection<LeagueGolfer> LeagueGolfers { get; set; }
    public ICollection<UserLeague> UserLeagues { get; set; }
}
```

### Season Management

#### Season
```csharp
public class Season
{
    public string Id { get; set; }
    public string LeagueId { get; set; }              // Tenant isolation
    public string Key { get; set; }                   // URL-friendly (e.g., "2024")
    public string Name { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsLocked { get; set; }                // Prevent changes
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Navigation
    public League League { get; set; }
    public SeasonSettings Settings { get; set; }
    public ICollection<SeasonEvent> Events { get; set; }
    public ICollection<SeasonTeam> Teams { get; set; }
    public ICollection<SeasonGolfer> Golfers { get; set; }
}
```

#### SeasonSettings
```csharp
public class SeasonSettings
{
    public string Id { get; set; }
    public string SeasonId { get; set; }
    public string LeagueId { get; set; }
    
    // Handicap Settings
    public HandicapType HandicapType { get; set; }
    public int? MaxHandicap { get; set; }
    public MaxScoreForHandicap MaxScoreForHandicap { get; set; }
    
    // Scoring Settings
    public IndividualScoringType IndividualScoringType { get; set; }
    public TeamScoringType TeamScoringType { get; set; }
    public MissingPlayerType MissingPlayerType { get; set; }
    public MissingTeamType MissingTeamType { get; set; }
    
    // Defaults
    public string? DefaultCourseId { get; set; }
    public TimeOnly? DefaultStartTime { get; set; }
    
    // Navigation
    public Season Season { get; set; }
}
```

#### SeasonEvent
```csharp
public class SeasonEvent
{
    public string Id { get; set; }
    public string SeasonId { get; set; }
    public string LeagueId { get; set; }
    public DateTime EventDate { get; set; }
    public string? CourseId { get; set; }
    public string? TeeId { get; set; }
    public HolesPlayed HolesPlayed { get; set; }
    public SeasonEventType EventType { get; set; }
    public string? Notes { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    
    // Navigation
    public Season Season { get; set; }
    public Course? Course { get; set; }
    public Tee? Tee { get; set; }
    public ICollection<SeasonEventGolfer> EventGolfers { get; set; }
    public ICollection<SeasonEventMatch> Matches { get; set; }
}
```

#### SeasonGolfer (Golfer participation in a season)
```csharp
public class SeasonGolfer
{
    public string Id { get; set; }
    public string SeasonId { get; set; }
    public string LeagueId { get; set; }
    public string LeagueGolferId { get; set; }         // References LeagueGolfer
    public string GolferId { get; set; }               // Denormalized for queries
    public string? TeamId { get; set; }
    public double? SeasonHandicap { get; set; }        // Season-specific handicap
    public int? TotalEvents { get; set; }              // Computed
    public double? AverageScore { get; set; }          // Computed
    public double? TotalPoints { get; set; }           // Computed
    public DateTime JoinedAt { get; set; }

    // Navigation
    public Season Season { get; set; }
    public LeagueGolfer LeagueGolfer { get; set; }
    public Golfer Golfer { get; set; }
    public SeasonTeam? Team { get; set; }
}
```

#### SeasonTeam
```csharp
public class SeasonTeam
{
    public string Id { get; set; }
    public string SeasonId { get; set; }
    public string LeagueId { get; set; }
    public string Name { get; set; }
    public string? AvatarUrl { get; set; }
    public double? TotalPoints { get; set; }           // Computed
    public int? Wins { get; set; }                     // Computed
    public int? Losses { get; set; }                   // Computed
    public int? Ties { get; set; }                     // Computed
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Season Season { get; set; }
    public ICollection<SeasonGolfer> Members { get; set; }
}
```

### Course Management (Global - Not Tenant-Specific)

#### Course
```csharp
public class Course
{
    public string Id { get; set; }
    public string Key { get; set; }                    // URL-friendly slug
    public string Name { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Website { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int HoleCount { get; set; }                 // 9 or 18
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public ICollection<Tee> Tees { get; set; }
    public ICollection<Hole> Holes { get; set; }
}
```

#### Tee
```csharp
public class Tee
{
    public string Id { get; set; }
    public string CourseId { get; set; }
    public string Name { get; set; }                   // e.g., "Blue", "White"
    public string ColorCode { get; set; }              // Hex color
    public double RatingFront { get; set; }
    public double RatingBack { get; set; }
    public int SlopeFront { get; set; }
    public int SlopeBack { get; set; }
    public int YardsFront { get; set; }
    public int YardsBack { get; set; }
    public int ParFront { get; set; }
    public int ParBack { get; set; }

    // Navigation
    public Course Course { get; set; }
    public ICollection<HoleTee> HoleTees { get; set; }
}
```

#### Hole
```csharp
public class Hole
{
    public string Id { get; set; }
    public string CourseId { get; set; }
    public int HoleNumber { get; set; }                // 1-18
    public string? Name { get; set; }
    public string? Description { get; set; }

    // GPS Coordinates for Mobile App
    public double? TeeBoxLatitude { get; set; }        // Tee box location
    public double? TeeBoxLongitude { get; set; }
    public double? GreenLatitude { get; set; }         // Green center location
    public double? GreenLongitude { get; set; }
    public double? FairwayLatitude { get; set; }       // Fairway landing zone (optional)
    public double? FairwayLongitude { get; set; }

    // GPS Geofence for auto-detection
    public double? GeofenceRadius { get; set; }        // Meters - for detecting when golfer is on this hole

    // Hole characteristics
    public string? Dogleg { get; set; }                // "Left", "Right", "None"
    public string? HazardNotes { get; set; }           // Water, bunkers, etc.

    // Navigation
    public Course Course { get; set; }
    public ICollection<HoleTee> HoleTees { get; set; }
}
```

#### HoleTee (Hole details per tee)
```csharp
public class HoleTee
{
    public string Id { get; set; }
    public string TeeId { get; set; }
    public string HoleId { get; set; }
    public int HoleNumber { get; set; }
    public int Par { get; set; }
    public int Yardage { get; set; }
    public int Handicap { get; set; }                  // Stroke index

    // GPS for tee-specific locations (if tees are in different locations)
    public double? TeeBoxLatitude { get; set; }        // Override hole's tee box location
    public double? TeeBoxLongitude { get; set; }

    // Navigation
    public Tee Tee { get; set; }
    public Hole Hole { get; set; }
}
```

### Scoring & Rounds

#### Round
```csharp
public class Round
{
    public string Id { get; set; }
    public string GolferId { get; set; }              // Global golfer ID
    public string? LeagueId { get; set; }             // Null if casual round
    public string? LeagueGolferId { get; set; }       // Null if casual round
    public string CourseId { get; set; }
    public string TeeId { get; set; }
    public string? ScorecardId { get; set; }
    public string? SeasonEventId { get; set; }
    public DateTime PlayedDate { get; set; }
    public int? TotalScore { get; set; }               // Computed
    public bool IsComplete { get; set; }
    public bool IsLeagueRound { get; set; }            // True if part of league play
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Golfer Golfer { get; set; }
    public LeagueGolfer? LeagueGolfer { get; set; }
    public Course Course { get; set; }
    public Tee Tee { get; set; }
    public Scorecard? Scorecard { get; set; }
    public ICollection<RoundHole> Holes { get; set; }
}
```

See [data-model-part2.md](./data-model-part2.md) for remaining entities.

