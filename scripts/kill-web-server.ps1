# Kill Blazor WebAssembly Dev Server
# This script kills all processes using port 7213 (the Web project dev server)

Write-Host "Checking for processes on port 7213..." -ForegroundColor Cyan

$connections = Get-NetTCPConnection -LocalPort 7213 -ErrorAction SilentlyContinue

if ($connections) {
    $processIds = $connections | Select-Object -ExpandProperty OwningProcess -Unique

    foreach ($procId in $processIds) {
        try {
            $process = Get-Process -Id $procId -ErrorAction SilentlyContinue
            if ($process) {
                Write-Host "Killing process: $($process.ProcessName) (PID: $procId)" -ForegroundColor Yellow
                Stop-Process -Id $procId -Force
                Write-Host "Process killed successfully" -ForegroundColor Green
            }
        }
        catch {
            Write-Host "Failed to kill process $procId : $_" -ForegroundColor Red
        }
    }
    
    # Wait a moment for the port to be released
    Start-Sleep -Seconds 1
    
    # Verify the port is free
    $stillUsed = Get-NetTCPConnection -LocalPort 7213 -ErrorAction SilentlyContinue
    if ($stillUsed) {
        Write-Host "Warning: Port 7213 is still in use!" -ForegroundColor Red
    }
    else {
        Write-Host "Port 7213 is now free!" -ForegroundColor Green
    }
}
else {
    Write-Host "No processes found on port 7213" -ForegroundColor Green
}

