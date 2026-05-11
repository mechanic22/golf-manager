param(
    [switch]$StartApi
)

$ErrorActionPreference = "Stop"

Write-Host "========================================"
Write-Host "Reset Database and Import Holy Grail Data"
Write-Host "========================================"
Write-Host ""

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$apiDir = Join-Path $repoRoot "src/GolfManager.Api"
$backupFile = Join-Path $apiDir "DkGolf_Backup_202604270946.sql"

Set-Location $apiDir

$dbFiles = @(
    "GolfManager.db",
    "GolfManager.db-shm",
    "GolfManager.db-wal"
)

$deletedAny = $false
foreach ($file in $dbFiles) {
    if (Test-Path $file) {
        Remove-Item $file -Force
        $deletedAny = $true
    }
}

if ($deletedAny) {
    Write-Host "Deleted existing local database files."
} else {
    Write-Host "No existing local database files found."
}

if (-not (Test-Path $backupFile)) {
    Write-Host ""
    Write-Host "ERROR: Holy Grail backup file not found."
    Write-Host "Expected: $backupFile"
    exit 1
}

$size = (Get-Item $backupFile).Length
$sizeMb = [Math]::Round($size / 1MB, 2)

Write-Host ""
Write-Host "Holy Grail backup file found ($sizeMb MB)."
Write-Host ""
Write-Host "What to do next:"
Write-Host "1. Start API watcher: dotnet watch run (from src/GolfManager.Api)"
Write-Host "2. Import runs automatically on startup"
Write-Host "3. Login password for imported users: ChangeMe123!"
Write-Host "4. Open league URL: https://localhost:7213/league/dkgl"
Write-Host ""

if ($StartApi) {
    Write-Host "Starting API watcher..."
    dotnet watch run
}
