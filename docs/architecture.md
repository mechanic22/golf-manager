# Architecture Overview

## System Overview

GolfManager v2 is a comprehensive, multi-tenant golf league management system built with modern .NET technologies. The application provides end-to-end golf league management with real-time scoring, handicap tracking, and mobile GPS integration.

## Core Architecture

### Backend (.NET 10)
- **API Layer**: ASP.NET Core Web API with RESTful endpoints
- **Data Layer**: Entity Framework Core with multi-provider database support (SQLite for dev, SQL Server for prod)
- **Service Layer**: Business logic services for leagues, players, scoring, and handicaps
- **Real-time Layer**: SignalR for live score updates and notifications

### Frontend (Blazor WebAssembly)
- **Web Client**: Blazor WASM application using Material Design 3 components
- **Styling**: Tailwind CSS with custom Material Components library
- **State Management**: Client-side state with API integration

### Database Architecture
- **Multi-tenant Design**: Leagues as top-level entities with isolated data
- **Provider Flexibility**: SQLite (development), SQL Server (production), In-Memory (testing)
- **Migration System**: EF Core migrations with automated deployment

## Data Flow

```
User Request → API Controller → Service Layer → Data Access → Database
                      ↓
              SignalR Hub → Real-time Updates → Client
```

## Key Components

### League Management
- Multi-tenant league support with custom domains
- Season and event scheduling
- Team creation and player management

### Scoring System
- Real-time score entry with multiple scoring types
- Automated handicap calculations (Bob's, USGA, Scratch)
- Team and individual scoring modes

### Course Management
- GPS-enabled course database with hole coordinates
- Multiple tee support with rating/slope data
- Mobile app integration for distance calculation

### Authentication & Security
- JWT-based authentication with refresh tokens
- Role-based authorization (Global Admin, League Admin, Player)
- Multi-tenant user management

## Deployment Architecture

### Development Environment
- Local SQLite database (no setup required)
- Hot reload for API and web client
- CSS watch mode for styling changes

### Production Environment
- SQL Server database (Azure SQL/AWS RDS)
- Docker containerization support
- Environment-based configuration
- SSL and custom domain support

## Future Extensions

- **Mobile App**: MAUI-based cross-platform mobile application
- **One-Time Events**: Standalone tournaments with pay-per-event model
- **Financial Management**: Subscription and payment processing
- **Advanced Analytics**: Performance tracking and statistics</content>
<parameter name="filePath">/Users/tonygilbert/Projects/code/5thbox/dkgolf/golf-manager/docs/architecture.md