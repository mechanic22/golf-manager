# Pinzo - Complete Sitemap & Navigation Structure

## Overview
This document defines the complete page hierarchy, navigation structure, and role-based access for the Pinzo golf management application.

---

## Role Definitions

| Role | Description | Permissions |
|------|-------------|-------------|
| **Global Admin** | Platform owner | Full access to everything |
| **League Admin** | League commissioner/owner | Manage specific league(s), create seasons, manage members |
| **Season Admin** | Season coordinator | Manage specific season(s), create events, enter scores |
| **Moderator** | Scorekeeper | Enter scores for events |
| **Player** | League member | View leagues, register for events, view own stats |

---

## Page Hierarchy

### 🏠 Public Pages (MainLayout)
**Layout**: `MainLayout.razor` (transparent nav, public-facing)

#### `/` - Home Page
- **Access**: Everyone (public)
- **Purpose**: Marketing landing page
- **Content**:
  - Hero section with CTA
  - Feature highlights (Score Tracking, Handicaps, Season Management)
  - "Get Started" / "Sign In" buttons (if not authenticated)
  - "Go to Dashboard" button (if authenticated)
- **Navigation**: Top nav with Home, Icons, Sign In, Get Started

#### `/login` - Login Page
- **Access**: Everyone (public)
- **Purpose**: User authentication
- **Content**: Email/password form, "Forgot Password" link, "Create Account" link

#### `/register` - Registration Page
- **Access**: Everyone (public)
- **Purpose**: New user signup
- **Content**: Email, password, confirm password, first/last name

#### `/icons` - Icon Library
- **Access**: Everyone (public)
- **Purpose**: Developer reference for available icons
- **Content**: Grid of all Material Icons and Golf Icons

---

### 🎯 Authenticated Pages (AppLayout)
**Layout**: `AppLayout.razor` (solid nav, app-focused)

#### `/dashboard` - User Dashboard
- **Access**: All authenticated users
- **Purpose**: Personal hub showing user's leagues and events
- **Content by Role**:
  - **All Users**:
    - "My Leagues" section (leagues user is a member of)
    - "Upcoming Events" section (events user is registered for)
    - Quick stats (rounds played, current handicap)
  - **Global Admin**:
    - Additional "Platform Stats" section
    - "Create League" button
  - **League Admin**:
    - "Create League" button
    - "Manage My Leagues" quick links
- **Navigation**: Dashboard, Leagues, Players, Events

---

### 🏆 League Pages (AppLayout)
**Base Route**: `/league/{key}`

#### `/league/{key}` - League Overview
- **Access**: League members + admins
- **Purpose**: League dashboard and quick stats
- **Content by Role**:
  - **All Members**:
    - League name, description
    - Active seasons list
    - Recent activity feed
    - Member count
  - **League Admin**:
    - "Create Season" button
    - "Manage Members" button
    - "Edit League" button
- **Tabs**: Overview, Seasons, Members, Settings (admin only)

#### `/league/{key}/seasons` - League Seasons
- **Access**: League members + admins
- **Purpose**: View all seasons for this league
- **Content by Role**:
  - **All Members**:
    - List of seasons (Active, Upcoming, Past)
    - Season cards with: name, dates, event count, player count
  - **League Admin**:
    - "Create Season" button
    - Edit/Delete season actions
- **Tabs**: Overview, Seasons, Members, Settings (admin only)

#### `/league/{key}/members` - League Members
- **Access**: League members + admins
- **Purpose**: View and manage league membership
- **Content by Role**:
  - **All Members**:
    - List of members with name, email, join date
    - Member stats (rounds played, avg score)
  - **League Admin**:
    - "Invite Member" button
    - "Remove Member" action
    - "Promote to Admin" action
- **Tabs**: Overview, Seasons, Members, Settings (admin only)

#### `/league/{key}/settings` - League Settings
- **Access**: League Admin + Global Admin only
- **Purpose**: Configure league settings
- **Content**:
  - League name, key, description
  - Default scoring format
  - Privacy settings
  - Danger zone (delete league)
- **Tabs**: Overview, Seasons, Members, Settings

---

### 📅 Season Pages (AppLayout)
**Base Route**: `/league/{key}/season/{seasonKey}`

