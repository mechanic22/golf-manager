# One-Time Tournament Events

## 🎯 Overview

GolfManager v2 supports **one-time tournament events** - standalone tournaments that don't require a league or season. Perfect for charity scrambles, corporate outings, or casual tournaments.

## 💡 Concept

**Use Case**: A user wants to host a one-time golf tournament (scramble, best ball, etc.) without creating a full league.

**Features**:
- Pay-per-event model (e.g., $49 per tournament)
- Real-time scoring and leaderboards
- Team-based formats (scrambles, best ball)
- Public or private tournaments
- Custom tournament settings
- Live updates via SignalR
- No league/season required

## 🆚 One-Time Event vs League Event

| Feature | One-Time Event | League Event |
|---------|---------------|--------------|
| **Requires League** | ❌ No | ✅ Yes |
| **Requires Season** | ❌ No | ✅ Yes |
| **Cost Model** | Pay per event ($49) | Included in subscription |
| **Duration** | Single day/event | Ongoing season |
| **Participants** | Anyone with link | League members only |
| **Handicaps** | Manual entry or none | Tracked over time |
| **Teams** | Event-specific | Season-long |
| **Leaderboard** | Event only | Season standings |

## 📊 Data Model

### OneTimeEvent (Standalone Tournament)

```csharp
public class OneTimeEvent
{
    public string Id { get; set; }
    public string Key { get; set; }                     // URL-friendly slug
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    
    // Organizer
    public string OrganizerId { get; set; }             // User who created it
    public string? OrganizationName { get; set; }       // e.g., "Acme Corp Charity"
    
    // Event Details
    public DateTime EventDate { get; set; }
    public string? CourseId { get; set; }
    public string? TeeId { get; set; }
    public HolesPlayed HolesPlayed { get; set; }        // 9 or 18
    
    // Tournament Settings
    public TournamentFormat Format { get; set; }        // Scramble, BestBall, Stroke, etc.
    public int TeamSize { get; set; }                   // 2, 4, etc.
    public bool UseHandicaps { get; set; }
    public int? MaxTeams { get; set; }
    public decimal? EntryFee { get; set; }              // Optional per-team entry fee
    
    // Access Control
    public EventAccessType AccessType { get; set; }     // Public, Private, InviteOnly
    public string? RegistrationCode { get; set; }       // For private events
    public DateTime? RegistrationDeadline { get; set; }
    
    // Status
    public EventStatus Status { get; set; }             // Draft, Open, InProgress, Completed
    public bool IsPublished { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    
    // Payment (to GolfManager)
    public decimal PlatformFee { get; set; }            // e.g., $49
    public bool IsPaid { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? StripePaymentIntentId { get; set; }
    
    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    // Navigation
    public User Organizer { get; set; }
    public Course? Course { get; set; }
    public Tee? Tee { get; set; }
    public ICollection<OneTimeEventTeam> Teams { get; set; }
    public ICollection<OneTimeEventHoleGame> HoleGames { get; set; }
}

public enum TournamentFormat
{
    Scramble,           // Team scramble (best shot)
    BestBall,           // Best ball per hole
    StrokePlay,         // Individual stroke play
    MatchPlay,          // Match play
    Chapman,            // Chapman/Pinehurst format
    Shamble,            // Scramble off tee, then individual
    TwoManScramble,
    FourManScramble
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
    Open,               // Registration open
    RegistrationClosed, // No more registrations
    InProgress,         // Tournament in progress
    Completed,          // Tournament finished
    Cancelled
}
```

### OneTimeEventTeam

```csharp
public class OneTimeEventTeam
{
    public string Id { get; set; }
    public string EventId { get; set; }
    public string TeamName { get; set; }
    public int TeamNumber { get; set; }                 // 1, 2, 3, etc.
    public int? StartingHole { get; set; }              // Shotgun start
    public DateTime? TeeTime { get; set; }
    
    // Scoring
    public int? TotalScore { get; set; }                // Computed
    public int? TotalNet { get; set; }                  // With handicap
    public int? Position { get; set; }                  // Final standing
    
    // Status
    public bool IsCheckedIn { get; set; }
    public DateTime? CheckInTime { get; set; }
    public bool IsComplete { get; set; }
    
    // Contact (for organizer)
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    
    // Navigation
    public OneTimeEvent Event { get; set; }
    public ICollection<OneTimeEventPlayer> Players { get; set; }
    public ICollection<OneTimeEventScore> Scores { get; set; }
}
```

