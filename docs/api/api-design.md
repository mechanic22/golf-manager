# GolfManager v2 - API Design

## 🎯 API Design Principles

1. **RESTful**: Follow REST conventions and HTTP semantics
2. **Versioned**: URL-based versioning (`/api/v1/...`)
3. **Consistent**: Standard response formats and error handling
4. **Secure**: JWT authentication, league-based authorization
5. **Documented**: OpenAPI/Swagger documentation
6. **Performant**: Pagination, filtering, caching support

## 🔐 Authentication

### Endpoints

```
POST   /api/v1/auth/register
POST   /api/v1/auth/login
POST   /api/v1/auth/refresh
POST   /api/v1/auth/logout
GET    /api/v1/auth/me
POST   /api/v1/auth/change-password
POST   /api/v1/auth/forgot-password
POST   /api/v1/auth/reset-password
```

### OAuth Support
```
GET    /api/v1/auth/oauth/{provider}/login
GET    /api/v1/auth/oauth/{provider}/callback
```

## 🏌️ League Management

### Endpoints

```
GET    /api/v1/leagues                          # List all leagues (user's leagues)
POST   /api/v1/leagues                          # Create league (admin only)
GET    /api/v1/leagues/{leagueKey}              # Get league details
PUT    /api/v1/leagues/{leagueKey}              # Update league
DELETE /api/v1/leagues/{leagueKey}              # Delete league (soft)

GET    /api/v1/leagues/{leagueKey}/members      # List league members
POST   /api/v1/leagues/{leagueKey}/members      # Add member
DELETE /api/v1/leagues/{leagueKey}/members/{userId}  # Remove member

# Custom Domain Management
GET    /api/v1/leagues/{leagueKey}/domain       # Get custom domain settings
PUT    /api/v1/leagues/{leagueKey}/domain       # Set custom domain
POST   /api/v1/leagues/{leagueKey}/domain/verify  # Verify domain ownership
DELETE /api/v1/leagues/{leagueKey}/domain       # Remove custom domain
```

## 📅 Season Management

### Endpoints

```
GET    /api/v1/leagues/{leagueKey}/seasons
POST   /api/v1/leagues/{leagueKey}/seasons
GET    /api/v1/leagues/{leagueKey}/seasons/{seasonKey}
PUT    /api/v1/leagues/{leagueKey}/seasons/{seasonKey}
DELETE /api/v1/leagues/{leagueKey}/seasons/{seasonKey}
POST   /api/v1/leagues/{leagueKey}/seasons/{seasonKey}/lock
POST   /api/v1/leagues/{leagueKey}/seasons/{seasonKey}/unlock

# Season Settings
GET    /api/v1/leagues/{leagueKey}/seasons/{seasonKey}/settings
PUT    /api/v1/leagues/{leagueKey}/seasons/{seasonKey}/settings

# Season Golfers
GET    /api/v1/leagues/{leagueKey}/seasons/{seasonKey}/golfers
POST   /api/v1/leagues/{leagueKey}/seasons/{seasonKey}/golfers
DELETE /api/v1/leagues/{leagueKey}/seasons/{seasonKey}/golfers/{golferId}

# Season Teams
GET    /api/v1/leagues/{leagueKey}/seasons/{seasonKey}/teams
POST   /api/v1/leagues/{leagueKey}/seasons/{seasonKey}/teams
GET    /api/v1/leagues/{leagueKey}/seasons/{seasonKey}/teams/{teamId}
PUT    /api/v1/leagues/{leagueKey}/seasons/{seasonKey}/teams/{teamId}
DELETE /api/v1/leagues/{leagueKey}/seasons/{seasonKey}/teams/{teamId}

# Leaderboards
GET    /api/v1/leagues/{leagueKey}/seasons/{seasonKey}/leaderboard/individual
GET    /api/v1/leagues/{leagueKey}/seasons/{seasonKey}/leaderboard/team
```

## 📆 Event Management

### Endpoints

```
GET    /api/v1/leagues/{leagueKey}/seasons/{seasonKey}/events
POST   /api/v1/leagues/{leagueKey}/seasons/{seasonKey}/events
GET    /api/v1/leagues/{leagueKey}/seasons/{seasonKey}/events/{eventId}
PUT    /api/v1/leagues/{leagueKey}/seasons/{seasonKey}/events/{eventId}
DELETE /api/v1/leagues/{leagueKey}/seasons/{seasonKey}/events/{eventId}

# Event Golfers
GET    /api/v1/leagues/{leagueKey}/seasons/{seasonKey}/events/{eventId}/golfers
POST   /api/v1/leagues/{leagueKey}/seasons/{seasonKey}/events/{eventId}/golfers
GET    /api/v1/leagues/{leagueKey}/seasons/{seasonKey}/events/{eventId}/golfers/{golferId}

# Event Matches
GET    /api/v1/leagues/{leagueKey}/seasons/{seasonKey}/events/{eventId}/matches
POST   /api/v1/leagues/{leagueKey}/seasons/{seasonKey}/events/{eventId}/matches
GET    /api/v1/leagues/{leagueKey}/seasons/{seasonKey}/events/{eventId}/matches/{matchId}
PUT    /api/v1/leagues/{leagueKey}/seasons/{seasonKey}/events/{eventId}/matches/{matchId}
POST   /api/v1/leagues/{leagueKey}/seasons/{seasonKey}/events/{eventId}/matches/{matchId}/calculate

# Event Leaderboard
GET    /api/v1/leagues/{leagueKey}/seasons/{seasonKey}/events/{eventId}/leaderboard
```

