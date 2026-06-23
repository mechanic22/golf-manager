$ErrorActionPreference = "Stop"

Write-Host "Dropping and recreating GolfManager database..."
docker exec golfmanager-postgres psql -U postgres -c "DROP DATABASE IF EXISTS golfmanager;"
docker exec golfmanager-postgres psql -U postgres -c "CREATE DATABASE golfmanager;"

Write-Host ""
Write-Host "Done. Run 'dotnet watch run --project src/GolfManager.Api' to apply migrations and seed data."
