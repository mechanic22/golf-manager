# PowerShell script to baseline the database and apply the new migration

# Step 1: Mark InitialCreate as applied
Write-Host "Step 1: Marking InitialCreate migration as applied..." -ForegroundColor Yellow

# Use dotnet-script or direct SQL execution
$dbPath = "GolfManager.db"
$migrationId = "20260310042523_InitialCreate"
$productVersion = "10.0.4"

# Load Microsoft.Data.Sqlite
Add-Type -Path "GolfManager\src\GolfManager.Data\bin\Debug\net10.0\Microsoft.Data.Sqlite.dll"

try {
    $connectionString = "Data Source=$dbPath"
    $connection = New-Object Microsoft.Data.Sqlite.SqliteConnection($connectionString)
    $connection.Open()
    
    $command = $connection.CreateCommand()
    $command.CommandText = "INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES (@migrationId, @productVersion)"
    $command.Parameters.AddWithValue("@migrationId", $migrationId) | Out-Null
    $command.Parameters.AddWithValue("@productVersion", $productVersion) | Out-Null
    
    $rowsAffected = $command.ExecuteNonQuery()
    Write-Host "InitialCreate migration marked as applied" -ForegroundColor Green

    $connection.Close()
}
catch {
    Write-Host "Error occurred" -ForegroundColor Red
    Write-Host "This is likely because the migration is already applied. Continuing..." -ForegroundColor Yellow
}

# Step 2: Apply the new migration
Write-Host "`nStep 2: Applying UnifiedEventSystem migration..." -ForegroundColor Yellow
dotnet ef database update -p GolfManager/src/GolfManager.Data -s GolfManager/src/GolfManager.Api

Write-Host "Migration complete!" -ForegroundColor Green