### OneTimeEventPlayer

```csharp
public class OneTimeEventPlayer
{
    public string Id { get; set; }
    public string TeamId { get; set; }
    public string EventId { get; set; }
    
    // Player Info
    public string? UserId { get; set; }                 // If registered user
    public string PlayerName { get; set; }
    public string? Email { get; set; }
    public double? Handicap { get; set; }               // Manual entry
    
    // Team Role
    public int PlayerNumber { get; set; }               // 1-4 on team
    public bool IsCaptain { get; set; }
    
    // Navigation
    public OneTimeEventTeam Team { get; set; }
    public User? User { get; set; }
}
```

### OneTimeEventScore (Hole-by-Hole)

```csharp
public class OneTimeEventScore
{
    public string Id { get; set; }
    public string EventId { get; set; }
    public string TeamId { get; set; }
    public int HoleNumber { get; set; }
    
    // Hole reference data
    public int Par { get; set; }
    public int Handicap { get; set; }
    public int Yardage { get; set; }
    
    // Score
    public int? RawScore { get; set; }                  // Team score for hole
    public int? NetScore { get; set; }                  // With handicap
    public int? Putts { get; set; }
    
    // Individual scores (for best ball, etc.)
    public string? IndividualScoresJson { get; set; }   // JSON: {player1: 4, player2: 5, ...}
    
    // Timestamp
    public DateTime? ScoredAt { get; set; }
    public string? ScoredBy { get; set; }               // User ID who entered
    
    // Navigation
    public OneTimeEvent Event { get; set; }
    public OneTimeEventTeam Team { get; set; }
}
```

### OneTimeEventHoleGame (Side Games)

```csharp
public class OneTimeEventHoleGame
{
    public string Id { get; set; }
    public string EventId { get; set; }
    public string Name { get; set; }                    // "Closest to Pin", "Longest Drive"
    public HoleGameType Type { get; set; }
    public int? HoleNumber { get; set; }                // Specific hole or null for all
    public string? Description { get; set; }
    public decimal? Prize { get; set; }
    
    // Winner
    public string? WinningTeamId { get; set; }
    public string? WinningPlayerId { get; set; }
    public string? WinningValue { get; set; }           // e.g., "3 feet 2 inches"
    
    // Navigation
    public OneTimeEvent Event { get; set; }
    public OneTimeEventTeam? WinningTeam { get; set; }
}

public enum HoleGameType
{
    ClosestToPin,
    LongestDrive,
    LongestPutt,
    SkinGame,
    Custom
}
```

## 🔄 User Flow

### Organizer Creates Tournament

1. **Create Event**
   - User clicks "Host a Tournament"
   - Enters event details (name, date, course, format)
   - Selects tournament format (scramble, best ball, etc.)
   - Sets team size and max teams
   - Chooses access type (public/private)

2. **Configure Settings**
   - Add hole games (closest to pin, longest drive)
   - Set entry fees (optional)
   - Configure handicap rules
   - Set registration deadline

3. **Payment**
   - Pay platform fee ($49 for one-time event)
   - Stripe checkout
   - Event is published

4. **Share Event**
   - Get shareable link: `golfmanager.app/events/acme-charity-scramble`
   - Share registration code (if private)
   - Send invites via email

### Teams Register

1. **Find Event**
   - Visit public event link or enter registration code
   - View event details

2. **Register Team**
   - Enter team name
   - Add player names and handicaps
   - Provide contact info
   - Pay entry fee (if applicable)

3. **Receive Confirmation**
   - Email confirmation with event details
   - Login credentials for score entry

### Tournament Day

1. **Check-In**
   - Teams check in at event
   - Organizer assigns starting holes (shotgun start)
   - Teams receive scorecards

2. **Live Scoring**
   - Team captain logs in on mobile
   - Enters scores hole-by-hole
   - Scores broadcast via SignalR
   - Leaderboard updates in real-time

3. **Hole Games**
   - Record closest to pin, longest drive, etc.
   - Update winners in real-time

4. **Completion**
   - Teams finish and submit final scores
   - Leaderboard finalizes
   - Winners announced

### Post-Event

1. **Results**
   - View final leaderboard
   - Export results (PDF, CSV)
   - Share results on social media

2. **Photos & Recap**
   - Upload tournament photos
   - Post-event summary

## 💰 Pricing Model

### Platform Fees (One-Time Events)

