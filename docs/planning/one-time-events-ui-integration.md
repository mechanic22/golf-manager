# One-Time Events UI Integration Plan

## Overview

This document outlines the plan to integrate one-time events into the Pinzo UI, creating a seamless experience for both event organizers and participants.

## Design Principles

1. **Dual-Mode System**: Support both league-based events (existing) and standalone one-time events (new)
2. **Public Discovery**: Allow anyone to discover and register for public events
3. **Simple Creation**: Make it easy for organizers to create events without league setup
4. **Mobile-First**: Optimize for mobile registration and scoring
5. **Professional UI**: Match the existing Pinzo design language

## User Flows

### Flow 1: Public User Discovers Event
1. Visit `/events` (public events discovery page)
2. Browse upcoming public events
3. Click event to view details
4. Register team (with or without account)
5. Receive confirmation email
6. Check-in on event day
7. Enter scores during round
8. View live leaderboard

### Flow 2: Organizer Creates Event
1. Login to dashboard
2. Click "Create Event" button
3. Complete event creation wizard:
   - Basic Info (name, date, description)
   - Venue (course, tees, holes)
   - Format (scramble, stroke play, team size, handicaps)
   - Access (public, private, invite-only)
   - Registration (deadline, max teams, registration code)
4. Publish event
5. Share event link
6. Manage registrations
7. Check-in teams on event day
8. Monitor live scoring
9. Finalize results

### Flow 3: Authenticated User Views Their Events
1. Login to dashboard
2. See "My Events" section showing:
   - Events I'm organizing
   - Events I'm registered for
   - Past events
3. Click event to manage or view details

## Page Structure

### 1. `/events` - Public Events Discovery
**Purpose**: Allow anyone to discover upcoming public one-time events

**Features**:
- List of upcoming public events
- Search by name, location, date
- Filter by format, date range, location
- Sort by date, popularity
- Event cards showing:
  - Event name and date
  - Course and location
  - Format and team size
  - Spots available
  - Registration status
  - "Register" button

**Access**: Public (no login required)

### 2. `/event/{key}` - Event Detail & Registration
**Purpose**: View event details and register teams

**Features**:
- Event header (name, date, organizer)
- Event details (description, format, rules)
- Course information
- Registration form (if open)
- Current team list (if public)
- Live leaderboard (during event)
- Event status badge

**Access**: Public for public events, code-protected for private events

### 3. `/dashboard` - Enhanced Dashboard
**Purpose**: Show both leagues and events for authenticated users

**New Sections**:
- "My Events" section showing:
  - Events I'm organizing (with quick actions)
  - Events I'm registered for (with countdown)
  - Past events
- "Create Event" button alongside "Create League"

**Access**: Authenticated users only

### 4. `/my-events` - My Events Page
**Purpose**: Manage all user's one-time events

**Features**:
- Tabs: "Organizing" | "Registered" | "Past"
- Event cards with quick actions
- Create new event button
- Search and filter

**Access**: Authenticated users only

### 5. `/event/{key}/manage` - Organizer Dashboard
**Purpose**: Manage event as organizer

**Features**:
- Event overview (stats, status)
- Team management (view, edit, delete)
- Check-in interface
- Live scoring monitor
- Results finalization
- Event settings
- Share event link

**Access**: Event organizer only

### 6. `/event/{key}/register` - Team Registration
**Purpose**: Register a team for an event

**Features**:
- Team information form
- Captain details
- Player roster (based on team size)
- Handicap entry (if required)
- Registration code (if private)
- Payment (future)
- Confirmation

**Access**: Public or code-protected

### 7. `/event/{key}/scorecard` - Mobile Scorecard
**Purpose**: Enter scores during the round

**Features**:
- Hole-by-hole score entry
- Current team position
- Live leaderboard
- Hole games tracking (future)
- Photo upload (future)

**Access**: Team members only

## Component Architecture

### Reusable Components

1. **EventCard** - Display event summary
2. **EventStatusBadge** - Show event status (Draft, Published, InProgress, etc.)
3. **EventFormatBadge** - Show format (Scramble, Stroke Play, etc.)
4. **TeamRoster** - Display team members
5. **Leaderboard** - Live standings
6. **RegistrationForm** - Team registration
7. **EventCreationWizard** - Multi-step event creation
8. **CheckInInterface** - Team check-in on event day

## Implementation Phases

### Phase 1: Foundation (COMPLETE ✅)
- [x] Database entities
- [x] Enums
- [x] Multi-round support
- [x] Migrations

### Phase 2: API Layer (NEXT)
- [ ] Create DTOs
- [ ] Implement services
- [ ] Build API controllers
- [ ] Add validation

### Phase 3: UI Components
- [ ] Public events discovery page
- [ ] Event detail/registration page
- [ ] Event creation wizard
- [ ] Organizer dashboard
- [ ] Dashboard integration

### Phase 4: Advanced Features (Future)
- [ ] Payment integration
- [ ] Live scoring with SignalR
- [ ] Hole games
- [ ] Photo uploads
- [ ] SMS notifications

## Next Steps

1. **Start Phase 2**: Build the API layer (DTOs, Services, Controllers)
2. **Test API**: Ensure all endpoints work correctly
3. **Build UI**: Create Blazor pages and components
4. **Integration**: Connect UI to API
5. **Testing**: End-to-end testing of user flows
6. **Polish**: Refine UI/UX based on feedback


