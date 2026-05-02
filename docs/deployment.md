# Deployment Guide

## Overview

GolfManager v2 supports multiple deployment scenarios from local development to production cloud environments. The application uses environment-based configuration for seamless transitions between environments.

## Prerequisites

- .NET 10 SDK
- Node.js 18+ and npm
- Database server (SQL Server for production, SQLite for development)

## Environment Configuration

### Environment Variables

Copy `.env.example` to `.env` and configure:

```bash
# Database
DatabaseProvider=SqlServer
ConnectionStrings__DefaultConnection=Server=your-server.database.windows.net,1433;Initial Catalog=GolfManager;...

# JWT
Jwt__SecretKey=YourSuperSecretKeyThatIsAtLeast32CharactersLongForHS256Algorithm
Jwt__Issuer=GolfManager
Jwt__Audience=GolfManager

# Application
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://+:443;http://+:80
```

### Database Setup

#### Production (SQL Server)
1. Create SQL Server database
2. Run EF Core migrations:
   ```bash
   cd src/GolfManager.Api
   dotnet ef database update
   ```

#### Development (SQLite)
- Database is created automatically on first run
- No additional setup required

## Build Process

### 1. Install Dependencies
```bash
# Restore .NET packages
dotnet restore

# Install Node.js dependencies
npm install
```

### 2. Build CSS
```bash
# Build optimized CSS
npm run build:css
```

### 3. Build Application
```bash
# Build for production
dotnet build --configuration Release

# Publish
dotnet publish src/GolfManager.Api/GolfManager.Api.csproj --configuration Release --output ./publish
```

## Deployment Options

### Docker Deployment

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/GolfManager.Api/GolfManager.Api.csproj", "src/GolfManager.Api/"]
RUN dotnet restore "src/GolfManager.Api/GolfManager.Api.csproj"
COPY . .
WORKDIR "/src/src/GolfManager.Api"
RUN dotnet build "GolfManager.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GolfManager.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GolfManager.Api.dll"]
```

### Azure App Service

1. Create App Service with .NET 10 runtime
2. Configure environment variables in Application Settings
3. Deploy using:
   - Azure DevOps pipelines
   - GitHub Actions
   - Azure CLI: `az webapp up`

### AWS ECS/Fargate

1. Build Docker image
2. Push to ECR
3. Deploy to ECS cluster with task definition

### Local Production

```bash
# Set environment
export ASPNETCORE_ENVIRONMENT=Production

# Run application
dotnet run --project src/GolfManager.Api --configuration Release
```

## Database Migration

### Automated Migration
```bash
# Apply pending migrations
cd scripts
./ApplyMigration.ps1
```

### Manual Migration
```bash
cd src/GolfManager.Api
dotnet ef database update
```

## SSL and Custom Domains

### SSL Configuration
- Automatic SSL provisioning for custom domains
- Azure Front Door or Cloudflare for SSL termination

### Custom Domain Setup
1. Add domain to league configuration
2. Configure DNS CNAME record
3. Enable SSL certificate provisioning

## Monitoring and Logging

### Application Insights (Azure)
```bash
APPINSIGHTS_INSTRUMENTATIONKEY=your-key-here
```

### Health Checks
- `/health` - Basic health check
- `/health/ready` - Readiness probe
- `/health/live` - Liveness probe

## Scaling Considerations

### Horizontal Scaling
- Stateless API design supports multiple instances
- Shared SQL Server database
- Redis for session/cache if needed

### Database Scaling
- Read replicas for reporting
- Connection pooling configured
- Query optimization with EF Core

## Backup and Recovery

### Database Backup
- Automated backups via Azure SQL/AWS RDS
- Point-in-time restore capability
- Cross-region replication for DR

### Application Backup
- Infrastructure as Code (Bicep/Terraform)
- Configuration stored in environment variables
- Container images versioned in registry

## Troubleshooting

### Common Issues

**Database Connection Failed**
- Verify connection string
- Check firewall rules
- Ensure database server is running

**SSL Certificate Issues**
- Verify domain ownership
- Check DNS propagation
- Review certificate authority logs

**Performance Issues**
- Monitor database query performance
- Check memory/CPU usage
- Review application logs</content>
<parameter name="filePath">/Users/tonygilbert/Projects/code/5thbox/dkgolf/golf-manager/docs/deployment.md