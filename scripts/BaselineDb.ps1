# Simple PowerShell script to baseline the database
$dbPath = "GolfManager.db"

# Check if database exists
if (-not (Test-Path $dbPath)) {
    Write-Host "Database not found at $dbPath" -ForegroundColor Red
    exit 1
}

Write-Host "Baselining database..." -ForegroundColor Yellow

# Execute SQL using dotnet-script approach
$sql = @"
INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
VALUES ('20260310042523_InitialCreate', '10.0.4');
"@

# Save SQL to temp file
$tempSqlFile = "temp_baseline.sql"
$sql | Out-File -FilePath $tempSqlFile -Encoding UTF8

Write-Host "SQL command created. Please execute manually or use a SQLite tool." -ForegroundColor Yellow
Write-Host "SQL: $sql" -ForegroundColor Cyan

# Clean up
# Remove-Item $tempSqlFile

Write-Host "`nAlternatively, run this command:" -ForegroundColor Yellow
Write-Host "dotnet ef database update 20260315215940_UnifiedEventSystem -p GolfManager/src/GolfManager.Data -s GolfManager/src/GolfManager.Api" -ForegroundColor Cyan