- **Basic Event**: $49 per tournament
  - Up to 50 teams
  - Real-time scoring
  - Basic leaderboard
  - Email support

- **Premium Event**: $99 per tournament
  - Up to 100 teams
  - Advanced hole games
  - Custom branding
  - Photo gallery
  - Priority support

- **Enterprise Event**: $199 per tournament
  - Unlimited teams
  - White-label experience
  - Dedicated support
  - Custom features

### Optional Add-Ons

- **Entry Fee Collection**: 3% of collected fees
- **Custom Domain**: +$20
- **SMS Notifications**: +$15
- **Professional Reporting**: +$25

## 🔧 API Endpoints

### Event Management

```
POST   /api/v1/events/one-time
GET    /api/v1/events/one-time/{key}
PUT    /api/v1/events/one-time/{key}
DELETE /api/v1/events/one-time/{key}
POST   /api/v1/events/one-time/{key}/publish
POST   /api/v1/events/one-time/{key}/cancel

# Payment
POST   /api/v1/events/one-time/{key}/payment
GET    /api/v1/events/one-time/{key}/payment/status
```

### Team Registration

```
GET    /api/v1/events/one-time/{key}/teams
POST   /api/v1/events/one-time/{key}/teams/register
GET    /api/v1/events/one-time/{key}/teams/{teamId}
PUT    /api/v1/events/one-time/{key}/teams/{teamId}
POST   /api/v1/events/one-time/{key}/teams/{teamId}/check-in
```

### Scoring

```
GET    /api/v1/events/one-time/{key}/teams/{teamId}/scores
PUT    /api/v1/events/one-time/{key}/teams/{teamId}/scores/{holeNumber}
POST   /api/v1/events/one-time/{key}/teams/{teamId}/scores/batch
POST   /api/v1/events/one-time/{key}/teams/{teamId}/complete
```

### Leaderboard

```
GET    /api/v1/events/one-time/{key}/leaderboard
GET    /api/v1/events/one-time/{key}/leaderboard/live
```

### Hole Games

```
GET    /api/v1/events/one-time/{key}/hole-games
POST   /api/v1/events/one-time/{key}/hole-games
PUT    /api/v1/events/one-time/{key}/hole-games/{gameId}/winner
```

## 🎨 UI Features

### Public Event Page

- Event details and description
- Course information
- Registration form
- Current team list
- Live leaderboard (during event)

### Organizer Dashboard

- Event overview
- Team management
- Check-in status
- Live scoring monitor
- Hole game management
- Results and reporting

### Team Scorecard (Mobile)

- Hole-by-hole score entry
- Current team position
- Live leaderboard
- Hole games tracking
- Photo upload

## 📱 Real-Time Features (SignalR)

### Live Updates

- Score entry broadcasts to all viewers
- Leaderboard updates automatically
- Hole game winners announced
- Team check-ins visible
- Completion notifications

### SignalR Hub

```csharp
public class OneTimeEventHub : Hub
{
    public async Task JoinEvent(string eventKey)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, eventKey);
    }
    
    public async Task ScoreUpdated(string eventKey, string teamId, int holeNumber, int score)
    {
        await Clients.Group(eventKey).SendAsync("ScoreUpdated", teamId, holeNumber, score);
    }
    
    public async Task LeaderboardUpdated(string eventKey, object leaderboard)
    {
        await Clients.Group(eventKey).SendAsync("LeaderboardUpdated", leaderboard);
    }
}
```

## ✅ Benefits

### For Organizers

- ✅ No league setup required
- ✅ Quick tournament creation
- ✅ Real-time scoring management
- ✅ Professional leaderboards
- ✅ Automated results
- ✅ Entry fee collection (optional)

### For Participants

- ✅ Easy registration
- ✅ Mobile score entry
- ✅ Live leaderboard viewing
- ✅ No app download required (web-based)
- ✅ Instant results

### For GolfManager

- ✅ New revenue stream ($49-$199 per event)
- ✅ Lower barrier to entry (no subscription)
- ✅ Viral growth potential (public events)
- ✅ Upsell to league subscriptions

## 🚀 Future Enhancements

- [ ] Spectator mode (view-only access)
- [ ] Live streaming integration
- [ ] Automated pairings
- [ ] Prize fund management
- [ ] Sponsor logos and branding
- [ ] Post-event surveys
- [ ] Photo galleries
- [ ] Social media integration
- [ ] Recurring annual events

