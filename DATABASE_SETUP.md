# Database Configuration Guide

## 🎯 Overview

GolfManager v2 uses **environment-based database configuration**:
- **Development (Local)**: SQLite (file-based, no installation required)
- **Production**: SQL Server (Azure SQL, AWS RDS, or on-premises)
- **Testing**: In-Memory provider (for integration tests)

This approach gives you:
- ✅ **Zero setup** for local development
- ✅ **Production-grade** database for deployments
- ✅ **Same codebase** - no code changes between environments
- ✅ **Easy switching** - just change configuration

---

## 🖥️ Local Development (SQLite)

### Current Setup
- **Database Provider**: SQLite
- **Connection String**: `Data Source=GolfManager.db`
- **Database File**: `golf-manager/src/GolfManager.Api/GolfManager.db`
- **Configuration**: `appsettings.Development.json`

### How It Works
When you run the API in Development mode:
1. ✅ Reads `appsettings.Development.json`
2. ✅ Uses SQLite provider
3. ✅ Creates `GolfManager.db` file automatically
4. ✅ Seeds demo data on first run
5. ✅ No SQL Server installation needed!

### Running Locally
```bash
cd golf-manager/src/GolfManager.Api
dotnet run
```

The database is created automatically. You'll see:
```
✅ GolfManager.db created
✅ Demo data seeded (leagues, seasons, players, etc.)
✅ API running on https://localhost:7012
```

### Reset Local Database
To start fresh, just delete the database file:
```bash
cd golf-manager/src/GolfManager.Api
rm GolfManager.db
dotnet run  # Database will be recreated
```

---

## 🚀 Production Deployment (SQL Server)

### Supported SQL Server Options
- **Azure SQL Database** (recommended)
- **AWS RDS for SQL Server**
- **SQL Server on VM** (Azure/AWS/On-premises)
- **SQL Server Express** (free, for small deployments)

### Configuration

#### Option 1: Environment Variables (Recommended)
Set these environment variables in your hosting platform:

```bash
DatabaseProvider=SqlServer
ConnectionStrings__DefaultConnection="Server=your-server.database.windows.net;Database=GolfManager;User Id=your-user;Password=your-password;Encrypt=True"
```

#### Option 2: appsettings.Production.json
Update the connection string in `appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server;Database=GolfManager;User Id=your-user;Password=your-password;TrustServerCertificate=True"
  },
  "DatabaseProvider": "SqlServer"
}
```

⚠️ **Never commit production connection strings to source control!**

### Azure SQL Database Example

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:golfmanager.database.windows.net,1433;Initial Catalog=GolfManager;Persist Security Info=False;User ID=admin;Password=YourPassword123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  },
  "DatabaseProvider": "SqlServer"
}
```

### AWS RDS Example

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=golfmanager.abcd1234.us-east-1.rds.amazonaws.com;Database=GolfManager;User Id=admin;Password=YourPassword123!;Encrypt=True;TrustServerCertificate=True"
  },
  "DatabaseProvider": "SqlServer"
}
```

---

## 🔄 Database Migrations

### Create a New Migration
```bash
cd golf-manager/src/GolfManager.Api
dotnet ef migrations add YourMigrationName --project ../GolfManager.Data
```

### Apply Migrations (Development)
```bash
# SQLite (automatic on startup)
dotnet run
```

### Apply Migrations (Production)
```bash
# Set production environment
export ASPNETCORE_ENVIRONMENT=Production

# Apply migrations
dotnet ef database update --project ../GolfManager.Data
```

Or use the automated deployment script (see below).

---

## 📦 Deployment Checklist

### Before Deploying

1. ✅ **Create Production Database**
   - Azure: Create Azure SQL Database
   - AWS: Create RDS SQL Server instance
   - On-prem: Install SQL Server

2. ✅ **Configure Connection String**
   - Use environment variables (preferred)
   - Or update `appsettings.Production.json`

3. ✅ **Test Connection**
   ```bash
   # Test locally first
   export DatabaseProvider=SqlServer
   export ConnectionStrings__DefaultConnection="your-connection-string"
   dotnet run --environment=Production
   ```

4. ✅ **Apply Migrations**
   ```bash
   dotnet ef database update --project src/GolfManager.Data
   ```