#### `/league/{key}/season/{seasonKey}` - Season Overview
- **Access**: League members + admins
- **Purpose**: Season dashboard and quick stats
- **Content by Role**:
  - **All Members**:
    - Season name, dates, status
    - Leaderboard (top 5)
    - Upcoming events
    - Recent rounds
  - **Season Admin / League Admin**:
    - "Create Event" button
    - "Manage Players" button
    - "Edit Season" button
- **Tabs**: Overview, Events, Standings, Players, Settings (admin only)

#### `/league/{key}/season/{seasonKey}/events` - Season Events
- **Access**: League members + admins
- **Purpose**: View all events in this season
- **Content by Role**:
  - **All Members**:
    - List of events (Upcoming, In Progress, Completed)
    - Event cards with: name, date, course, registered players
    - "Register" button (if not registered)
  - **Season Admin / League Admin**:
    - "Create Event" button
    - "Edit Event" action
    - "Enter Scores" button (for completed events)
- **Tabs**: Overview, Events, Standings, Players, Settings (admin only)

#### `/league/{key}/season/{seasonKey}/standings` - Season Standings
- **Access**: League members + admins
- **Purpose**: View season leaderboard and player rankings
- **Content**:
  - Full leaderboard table
  - Columns: Rank, Player, Rounds, Avg Score, Total Points, Handicap
  - Filter by: All Players, Active Players
  - Sort by: Rank, Name, Avg Score
- **Tabs**: Overview, Events, Standings, Players, Settings (admin only)

#### `/league/{key}/season/{seasonKey}/players` - Season Players
- **Access**: League members + admins
- **Purpose**: View and manage season participants
- **Content by Role**:
  - **All Members**:
    - List of players in this season
    - Player cards with: name, handicap, rounds played
  - **Season Admin / League Admin**:
    - "Add Player" button
    - "Remove Player" action
    - "Edit Handicap" action
- **Tabs**: Overview, Events, Standings, Players, Settings (admin only)

---

### 🏌️ Event Pages (AppLayout)
**Base Route**: `/league/{key}/season/{seasonKey}/event/{eventKey}`

#### `/league/{key}/season/{seasonKey}/event/{eventKey}` - Event Detail
- **Access**: League members + admins
- **Purpose**: View event details and register
- **Content by Role**:
  - **All Members**:
    - Event name, date, time, course
    - Registered players list
    - "Register" button (if not registered, event is open)
    - "Unregister" button (if registered, before deadline)
    - Leaderboard (if event is completed)
  - **Season Admin / League Admin**:
    - "Edit Event" button
    - "Enter Scores" button
    - "Close Registration" button
- **Actions**:
  - Register for event
  - View event leaderboard
  - Navigate to score entry (admin only)

#### `/league/{key}/season/{seasonKey}/event/{eventKey}/scores` - Score Entry
- **Access**: Season Admin, League Admin, Global Admin, Moderator only
- **Purpose**: Enter scores for event participants
- **Content**:
  - Player list with score entry fields
  - Hole-by-hole scorecard
  - "Save Scores" button
  - "Finalize Event" button (marks event as complete)
- **Validation**:
  - Scores must be valid (par ± reasonable range)
  - All players must have scores before finalizing

---

### 👤 Player/Profile Pages (AppLayout)
**Base Route**: `/player/{id}` or `/profile`

#### `/profile` - My Profile
- **Access**: Authenticated users (own profile)
- **Purpose**: View and edit personal information
- **Content**:
  - Name, email, phone
  - Profile photo
  - Handicap history
  - Recent rounds
  - Stats summary
- **Tabs**: Profile, Stats, Rounds, Handicap

#### `/profile/stats` - My Stats
- **Access**: Authenticated users (own profile)
- **Purpose**: Detailed personal statistics
- **Content**:
  - Scoring average by course
  - Best/worst holes
  - Trends over time
  - Comparison to league average

#### `/profile/rounds` - My Rounds
- **Access**: Authenticated users (own profile)
- **Purpose**: View all rounds played
- **Content**:
  - List of rounds with: date, course, score, differential
  - Filter by: season, league, date range
  - Export to CSV