## 👤 Golfer Management (Global)

### Golfer Profile Endpoints

```
# Global Golfer Profile (User's golfer identity)
GET    /api/v1/golfers/me                           # Current user's golfer profile
PUT    /api/v1/golfers/me                           # Update golfer profile
GET    /api/v1/golfers/{golferId}                   # Get any golfer's public profile

# Global Stats (Across All Leagues)
GET    /api/v1/golfers/me/stats                     # My stats across all leagues
GET    /api/v1/golfers/me/stats/summary             # Aggregated summary
GET    /api/v1/golfers/me/rounds                    # All rounds across leagues
GET    /api/v1/golfers/me/handicap                  # Overall personal handicap
GET    /api/v1/golfers/me/handicap-history          # Global handicap history
GET    /api/v1/golfers/{golferId}/stats             # Public stats for any golfer

# League Memberships
GET    /api/v1/golfers/me/leagues                   # All leagues I'm part of
GET    /api/v1/golfers/me/leagues/{leagueKey}       # My profile in specific league

# Equipment (Global - shared across leagues)
GET    /api/v1/golfers/me/clubs                     # My golf clubs
POST   /api/v1/golfers/me/clubs                     # Add club to bag
PUT    /api/v1/golfers/me/clubs/{clubId}            # Update club
DELETE /api/v1/golfers/me/clubs/{clubId}            # Remove club
```

## 🏌️ League Golfer Management (League-Specific)

### League Golfer Endpoints

```
# League Golfer Directory
GET    /api/v1/leagues/{leagueKey}/golfers          # All golfers in this league
POST   /api/v1/leagues/{leagueKey}/golfers          # Add golfer to league (invite/join)
GET    /api/v1/leagues/{leagueKey}/golfers/{golferId}  # Golfer's league profile
PUT    /api/v1/leagues/{leagueKey}/golfers/{golferId}  # Update league-specific profile
DELETE /api/v1/leagues/{leagueKey}/golfers/{golferId}  # Remove from league

# League-Specific Stats
GET    /api/v1/leagues/{leagueKey}/golfers/{golferId}/stats           # Stats in this league only
GET    /api/v1/leagues/{leagueKey}/golfers/{golferId}/rounds          # Rounds in this league
GET    /api/v1/leagues/{leagueKey}/golfers/{golferId}/handicap        # League handicap
GET    /api/v1/leagues/{leagueKey}/golfers/{golferId}/handicap-history  # League handicap history
GET    /api/v1/leagues/{leagueKey}/golfers/{golferId}/seasons         # Seasons participated in

# League Golfer Settings
GET    /api/v1/leagues/{leagueKey}/golfers/me/settings                # My league-specific settings
PUT    /api/v1/leagues/{leagueKey}/golfers/me/settings                # Update league settings
```

## ⛳ Course Management

### Endpoints (Global - No league context)

```
GET    /api/v1/courses
POST   /api/v1/courses
GET    /api/v1/courses/{courseKey}
PUT    /api/v1/courses/{courseKey}
DELETE /api/v1/courses/{courseKey}

# Tees
GET    /api/v1/courses/{courseKey}/tees
POST   /api/v1/courses/{courseKey}/tees
GET    /api/v1/courses/{courseKey}/tees/{teeId}
PUT    /api/v1/courses/{courseKey}/tees/{teeId}
DELETE /api/v1/courses/{courseKey}/tees/{teeId}

# Holes
GET    /api/v1/courses/{courseKey}/holes
POST   /api/v1/courses/{courseKey}/holes
GET    /api/v1/courses/{courseKey}/holes/{holeNumber}
PUT    /api/v1/courses/{courseKey}/holes/{holeNumber}

# GPS & Mobile Support
GET    /api/v1/courses/{courseKey}/holes/{holeNumber}/gps     # Get GPS data for hole
PUT    /api/v1/courses/{courseKey}/holes/{holeNumber}/gps     # Update GPS coordinates
POST   /api/v1/courses/{courseKey}/detect-hole                # Detect hole from GPS coords
     # Body: { latitude, longitude }
     # Returns: { holeNumber, distance, suggestedClub }
```

## 🏌️‍♂️ Round & Scoring

### Endpoints

```
# Rounds
GET    /api/v1/leagues/{leagueKey}/rounds
POST   /api/v1/leagues/{leagueKey}/rounds
GET    /api/v1/leagues/{leagueKey}/rounds/{roundId}
PUT    /api/v1/leagues/{leagueKey}/rounds/{roundId}
DELETE /api/v1/leagues/{leagueKey}/rounds/{roundId}

# Round Holes (Score Entry)
GET    /api/v1/leagues/{leagueKey}/rounds/{roundId}/holes
PUT    /api/v1/leagues/{leagueKey}/rounds/{roundId}/holes/{holeNumber}
POST   /api/v1/leagues/{leagueKey}/rounds/{roundId}/holes/batch  # Batch update

# Scorecards
GET    /api/v1/leagues/{leagueKey}/scorecards
POST   /api/v1/leagues/{leagueKey}/scorecards
GET    /api/v1/leagues/{leagueKey}/scorecards/{scorecardId}
POST   /api/v1/leagues/{leagueKey}/scorecards/{scorecardId}/complete
```

See [api-design-part2.md](./api-design-part2.md) for request/response models and examples.

