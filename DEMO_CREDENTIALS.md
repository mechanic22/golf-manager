# GolfManager Demo Credentials

## 💾 Database Configuration

**Database Type**: SQLite (file-based, no installation required!)
**Database File**: `GolfManager.db` (created automatically in the API project directory)
**Connection String**: `Data Source=GolfManager.db`

### Why SQLite?
- ✅ **No Installation Required**: Works out of the box
- ✅ **File-Based**: Database is a single file
- ✅ **Perfect for Development**: Easy to reset (just delete the file)
- ✅ **Cross-Platform**: Works on Windows, Mac, Linux

## Overview
When running the application in **Development** mode, the database is automatically seeded with demo data on first startup.

## Demo Users

### Admin User
- **Email**: `admin@golfmanager.com`
- **Password**: `Admin123!`
- **Role**: Global Admin + League Admin
- **Permissions**: Full access to all features

### Regular User
- **Email**: `demo@golfmanager.com`
- **Password**: `Demo123!`
- **Role**: League Member
- **Permissions**: Standard user access

## Demo League

### League Details
- **Name**: Demo Golf League
- **Key**: `demo-league`
- **Description**: A demo league for testing GolfManager
- **Members**: Both admin and demo users are members

## How to Use

### First Time Setup
1. **Start the API**:
   ```bash
   cd GolfManager
   dotnet run --project src/GolfManager.Api/GolfManager.Api.csproj
   ```
   The database will be automatically created and seeded on first run.

2. **Start the Web UI**:
   ```bash
   cd GolfManager
   dotnet run --project src/GolfManager.Web/GolfManager.Web.csproj
   ```

3. **Login**:
   - Navigate to the web UI (typically `https://localhost:5001`)
   - Click "Login"
   - Use one of the demo credentials above

### Testing Features

#### As Admin User (`admin@golfmanager.com`)
- ✅ Create new leagues
- ✅ Manage existing leagues
- ✅ Add/remove players
- ✅ Create seasons and events
- ✅ Record scores
- ✅ View all league data

#### As Regular User (`demo@golfmanager.com`)
- ✅ View leagues they're a member of
- ✅ Join new leagues
- ✅ View their own scores
- ✅ Participate in events
- ❌ Cannot manage league settings (not an admin)

## Database Reset

If you want to reset the demo data:

1. **Delete the SQLite database file**:
   ```bash
   # Stop the API first, then delete the database file
   rm GolfManager.db
   # Or on Windows PowerShell:
   # Remove-Item GolfManager.db
   ```

2. **Restart the API**:
   - The seeder will automatically recreate the database and demo data

## Production Note

⚠️ **Important**: The automatic seeding only runs in **Development** environment. In production, you should:
- Use proper user registration
- Set up admin users manually
- Configure proper password policies
- Use secure password hashing (the demo uses a simple SHA256 hash)

## Next Steps

After logging in with a demo account, try:
1. Creating a new league
2. Adding players to the league
3. Creating a season
4. Adding events to the season
5. Recording round scores
6. Viewing leaderboards and statistics