5. ✅ **Verify Data**
   - Check that database schema is created
   - Verify migrations table exists
   - Optional: Seed production data

---

## 🛠️ Switching Database Providers

### Development: SQLite → SQL Server LocalDB

If you want to use SQL Server LocalDB for development (Windows only):

**appsettings.Development.json**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=GolfManager;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "DatabaseProvider": "SqlServer"
}
```

### Production: SQL Server → PostgreSQL

To use PostgreSQL instead:

1. Add package:
   ```bash
   dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
   ```

2. Update `Program.cs`:
   ```csharp
   else if (databaseProvider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
   {
       options.UseNpgsql(connectionString);
   }
   ```

3. Update connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=golfmanager;Username=postgres;Password=password"
     },
     "DatabaseProvider": "PostgreSQL"
   }
   ```

---

## 🔍 Troubleshooting

### SQLite: "Database is locked"
- **Cause**: Multiple processes accessing the database
- **Solution**: Close all instances of the API

### SQL Server: "Login failed"
- **Cause**: Incorrect credentials or firewall
- **Solution**: 
  - Verify username/password
  - Check firewall rules (Azure/AWS)
  - Verify SQL Server is running

### Migrations: "Pending model changes"
- **Cause**: Entity model changed without migration
- **Solution**:
  ```bash
  dotnet ef migrations add AddYourChanges --project src/GolfManager.Data
  dotnet ef database update --project src/GolfManager.Data
  ```

### Connection Timeout
- **Cause**: Database server unreachable or slow
- **Solution**: Increase timeout in connection string:
  ```
  ...;Connection Timeout=60
  ```

---

## 📊 Configuration Summary

| Environment | Provider | Connection String | Config File |
|-------------|----------|-------------------|-------------|
| **Development** | SQLite | `Data Source=GolfManager.db` | `appsettings.Development.json` |
| **Production** | SQL Server | `Server=...;Database=...` | `appsettings.Production.json` or ENV vars |
| **Testing** | In-Memory | N/A (code-configured) | N/A |

---

## 🔐 Security Best Practices

### ✅ DO:
- Use **environment variables** for production connection strings
- Use **Azure Key Vault** or **AWS Secrets Manager** for secrets
- Use **Managed Identity** (Azure) or **IAM Roles** (AWS) when possible
- Enable **SSL/TLS encryption** for database connections
- Use **least-privilege** database accounts

### ❌ DON'T:
- Commit connection strings to source control
- Use `sa` or admin accounts in production
- Use `TrustServerCertificate=True` in production (use valid certs)
- Share database credentials in plain text

---

## 🚀 Quick Start Commands

### Local Development
```bash
# Start development
cd golf-manager/src/GolfManager.Api
dotnet run

# Reset database
rm GolfManager.db
dotnet run
```

### Production Deployment
```bash
# Set environment
export ASPNETCORE_ENVIRONMENT=Production

# Apply migrations
dotnet ef database update --project src/GolfManager.Data

# Run application
dotnet run --configuration Release
```

### Docker Deployment
```dockerfile
# Dockerfile example
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY publish/ .

# Set environment
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DatabaseProvider=SqlServer

ENTRYPOINT ["dotnet", "GolfManager.Api.dll"]
```

Run with environment variables:
```bash
docker run -d \
  -p 5000:8080 \
  -e DatabaseProvider=SqlServer \
  -e ConnectionStrings__DefaultConnection="Server=..." \
  golfmanager-api:latest
```

---

## 📚 Additional Resources

- [Entity Framework Core Documentation](https://docs.microsoft.com/ef/core/)
- [Azure SQL Database](https://azure.microsoft.com/services/sql-database/)
- [AWS RDS for SQL Server](https://aws.amazon.com/rds/sqlserver/)
- [Connection Strings Reference](https://www.connectionstrings.com/sql-server/)
- [EF Core Migrations](https://docs.microsoft.com/ef/core/managing-schemas/migrations/)

---

## 🤝 Contributing

When contributing database changes:
1. ✅ Test with both SQLite AND SQL Server
2. ✅ Create migrations for schema changes
3. ✅ Update seed data if needed
4. ✅ Document any new configuration requirements
5. ✅ Ensure backward compatibility

---

**Need help?** Check the [main README](README.md) or open an issue on GitHub.
