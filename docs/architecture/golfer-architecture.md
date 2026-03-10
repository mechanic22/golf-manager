# Golfer Architecture - Global vs League-Specific

## 🎯 Overview

The GolfManager v2 architecture treats **Golfers as global entities** that can participate in multiple leagues, with league-specific profiles and statistics.

## 📊 Entity Hierarchy

```
User (Authentication & Identity)
  ↓ (1:0..1) OPTIONAL
Golfer (Global Player Profile)
  ↓ (1:N)
LeagueGolfer (League Membership & Profile)
  ↓ (1:N)
SeasonGolfer (Season Participation)
  ↓ (1:N)
SeasonEventGolfer (Event Participation)
```

## 🗂️ Entity Breakdown

### 1. User (Authentication & Identity Layer)
- Email, password, authentication
- Global admin flag
- **Can optionally have a Golfer profile** (not required)

**User Types:**
- **Golfer Users**: Have a Golfer profile, can play in leagues
- **Admin Users**: League admins who don't play
- **Scorekeeper Users**: Enter scores but don't play
- **Viewer Users**: Family/friends who just view scores
- **Future**: Sponsors, media, etc.

### 2. Golfer (Global Player Identity)
**Purpose**: The golfer's universal identity across all leagues

**Important**:
- Every Golfer **must** have a User (for authentication)
- Not every User needs to be a Golfer (admins, scorekeepers, etc.)
- Relationship: User (1) ←→ (0..1) Golfer

**Key Fields**:
- `UserId` - One-to-one with User (required)
- `DisplayName` - Primary name
- `Nickname`, `AvatarUrl`, `Bio`
- `GlobalHandicap` - Overall handicap across all play
- `PhoneNumber`, `HomeCity`, `HomeState`

**Relationships**:
- One-to-one with User (required)
- One-to-many with LeagueGolfer (league memberships)
- One-to-many with Round (all rounds, league + casual)
- One-to-many with GolferClub (equipment)

**Stats Available**:
- Total rounds played (all leagues + casual)
- Overall average score
- Best/worst rounds
- Global handicap
- Equipment tracking

### 3. LeagueGolfer (League-Specific Profile)
**Purpose**: Represents a golfer's membership and profile within a specific league

**Key Fields**:
- `LeagueDisplayName` - Override name for this league (optional)
- `LeagueNickname`, `LeagueAvatarUrl` - League-specific customization
- `LeagueHandicap` - Handicap specific to this league
- `IsLeagueAdmin` - Admin rights in this league
- `JoinedAt`, `LeftAt` - Membership dates

**Relationships**:
- Many-to-one with Golfer (global)
- Many-to-one with League
- One-to-many with SeasonGolfer (season participation)

**Stats Available**:
- Rounds in this league only
- League-specific average score
- League handicap history
- Seasons participated in
- League-specific achievements

### 4. SeasonGolfer (Season Participation)
**Purpose**: Tracks a golfer's participation in a specific season

**Key Fields**:
- `SeasonHandicap` - Handicap for this season
- `TeamId` - Team assignment
- `TotalEvents`, `AverageScore`, `TotalPoints` - Season stats

### 5. SeasonEventGolfer (Event Participation)
**Purpose**: Tracks participation in a specific event

**Key Fields**:
- `EventHandicap` - Handicap at time of event
- `RawScore`, `NetScore`, `Points`
- `Position` - Finishing position

## 🎯 Use Cases

### Use Case 1: Player Joins First League
```
1. User creates account → User entity created
2. User creates golfer profile → Golfer entity created (optional step)
3. Golfer joins "Sunday League" → LeagueGolfer entity created
4. Golfer joins "2024 Season" → SeasonGolfer entity created
```

### Use Case 1b: Admin Creates Account (Non-Player)
```
1. User creates account → User entity created
2. User does NOT create golfer profile (admin only)
3. User is added as league admin → UserLeague created (no LeagueGolfer)
4. User can manage league but cannot play
```

### Use Case 2: Golfer Joins Second League
```
1. User already has Golfer profile (from League 1)
2. User joins "Wednesday League" → New LeagueGolfer entity created
3. Same Golfer, different LeagueGolfer profiles
4. Can have different display names, handicaps per league
```

### Use Case 3: Casual Round (No League)
```
1. Golfer plays a casual round
2. Round.LeagueId = null
3. Round.GolferId = golfer's global ID
4. Counts toward global stats, not league stats
```

### Use Case 4: League Round
```
1. Golfer plays in league event
2. Round.LeagueId = league ID
3. Round.LeagueGolferId = league golfer ID
4. Counts toward both global AND league stats
```

## 📈 Statistics Tracking

### Global Stats (via `/api/v1/golfers/me/stats`)
- All rounds across all leagues + casual rounds
- Overall average score
- Total rounds played
- Global handicap
- Best/worst performances
- Equipment statistics

### League Stats (via `/api/v1/leagues/{key}/golfers/{id}/stats`)
- Rounds in this league only
- League average score
- League handicap
- League-specific achievements
- Season history in this league

### Season Stats (via `/api/v1/leagues/{key}/seasons/{key}/golfers/{id}`)
- Season-specific performance
- Event participation
- Team contributions
- Season handicap progression

## 🔐 Privacy & Visibility

### Public Information (Any User Can See)
- Golfer display name
- Avatar
- Public stats (if golfer opts in)

### League Members Can See
- League-specific profile
- League stats
- Handicap in that league
- Round history in that league

### Private Information (Golfer Only)
- Global stats across all leagues
- Equipment details
- Personal handicap
- Casual rounds

## 🎨 Customization Per League

Golfers can customize their profile per league:
- **Display Name**: "John Smith" in League A, "Johnny" in League B
- **Avatar**: Different avatar per league
- **Nickname**: League-specific nickname
- **Handicap**: Calculated separately per league

## 🔄 Data Flow Example

### Posting a Score in a League Event

```
1. POST /api/v1/leagues/sunday-league/rounds
   - Creates Round with LeagueId and LeagueGolferId
   
2. Background Processing:
   - Update Round.TotalScore (sum of holes)
   - Update SeasonEventGolfer.RawScore
   - Update SeasonGolfer.AverageScore (league stats)
   - Update Golfer.GlobalHandicap (global stats)
   - Update LeagueGolfer.LeagueHandicap (league stats)
   
3. SignalR Notifications:
   - Notify league members of score update
   - Update league leaderboard
   - Update season standings
```

## ✅ Benefits of This Architecture

1. **Single Sign-On**: One account, multiple leagues
2. **Comprehensive Stats**: Track performance globally and per-league
3. **Flexibility**: Different identity per league if desired
4. **Casual Play**: Support rounds outside of league play
5. **Data Portability**: Golfer owns their global data
6. **Privacy**: League stats isolated from other leagues
7. **Scalability**: Easy to add new leagues without data duplication

