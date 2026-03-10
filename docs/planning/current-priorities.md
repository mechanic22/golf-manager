# GolfManager v2 - Current Priorities

## 🎯 Focus: League Management Backend

**Primary Goal**: Build a comprehensive backend API for golf league management

**Deferred**: Mobile app features (GPS, hole detection) - documented for future implementation

---

## ⭐ Phase 1: Foundation (CURRENT PRIORITY)

### 1.1 Project Setup
- [ ] Create .NET solution structure
  - GolfManager.Api
  - GolfManager.Core
  - GolfManager.Data
  - GolfManager.Services
  - GolfManager.Shared
- [ ] Configure NuGet packages
- [ ] Set up solution-level configuration

### 1.2 Core Data Models
- [ ] User entity
- [ ] Golfer entity (global)
- [ ] League entity (with custom domain fields)
- [ ] LeagueGolfer entity (league membership)
- [ ] UserLeague entity
- [ ] RefreshToken entity
- [ ] Base interfaces (IEntity, IAuditable, ISoftDelete)
- [ ] Enums (HandicapType, ScoringTypes, etc.)

### 1.3 Database Setup
- [ ] Configure DbContext with multi-tenancy
- [ ] Entity configurations and relationships
- [ ] Audit interceptors (CreatedAt, UpdatedAt)
- [ ] Query filters for tenant isolation
- [ ] Initial migration
- [ ] Database seeding (test data)
- [ ] Indexes for performance

### 1.4 Authentication & Authorization
- [ ] JWT token service
- [ ] User registration endpoint
- [ ] Login endpoint
- [ ] Refresh token rotation
- [ ] Password reset flow
- [ ] Authorization policies (LeagueAdmin, GlobalAdmin)
- [ ] Current user/league context service

### 1.5 API Infrastructure
- [ ] API versioning (/api/v1/...)
- [ ] Swagger/OpenAPI configuration
- [ ] Global error handling middleware
- [ ] Standard response wrapper
- [ ] Request/response logging
- [ ] CORS configuration
- [ ] Health checks

---

## ⭐ Phase 2: League Management (NEXT PRIORITY)

### 2.1 League Core
- [ ] League DTOs/ViewModels
- [ ] League repository
- [ ] League service
- [ ] League API endpoints (CRUD)
- [ ] League member management
- [ ] League settings

### 2.2 Custom Domain Support (Basic)
- [ ] Custom domain fields in League entity
- [ ] Set custom domain endpoint
- [ ] Domain verification endpoint (DNS TXT record)
- [ ] Domain resolution middleware
- [ ] Basic SSL support (manual for now)

### 2.3 User & Golfer Management
- [ ] User profile endpoints
- [ ] Golfer profile creation
- [ ] Golfer global stats endpoints
- [ ] League golfer endpoints
- [ ] User roles and permissions

### 2.4 Season Management
- [ ] Season entity and DTOs
- [ ] SeasonSettings entity
- [ ] Season CRUD endpoints
- [ ] Season lock/unlock functionality
- [ ] Active season management

### 2.5 Team Management
- [ ] SeasonTeam entity
- [ ] Team CRUD endpoints
- [ ] Team member assignment
- [ ] Team standings (basic)

---

## ⭐ Phase 3: Course Management

### 3.1 Course Core
- [ ] Course entity (global, not tenant-specific)
- [ ] Tee entity
- [ ] Hole entity
- [ ] HoleTee entity
- [ ] Course repository
- [ ] Course service

### 3.2 Course API
- [ ] Course CRUD endpoints
- [ ] Tee management endpoints
- [ ] Hole configuration endpoints
- [ ] Course search/filtering

### 3.3 Course Data
- [ ] Basic hole information (par, yardage, handicap)
- [ ] Rating and slope data
- [ ] Multiple tee support
- [ ] **GPS fields added to schema (for future use)**
- [ ] **GPS endpoints stubbed (not implemented yet)**

---

## ⭐ Phase 4: Event & Scoring

### 4.1 Event Management
- [ ] SeasonEvent entity
- [ ] SeasonEventGolfer entity
- [ ] SeasonEventMatch entity
- [ ] Event CRUD endpoints
- [ ] Event golfer participation
- [ ] Match pairing logic

### 4.2 Round & Scoring
- [ ] Round entity
- [ ] RoundHole entity
- [ ] Scorecard entity
- [ ] Round CRUD endpoints
- [ ] Score entry endpoints
- [ ] Batch score update
- [ ] Score validation

### 4.3 Handicap Calculation
- [ ] Handicap calculation service
- [ ] Bob's Method algorithm
- [ ] USGA handicap (basic)
- [ ] Background processing queue
- [ ] Handicap history tracking

---

## ⭐ Phase 5: Advanced Scoring

### 5.1 Scoring Systems
- [ ] Match Play scoring
- [ ] Stableford scoring
- [ ] Two-Point scoring
- [ ] Team scoring calculations
- [ ] Missing player handling

### 5.2 Statistics & Leaderboards
- [ ] Individual leaderboard
- [ ] Team leaderboard
- [ ] Event leaderboard
- [ ] Golfer statistics service
- [ ] Season statistics

---

## ⭐ Phase 6: Real-time Features

### 6.1 SignalR Setup
- [ ] ScoreHub
- [ ] EventHub
- [ ] LeagueHub
- [ ] Hub authentication
- [ ] Connection management

### 6.2 Notifications
- [ ] Score update broadcasting
- [ ] Leaderboard updates
- [ ] Event notifications
- [ ] Match completion alerts

---

## 🔮 Future Phases (Documented, Not Implemented Yet)

### Phase 7: One-Time Tournament Events
- OneTimeEvent entity and data model
- Event creation and management
- Team registration (public/private)
- Real-time scoring for tournaments
- Tournament leaderboards
- Hole games (closest to pin, longest drive)
- Pay-per-event payment processing
- Tournament formats (scramble, best ball, etc.)

### Phase 8: Mobile App Features
- GPS hole detection
- Auto-populate tee club
- Distance calculation
- Offline support
- Shot tracking

### Phase 9: Financial Management
- Stripe Connect integration
- League subscription management
- Golfer payment processing
- League dues and event fees
- Financial reporting
- Payout management
- Platform fee collection

### Phase 10: Advanced Features
- Advanced custom domain (white-label)
- Advanced analytics
- Reporting and exports
- Third-party integrations
- Accounting software integration

---

## 📝 Notes

- **GPS Features**: Data model includes GPS fields, but implementation is deferred
- **Custom Domains**: Basic support in Phase 2, advanced features later
- **Mobile App**: MAUI app development starts after backend is stable
- **Frontend**: Blazor WebAssembly can start in parallel with Phase 4-5

---

## ✅ Success Criteria for Phase 1

- [ ] Solution builds without errors
- [ ] Database migrations run successfully
- [ ] User can register and login
- [ ] JWT tokens are issued and validated
- [ ] Swagger documentation is accessible
- [ ] Health check endpoint responds
- [ ] Multi-tenancy context is set correctly

