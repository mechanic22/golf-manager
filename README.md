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

### Frontend Stack
- **Blazor WebAssembly** - Web client application
- **Tailwind CSS** - Utility-first CSS framework for custom styling
- **FifthBox Material Components** - Material Design 3 component library
- **MAUI** (Future) - Cross-platform mobile application

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

### Prerequisites
- **.NET 10 SDK** or later
- **Node.js and npm** (for Tailwind CSS compilation)
- **SQL Server** or **SQLite** (for development)
- **Visual Studio 2022** or **VS Code** with C# extension

### Installation

1. **Clone the repository:**
   ```bash
   git clone <repository-url>
   cd GolfManager
   ```

2. **Install npm dependencies:**
   ```bash
   npm install
   ```

3. **Restore .NET packages:**
   ```bash
   dotnet restore
   ```

4. **Set up the database:**
   ```bash
   # Update connection string in src/GolfManager.Api/appsettings.json
   # Run migrations
   cd src/GolfManager.Api
   dotnet ef database update
   ```

5. **Build Tailwind CSS:**
   ```bash
   # From the root directory
   npm run build:css

   # Or watch for changes during development
   npm run watch:css
   ```

6. **Run the application:**
   ```bash
   # Terminal 1: Run the API
   cd src/GolfManager.Api
   dotnet run

   # Terminal 2: Run the Blazor WebAssembly client
   cd src/GolfManager.Web
   dotnet run
   ```

### Using FifthBox Material Components