#### `/profile/handicap` - Handicap History
- **Access**: Authenticated users (own profile)
- **Purpose**: View handicap calculation history
- **Content**:
  - Current handicap index
  - Handicap trend chart
  - Rounds used in calculation
  - Explanation of calculation

#### `/player/{id}` - Player Detail (Public Profile)
- **Access**: League members (for players in same league)
- **Purpose**: View another player's public profile
- **Content**:
  - Name, photo
  - Current handicap
  - Leagues they're in
  - Recent rounds (limited)
  - Stats summary (limited)

---

### 🔍 Browse/Search Pages (AppLayout)

#### `/leagues` - Browse Leagues
- **Access**: Authenticated users
- **Purpose**: Discover and join leagues
- **Content**:
  - "My Leagues" section
  - "Discover Leagues" section (public leagues)
  - Search/filter by: name, location, format
  - "Create League" button (Global Admin, League Admin)

#### `/events` - Browse Events
- **Access**: Authenticated users
- **Purpose**: Discover and register for one-time events
- **Content**:
  - "My Events" section (registered events)
  - "Upcoming Events" section (all public events)
  - Filter by: date, location, format
  - "Create Event" button (Global Admin)

#### `/players` - Browse Players
- **Access**: Authenticated users
- **Purpose**: Find and connect with other players
- **Content**:
  - Search players by name
  - Filter by: league, handicap range
  - Player cards with: name, handicap, leagues

---

### ⚙️ Admin Pages (AppLayout)

#### `/admin` - Global Admin Dashboard
- **Access**: Global Admin only
- **Purpose**: Platform administration
- **Content**:
  - Platform stats (total users, leagues, events)
  - Recent activity
  - User management
  - League management
  - System settings

---

## Navigation Patterns

### Top Navigation (AppLayout)
- **Dashboard** - Always visible
- **Leagues** - Always visible
- **Players** - Always visible
- **Events** - Always visible
- **User Menu** - Dropdown with Profile, Settings, Logout

### Breadcrumbs
- **League Pages**: Dashboard > League Name
- **Season Pages**: Dashboard > League Name > Season Name
- **Event Pages**: Dashboard > League Name > Season Name > Event Name

### Tab Navigation
- **League Level**: Overview, Seasons, Members, Settings (admin)
- **Season Level**: Overview, Events, Standings, Players, Settings (admin)

---

## Role-Based Visibility Matrix

| Page | Player | Moderator | Season Admin | League Admin | Global Admin |
|------|--------|-----------|--------------|--------------|--------------|
| Dashboard | ✅ | ✅ | ✅ | ✅ | ✅ |
| League Overview | ✅ | ✅ | ✅ | ✅ | ✅ |
| League Seasons | ✅ | ✅ | ✅ | ✅ | ✅ |
| League Members | ✅ | ✅ | ✅ | ✅ | ✅ |
| League Settings | ❌ | ❌ | ❌ | ✅ | ✅ |
| Season Overview | ✅ | ✅ | ✅ | ✅ | ✅ |
| Season Events | ✅ | ✅ | ✅ | ✅ | ✅ |
| Season Standings | ✅ | ✅ | ✅ | ✅ | ✅ |
| Season Players | ✅ | ✅ | ✅ | ✅ | ✅ |
| Event Detail | ✅ | ✅ | ✅ | ✅ | ✅ |
| Score Entry | ❌ | ✅ | ✅ | ✅ | ✅ |
| My Profile | ✅ | ✅ | ✅ | ✅ | ✅ |
| Browse Leagues | ✅ | ✅ | ✅ | ✅ | ✅ |
| Browse Events | ✅ | ✅ | ✅ | ✅ | ✅ |
| Browse Players | ✅ | ✅ | ✅ | ✅ | ✅ |
| Global Admin | ❌ | ❌ | ❌ | ❌ | ✅ |

---

## Next Steps

1. ✅ **Sitemap Complete** - This document
2. ⏳ **Create Missing Pages** - Build stubs for pages that don't exist yet
3. ⏳ **Update Navigation** - Ensure all links work and are role-aware
4. ⏳ **Add Breadcrumbs** - Implement breadcrumb navigation
5. ⏳ **Document Data Needs** - For each page, list required API endpoints


