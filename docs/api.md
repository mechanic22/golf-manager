# API Reference

## Overview

GolfManager v2 provides a RESTful API built with ASP.NET Core. All endpoints require JWT authentication and return JSON responses wrapped in a standard `ApiResponse<T>` format.

## Authentication

All API requests require a Bearer token in the Authorization header:

```
Authorization: Bearer <jwt-token>
```

### Authentication Endpoints

#### POST /api/v1/auth/login
Authenticate user and return JWT tokens.

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "password"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJ...",
    "refreshToken": "refresh-token",
    "user": { "id": "1", "email": "user@example.com" }
  }
}
```

#### POST /api/v1/auth/refresh
Refresh access token using refresh token.

## Core Resources

### Leagues

#### GET /api/v1/leagues
Get all leagues for the authenticated user.

#### GET /api/v1/leagues/{leagueId}
Get specific league details.

#### POST /api/v1/leagues
Create a new league (Admin only).

#### PUT /api/v1/leagues/{leagueId}
Update league settings.

### Players

#### GET /api/v1/players?leagueId={leagueId}
Get all players in a league.

#### GET /api/v1/players/{playerId}
Get player details and statistics.

#### POST /api/v1/players
Add player to league.

#### PUT /api/v1/players/{playerId}
Update player information.

### Seasons

#### GET /api/v1/seasons?leagueId={leagueId}
Get seasons for a league.

#### POST /api/v1/seasons
Create new season.

#### PUT /api/v1/seasons/{seasonId}
Update season configuration.

### Events

#### GET /api/v1/events?leagueId={leagueId}&seasonId={seasonId}
Get events for a season.

#### POST /api/v1/events
Schedule new event.

#### PUT /api/v1/events/{eventId}
Update event details.

### Rounds & Scoring

#### GET /api/v1/rounds?eventId={eventId}
Get rounds for an event.

#### POST /api/v1/rounds
Start a new round.

#### PUT /api/v1/rounds/{roundId}/scores
Update scores for a round.

#### GET /api/v1/rounds/{roundId}/leaderboard
Get live leaderboard for a round.

### Handicap Management

#### GET /api/v1/handicap/{playerId}
Get current handicap for a player.

#### POST /api/v1/handicap/calculate
Trigger handicap recalculation.

#### GET /api/v1/handicap/history/{playerId}
Get handicap history.

## Real-time Updates

The API includes SignalR hubs for real-time features:

- **Score Updates**: `/hubs/scores` - Live score changes
- **Event Notifications**: `/hubs/events` - Event status updates
- **Leaderboards**: `/hubs/leaderboards` - Live leaderboard updates

## Response Format

All API responses follow this structure:

```json
{
  "success": true|false,
  "data": T | null,
  "message": "Optional message",
  "errors": ["Error details"] | null
}
```

## Error Handling

- **400 Bad Request**: Validation errors
- **401 Unauthorized**: Missing/invalid authentication
- **403 Forbidden**: Insufficient permissions
- **404 Not Found**: Resource not found
- **500 Internal Server Error**: Server errors

## Rate Limiting

API endpoints are rate-limited to prevent abuse. Standard limits:
- 100 requests per minute for authenticated users
- 10 requests per minute for unauthenticated endpoints

## Versioning

API is versioned with `/api/v1/` prefix. Future versions will use `/api/v2/`, etc.</content>
<parameter name="filePath">/Users/tonygilbert/Projects/code/5thbox/dkgolf/golf-manager/docs/api.md