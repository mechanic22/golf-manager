# GolfManager v2 - Project Plan

## 📋 Overview

This document outlines the comprehensive plan for building GolfManager v2, a modern, API-first golf league management system.

## 🎯 Goals

1. **API-First Design**: Build a comprehensive REST API that can serve multiple client types
2. **Multi-Tenancy**: Support multiple leagues with users participating across leagues
3. **Real-time Updates**: Implement SignalR for live scoring and notifications
4. **Improved Data Models**: Refine data structures based on HolyGrail learnings
5. **Background Processing**: Efficient handicap calculation and score processing
6. **Modern Auth**: JWT-based authentication with refresh tokens
7. **Scalability**: Cloud-ready architecture

## 📊 Project Phases

### Phase 1: Foundation & Core API (Weeks 1-3)
- [ ] Project structure setup
- [ ] Core data models and EF Core configuration
- [ ] Database design and migrations
- [ ] Authentication & authorization infrastructure
- [ ] Multi-tenancy implementation
- [ ] Base API controllers and endpoints

### Phase 2: League & Season Management (Weeks 4-5)
- [ ] League CRUD operations
- [ ] Season management APIs
- [ ] Season settings and configuration
- [ ] Event scheduling APIs
- [ ] Team management APIs

### Phase 3: Player & Scoring (Weeks 6-8)
- [ ] Golfer management APIs
- [ ] Round and scorecard APIs
- [ ] Score entry endpoints
- [ ] Handicap calculation service
- [ ] Background processing queue

### Phase 4: Course Management (Week 9)
- [ ] Course CRUD operations
- [ ] Tee management
- [ ] Hole configuration
- [ ] Course rating data

### Phase 5: Real-time Features (Week 10)
- [ ] SignalR hub setup
- [ ] Live score updates
- [ ] Event notifications
- [ ] Leaderboard broadcasting

### Phase 6: Advanced Features (Weeks 11-12)
- [ ] Match scoring calculations
- [ ] Team scoring algorithms
- [ ] Statistics and analytics APIs
- [ ] Reporting endpoints

### Phase 7: Client Applications (Weeks 13+)
- [ ] Blazor WebAssembly client
- [ ] MAUI mobile app
- [ ] Admin dashboards

## 🗄️ Data Model Improvements

### From HolyGrail Analysis

**Strengths to Keep:**
- Separation of Course, Tee, Hole, HoleTee structure
- Round and RoundHole granularity
- Season/SeasonEvent/SeasonEventGolfer hierarchy
- Flexible scoring settings

**Improvements to Make:**
1. **Multi-Tenancy**
   - Add TenantId to all entities
   - Implement tenant isolation at data layer
   - User-League-Role mapping table

2. **User Management**
   - Separate User from Golfer concept
   - User can have multiple Golfer profiles (one per league)
   - Enhanced profile information

3. **Audit Trail**
   - Add CreatedAt, UpdatedAt, CreatedBy, UpdatedBy to all entities
   - Soft delete support
   - Change tracking

4. **Performance**
   - Add strategic indexes
   - Computed columns for common aggregations
   - Materialized views for leaderboards

## 🔐 Authentication & Multi-Tenancy

### User Model
```
User (Global)
├── Email (unique)
├── PasswordHash
├── FirstName, LastName
├── IsGlobalAdmin
└── UserLeagues[]
    ├── LeagueId
    ├── GolferId (league-specific profile)
    ├── IsLeagueAdmin
    └── Roles[]
```

### Multi-Tenancy Strategy
- **League = Tenant**: Each league is a separate tenant
- **Shared Database**: Single database with TenantId (LeagueId) on all entities
- **Row-Level Security**: Query filters ensure data isolation
- **User Context**: JWT contains user's leagues and current league context

## 🔄 Background Processing

### Handicap Calculation Queue
- Event-driven: Triggered on score entry/update
- Queued processing to avoid blocking API calls
- Configurable calculation methods (Bob's, USGA, Scratch)
- Historical round consideration

### Score Processing
- Match point calculations
- Team score aggregations
- Leaderboard updates
- Statistics computation

## 📡 SignalR Hubs

### Planned Hubs

1. **ScoreHub**
   - Live score updates
   - Round completion notifications
   - Leaderboard changes

2. **EventHub**
   - Event start/end notifications
   - Match status updates
   - Player check-ins

3. **LeagueHub**
   - League-wide announcements
   - Season updates
   - Administrative notifications

## 🎨 API Design Principles

1. **RESTful**: Follow REST conventions
2. **Versioned**: API versioning from day one
3. **Consistent**: Standard response formats
4. **Documented**: OpenAPI/Swagger documentation
5. **Validated**: Input validation with FluentValidation
6. **Paginated**: Support for large datasets
7. **Filtered**: Query parameter filtering
8. **Sorted**: Flexible sorting options

## 📦 ViewModels & DTOs

### Naming Convention
- **Request**: `{Entity}{Action}Request` (e.g., `SeasonCreateRequest`)
- **Response**: `{Entity}ViewModel` (e.g., `SeasonViewModel`)
- **List Items**: `{Entity}ListItemViewModel` (e.g., `SeasonListItemViewModel`)

### Key ViewModels to Design
- LeagueViewModel, LeagueListItemViewModel
- SeasonViewModel, SeasonCreateRequest, SeasonUpdateRequest
- EventViewModel, EventCreateRequest, EventUpdateRequest
- GolferViewModel, GolferProfileViewModel
- RoundViewModel, ScoreEntryRequest
- TeamViewModel, TeamCreateRequest
- CourseViewModel, TeeViewModel

## 🧪 Testing Strategy

1. **Unit Tests**: Services and business logic
2. **Integration Tests**: API endpoints with test database
3. **E2E Tests**: Critical user flows
4. **Performance Tests**: Load testing for concurrent scoring

## 🚀 Deployment Strategy

- **Containerized**: Docker support
- **Cloud-Ready**: Azure/AWS compatible
- **CI/CD**: GitHub Actions
- **Database Migrations**: Automated with EF Core
- **Environment Configs**: Development, Staging, Production

## 📝 Next Steps

1. Review and refine this plan
2. Create detailed data model diagrams
3. Design API endpoint structure
4. Set up initial project structure
5. Begin Phase 1 implementation

