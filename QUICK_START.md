# GolfManager Quick Start Guide

## 🚀 Get Running in 5 Minutes

### 1. Prerequisites Check
```bash
# Verify .NET 10
dotnet --version  # Should be 10.x.x

# Verify Node.js
node --version    # Should be 18.x or higher
npm --version     # Should be 9.x or higher
```

### 2. Clone & Install
```bash
# Clone repository
git clone <repository-url>
cd GolfManager

# Install dependencies
npm install
dotnet restore
```

### 3. Database Setup
```bash
cd src/GolfManager.Api
dotnet ef database update
cd ../..
```

### 4. Run the Application

**Option A: Three Terminal Setup (Recommended)**
```bash
# Terminal 1: API
cd src/GolfManager.Api
dotnet watch run

# Terminal 2: Web Client
cd src/GolfManager.Web
dotnet watch run

# Terminal 3: CSS Watch
npm run watch:css
```

**Option B: Quick Start (Single Terminal)**
```bash
# Build CSS once
npm run build:css

# Run API (in background or separate terminal)
cd src/GolfManager.Api && dotnet run &

# Run Web Client
cd src/GolfManager.Web && dotnet run
```

### 5. Access the Application
- **Web Client**: https://localhost:5002
- **API**: https://localhost:5001
- **Swagger**: https://localhost:5001/swagger

## 📦 Material Components Quick Reference

### Import in Your Razor Component
```razor
@using MaterialComponents.UI
@using MaterialComponents.Enums
```

### Common Components

**Button**
```razor
<MaterialButton Style="MaterialButtonStyle.Filled" 
                Text="Click Me" 
                OnClick="HandleClick" />
```

**Input**
```razor
<MaterialInput @bind-Value="name" 
               Label="Name" 
               Variant="MaterialInputVariant.Outlined" />
```

**Card**
```razor
<MaterialCard Variant="MaterialCardVariant.Filled">
    <div class="md-card__content">
        <h2 class="md-card__title">Title</h2>
        <p class="md-card__subtitle">Subtitle</p>
    </div>
</MaterialCard>
```

**Container**
```razor
<MaterialContainer Size="MaterialContainerSize.Large">
    <!-- Your content -->
</MaterialContainer>
```

**Dialog**
```razor
<MaterialDialog @bind-IsOpen="showDialog" Title="Confirm">
    <p>Are you sure?</p>
    <MaterialButton Text="Yes" OnClick="HandleConfirm" />
</MaterialDialog>
```

## 🎨 Tailwind CSS Quick Reference

### Common Utilities
```html
<!-- Spacing -->
<div class="p-4 m-2 mt-6 mb-4">

<!-- Flexbox -->
<div class="flex items-center justify-between gap-4">

<!-- Grid -->
<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">

<!-- Responsive -->
<div class="w-full md:w-1/2 lg:w-1/3">

<!-- Colors -->
<div class="bg-white text-gray-900 border border-gray-200">
```

## 🔧 Common Tasks

### Add a New Page
```bash
# 1. Create page in src/GolfManager.Web/Pages/
# 2. Add route: @page "/my-page"
# 3. Add navigation link in Layout
```

### Add a New API Endpoint
```bash
# 1. Create controller in src/GolfManager.Api/Controllers/
# 2. Add service in src/GolfManager.Services/
# 3. Add DTOs in src/GolfManager.Shared/DTOs/
```

### Update Database Schema
```bash
cd src/GolfManager.Api
dotnet ef migrations add MigrationName
dotnet ef database update
```

### Run Tests
```bash
# All tests
dotnet test

# Specific project
dotnet test tests/GolfManager.Api.Tests

# With coverage
dotnet test /p:CollectCoverage=true
```

## 🐛 Troubleshooting

### CSS Not Updating
```bash
# Stop watch:css, then:
npm run build:css
npm run watch:css
```

### Port Already in Use
```bash
# Change ports in:
# - src/GolfManager.Api/Properties/launchSettings.json
# - src/GolfManager.Web/Properties/launchSettings.json
```

### Database Issues
```bash
# Reset database (dev only!)
cd src/GolfManager.Api
dotnet ef database drop
dotnet ef database update
```

## 📚 Next Steps

1. Read the full [README.md](README.md)
2. Check out [Material Components Demo](../fifthbox-materialcomponents/demo)
3. Review [API Documentation](docs/api/design.md)
4. See [Demo Credentials](DEMO_CREDENTIALS.md)

## 🆘 Need Help?

- Check [README_IMPROVEMENTS.md](README_IMPROVEMENTS.md) for detailed suggestions
- Review [docs/](docs/) folder for architecture details
- See Material Components examples in demo app