The project uses [FifthBox Material Components](https://www.nuget.org/packages/FifthBox.MaterialComponents.Blazor/) for UI components following Material Design 3 specifications.

#### Installation
The package is already included in `GolfManager.Web.csproj`:
```xml
<PackageReference Include="FifthBox.MaterialComponents.Blazor" Version="1.0.13" />
```

#### Setup
The components are already configured in the project:

1. **CSS and JavaScript** are referenced in `wwwroot/index.html`:
   ```html
   <link href="_content/FifthBox.MaterialComponents.Blazor/css/material-components.css" rel="stylesheet" />
   <script src="_content/FifthBox.MaterialComponents.Blazor/js/material-components.js"></script>
   ```

2. **Namespaces** are imported in `_Imports.razor`:
   ```csharp
   @using MaterialComponents.UI
   @using MaterialComponents.Enums
   @using MaterialComponents.Icons
   ```

#### Available Components (20+)

**Form Components:**
- `MaterialInput` - Text inputs with filled/outlined variants
- `MaterialSelect` - Dropdown selects with templating
- `MaterialButton` - Buttons (Filled, Outlined, Text, Elevated, Tonal)
- `MaterialCheckbox` - Checkboxes with validation
- `MaterialRadio` - Radio buttons
- `MaterialSwitch` - Toggle switches
- `MaterialSlider` - Range sliders

**Layout Components:**
- `MaterialCard` - Cards (Elevated, Filled, Outlined)
- `MaterialContainer` - Responsive containers
- `MaterialSurface` - Surface containers with elevation
- `MaterialChip` - Chips (Assist, Filter, Input, Suggestion)
- `MaterialDivider` - Horizontal/vertical dividers

**Navigation Components:**
- `MaterialTabs` - Tab navigation
- `MaterialNavigationBar` - Bottom navigation
- `MaterialBreadcrumbs` - Breadcrumb navigation

**Feedback Components:**
- `MaterialDialog` - Modal dialogs
- `MaterialSnackbar` - Toast notifications
- `MaterialProgressIndicator` - Progress indicators
- `MaterialOffcanvas` - Slide-out panels

**Action Components:**
- `MaterialFab` - Floating action buttons
- `MaterialIconButton` - Icon-only buttons

**Data Display:**
- `MaterialTable` - Data tables with sorting/filtering

**Utilities:**
- `MaterialIcon` - Icon system with 30+ built-in icons
- `MaterialRipple` - Touch ripple effects

#### Basic Usage Example

```razor
@page "/example"

<MaterialContainer Size="MaterialContainerSize.Large">
    <MaterialCard Variant="MaterialCardVariant.Filled">
        <div class="md-card__content">
            <h2 class="md-card__title">League Dashboard</h2>
            <p class="md-card__subtitle">Manage your golf league</p>

            <MaterialInput @bind-Value="leagueName"
                          Label="League Name"
                          Variant="MaterialInputVariant.Outlined" />

            <MaterialButton Style="MaterialButtonStyle.Filled"
                           Text="Save League"
                           OnClick="HandleSave" />
        </div>
    </MaterialCard>
</MaterialContainer>

@code {
    private string leagueName = "";

    private void HandleSave()
    {
        // Handle save logic
    }
}
```

#### Theme Support
Material Components includes built-in light/dark theme support:
- Automatic system preference detection
- CSS variables for customization
- Accessible color contrast ratios

#### Documentation
- **Demo App:** See the [fifthbox-materialcomponents demo](../fifthbox-materialcomponents/demo) for interactive examples
- **NuGet Package:** [FifthBox.MaterialComponents.Blazor](https://www.nuget.org/packages/FifthBox.MaterialComponents.Blazor/)
- **Source Code:** Located in `../fifthbox-materialcomponents/` directory

## 🎨 UI/UX Guidelines

### Design Principles
- **Mobile-First:** All interfaces designed for mobile, then enhanced for desktop
- **Professional & Clean:** Modern, polished UI inspired by premium SaaS applications
- **Accessibility:** WCAG compliant with keyboard navigation and screen reader support
- **Responsive:** Seamless experience across all device sizes
- **CSS-First:** Prefer CSS solutions over JavaScript for styling and layout

### Styling Approach
- **Tailwind CSS:** Utility-first framework for custom layouts and spacing
- **Material Components:** Pre-built components for consistent Material Design 3 patterns
- **SVG Icons Only:** Clean, professional iconography
- **Theme Support:** Light/dark mode with system preference detection

### Component Usage Guidelines
1. **Use Material Components** for standard UI patterns (buttons, inputs, cards, dialogs)
2. **Use Tailwind** for layout, spacing, responsive design, and custom styling
3. **Combine both** for maximum flexibility and consistency
4. **Maintain accessibility** in all custom components

## 📖 Documentation

See the `/docs` folder for detailed documentation:
- [Architecture Overview](docs/architecture/overview.md)
- [API Design](docs/api/design.md)
- [Data Model](docs/architecture/data-model.md)
- [Multi-Tenancy Strategy](docs/architecture/multi-tenancy.md)

## 🛠️ Development Workflow

### Running in Development Mode

**Recommended Setup (3 terminals):**

```bash
# Terminal 1: API Server
cd src/GolfManager.Api
dotnet watch run

# Terminal 2: Blazor WebAssembly Client
cd src/GolfManager.Web
dotnet watch run

# Terminal 3: Tailwind CSS Watch
npm run watch:css
```

### Building for Production

```bash
# Build CSS
npm run build:css

# Build solution
dotnet build --configuration Release

# Publish Web client
cd src/GolfManager.Web
dotnet publish --configuration Release
```

### Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/GolfManager.Api.Tests

# Run with coverage
dotnet test /p:CollectCoverage=true
```

### Code Quality

- **Follow .NET naming conventions**
- **Use nullable reference types** (enabled by default)
- **Write XML documentation** for public APIs
- **Keep components focused** - single responsibility principle
- **Test business logic** in Services layer
- **Use DTOs** for API contracts (in GolfManager.Shared)

## 🔄 Migration from HolyGrail

This project is a v2 rewrite of the HolyGrail golf league application, focusing on:
- **Improved API design** - RESTful endpoints with clear contracts
- **Better separation of concerns** - Clean architecture with distinct layers
- **Multi-tenancy support** - Multiple leagues with custom domains
- **Enhanced real-time capabilities** - SignalR for live updates
- **Mobile-first approach** - Responsive design with Material Components
- **Modern tech stack** - .NET 10, Blazor WebAssembly, Material Design 3

## 🔗 Related Projects

This repository is part of a larger ecosystem:

- **[fifthbox-materialcomponents](../fifthbox-materialcomponents/)** - Material Design 3 component library
- **[fifthbox-appbase](../fifthbox-appbase/)** - Application base template with authentication
- **[holy-grail](../holy-grail/)** - Original golf league application (v1)

## 📚 Helpful Resources

### Documentation
- [.NET 10 Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [Blazor WebAssembly Guide](https://learn.microsoft.com/en-us/aspnet/core/blazor/)
- [Material Design 3 Guidelines](https://m3.material.io/)
- [Tailwind CSS Documentation](https://tailwindcss.com/docs)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)

### Component Libraries
- [FifthBox Material Components](https://www.nuget.org/packages/FifthBox.MaterialComponents.Blazor/)
- [Material Components Demo](../fifthbox-materialcomponents/demo)

### Tools
- [Visual Studio 2022](https://visualstudio.microsoft.com/)
- [VS Code](https://code.visualstudio.com/)
- [Postman](https://www.postman.com/) - API testing
- [DB Browser for SQLite](https://sqlitebrowser.org/) - Database inspection

## 🤝 Contributing

### Getting Started
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes following the code quality guidelines
4. Test your changes thoroughly
5. Commit your changes (`git commit -m 'Add amazing feature'`)
6. Push to the branch (`git push origin feature/amazing-feature`)
7. Open a Pull Request

### Development Guidelines
- Follow existing code style and conventions
- Write tests for new features
- Update documentation for API changes
- Ensure all tests pass before submitting PR
- Keep PRs focused on a single feature/fix

## 📝 License

(To be determined)

