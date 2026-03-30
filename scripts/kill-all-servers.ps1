# Kill All Development Servers
# This script kills all processes for both the Web (7213) and API (7012) servers

Write-Host "=== Killing All Development Servers ===" -ForegroundColor Cyan
Write-Host ""

# Kill Web Server (port 7213)
Write-Host "Checking Web Server (port 7213)..." -ForegroundColor Cyan
$webConnections = Get-NetTCPConnection -LocalPort 7213 -ErrorAction SilentlyContinue

if ($webConnections) {
    $processIds = $webConnections | Select-Object -ExpandProperty OwningProcess -Unique
    foreach ($procId in $processIds) {
        try {
            $process = Get-Process -Id $procId -ErrorAction SilentlyContinue
            if ($process) {
                Write-Host "  Killing: $($process.ProcessName) (PID: $procId)" -ForegroundColor Yellow
                Stop-Process -Id $procId -Force
                Write-Host "  [OK] Killed successfully" -ForegroundColor Green
            }
        }
        catch {
            Write-Host "  [X] Failed to kill process $procId" -ForegroundColor Red
        }
    }
}
else {
    Write-Host "  No processes on port 7213" -ForegroundColor Gray
}

Write-Host ""

# Kill API Server (port 7012)
Write-Host "Checking API Server (port 7012)..." -ForegroundColor Cyan
$apiConnections = Get-NetTCPConnection -LocalPort 7012 -ErrorAction SilentlyContinue

if ($apiConnections) {
    $processIds = $apiConnections | Select-Object -ExpandProperty OwningProcess -Unique
    foreach ($procId in $processIds) {
        try {
            $process = Get-Process -Id $procId -ErrorAction SilentlyContinue
            if ($process) {
                Write-Host "  Killing: $($process.ProcessName) (PID: $procId)" -ForegroundColor Yellow
                Stop-Process -Id $procId -Force
                Write-Host "  [OK] Killed successfully" -ForegroundColor Green
            }
        }
        catch {
            Write-Host "  [X] Failed to kill process $procId" -ForegroundColor Red
        }
    }
}
else {
    Write-Host "  No processes on port 7012" -ForegroundColor Gray
}

Write-Host ""

# Wait for ports to be released
Start-Sleep -Seconds 1

# Verify ports are free
Write-Host "Verifying ports..." -ForegroundColor Cyan
$webStillUsed = Get-NetTCPConnection -LocalPort 7213 -ErrorAction SilentlyContinue
$apiStillUsed = Get-NetTCPConnection -LocalPort 7012 -ErrorAction SilentlyContinue

if ($webStillUsed) {
    Write-Host "  [X] Port 7213 is still in use!" -ForegroundColor Red
}
else {
    Write-Host "  [OK] Port 7213 is free" -ForegroundColor Green
}

if ($apiStillUsed) {
    Write-Host "  [X] Port 7012 is still in use!" -ForegroundColor Red
}
else {
    Write-Host "  [OK] Port 7012 is free" -ForegroundColor Green
}

Write-Host ""
Write-Host "=== Done ===" -ForegroundColor Cyan

