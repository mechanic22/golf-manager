# GolfManager v2

A comprehensive, multi-tenant Golf League Management system built with .NET Core, designed as the next generation of the HolyGrail golf league application.

## 🎯 Project Vision

GolfManager v2 is a complete rewrite focusing on:
- **API-First Architecture**: Comprehensive REST API backend
- **Multi-Tenancy**: Support for multiple leagues with users participating in multiple leagues
- **Custom Domains**: Leagues can use their own branded domains (e.g., `digikeygolf.com`)
- **GPS-Enabled Mobile**: Auto-detect holes and suggest clubs using GPS location
- **Real-time Updates**: SignalR-based push notifications for live scoring and updates
- **Modern Frontend Options**: Support for Blazor WebAssembly and MAUI mobile apps
- **Scalability**: Cloud-ready architecture with background processing

## 🏗️ Architecture

### Backend Stack
- **.NET 10** - Core framework
- **ASP.NET Core Web API** - RESTful API endpoints
- **Entity Framework Core** - Data access layer
- **SignalR** - Real-time push notifications
- **SQL Server** - Primary database
- **Background Services** - Handicap calculation, score processing

### Frontend Options
- **Blazor WebAssembly** - Web client with Tailwind CSS
- **MAUI** - Cross-platform mobile application
- **Material Components** - UI component library (FifthBox.MaterialComponents)

### Authentication & Authorization
- **JWT-based authentication** - Leveraging fifthbox-appbase patterns
- **Multi-tenant user management** - Users can belong to multiple leagues
- **Role-based authorization** - League admins, global admins, players

## 📋 Core Features

### League Management
- Multi-tenant league support
- Season management with configurable settings
- Event scheduling and management
- Team creation and management

### Player Management
- Golfer profiles and statistics
- Handicap tracking (multiple systems: Bob's, USGA, Scratch)
- Performance analytics
- Historical round data

### Scoring System
- Real-time score entry
- Multiple scoring types (Match Play, Stableford, Two-Point)
- Automated handicap calculations
- Team and individual scoring

### Course Management
- Course database with tee information
- Hole-by-hole details with GPS coordinates
- Rating and slope data
- Multiple tee support per course
- GPS-based hole detection for mobile app
- Automatic distance calculation to green
- Club suggestions based on distance

### Real-time Features (SignalR)
- Live score updates
- Event notifications
- Leaderboard updates
- Match status changes

### Multi-Tenancy & Branding
- Custom domain support per league
- DNS verification and SSL provisioning
- Subdomain pattern (`league.golfmanager.app`)
- Custom domain pattern (`digikeygolf.com`)

### One-Time Tournament Events (Future)
- Standalone tournaments without league requirement
- Pay-per-event model ($49-$199 per tournament)
- Team-based formats (scramble, best ball, etc.)
- Public or private event registration
- Real-time scoring and leaderboards
- Hole games (closest to pin, longest drive)
- Entry fee collection
- Mobile score entry for teams
- Perfect for charity scrambles and corporate outings

### Mobile App Features (Future)
- GPS-based hole detection
- Auto-populate tee club and distance
- Real-time distance to green
- Offline course data
- Shot tracking and statistics

### Financial Management (Future)
- League subscription management (tiered pricing)
- Online payment processing for league dues
- Event fee collection
- Golfer balance tracking
- Automated invoicing and receipts
- Payout management for leagues
- Financial reporting and analytics
- Stripe integration (Connect + Subscriptions)

## 🗂️ Project Structure

```
GolfManager/
├── src/
│   ├── GolfManager.Api/              # Main API project
│   ├── GolfManager.Core/             # Domain models and interfaces
│   ├── GolfManager.Data/             # EF Core, repositories, migrations
│   ├── GolfManager.Services/         # Business logic and services
│   ├── GolfManager.Hubs/             # SignalR hubs
│   └── GolfManager.Shared/           # Shared models, DTOs, ViewModels
├── clients/
│   ├── GolfManager.Web/              # Blazor WebAssembly client
│   └── GolfManager.Mobile/           # MAUI mobile app
├── tests/
│   ├── GolfManager.Api.Tests/
│   ├── GolfManager.Services.Tests/
│   └── GolfManager.Data.Tests/
└── docs/
    ├── api/                          # API documentation
    ├── architecture/                 # Architecture decisions
    └── planning/                     # Planning documents
```

## 🚀 Getting Started

(To be added during implementation)

## 📖 Documentation

See the `/docs` folder for detailed documentation:
- [Architecture Overview](docs/architecture/overview.md)
- [API Design](docs/api/design.md)
- [Data Model](docs/architecture/data-model.md)
- [Multi-Tenancy Strategy](docs/architecture/multi-tenancy.md)

## 🔄 Migration from HolyGrail

This project is a v2 rewrite of the HolyGrail golf league application, focusing on:
- Improved API design
- Better separation of concerns
- Multi-tenancy support
- Enhanced real-time capabilities
- Mobile-first approach

## 📝 License

(To be determined)

