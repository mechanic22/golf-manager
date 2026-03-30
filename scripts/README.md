# Development Scripts

This folder contains PowerShell scripts to help manage the development servers.

## Problem: `dotnet watch` Hangs on Ctrl+C

When running `dotnet watch` for the Blazor WebAssembly project, pressing Ctrl+C often causes the process to hang instead of terminating cleanly. This leaves orphaned processes running on ports 7213 (Web) and 7012 (API), preventing you from starting the servers again.

## Solution: Kill Scripts

Use these scripts to forcefully terminate the development servers when they hang.

### Available Scripts

#### `kill-web-server.ps1`
Kills all processes using port 7213 (Blazor WebAssembly Dev Server)

```powershell
.\scripts\kill-web-server.ps1
```

#### `kill-api-server.ps1`
Kills all processes using port 7012 (API Server)

```powershell
.\scripts\kill-api-server.ps1
```

#### `kill-all-servers.ps1`
Kills all processes on both ports 7213 and 7012

```powershell
.\scripts\kill-all-servers.ps1
```

## Usage

### When `dotnet watch` Hangs

1. **Try Ctrl+C first** - Give it 5-10 seconds to respond
2. **If it hangs**, close the terminal window
3. **Run the kill script**:
   ```powershell
   cd GolfManager
   .\scripts\kill-all-servers.ps1
   ```
4. **Restart your dev servers**

### Quick Access

You can create aliases in your PowerShell profile for quick access:

```powershell
# Add to your PowerShell profile ($PROFILE)
function Kill-WebServer { & "F:\code\GolfManager\GolfManager\scripts\kill-web-server.ps1" }
function Kill-ApiServer { & "F:\code\GolfManager\GolfManager\scripts\kill-api-server.ps1" }
function Kill-AllServers { & "F:\code\GolfManager\GolfManager\scripts\kill-all-servers.ps1" }

Set-Alias kweb Kill-WebServer
Set-Alias kapi Kill-ApiServer
Set-Alias kall Kill-AllServers
```

Then you can just type `kall` from anywhere to kill all servers!

## Alternative: Use Task Manager

If the scripts don't work, you can manually kill the processes:

1. Open Task Manager (Ctrl+Shift+Esc)
2. Go to the "Details" tab
3. Find `dotnet.exe` processes
4. Right-click → End Task

## Why Does This Happen?

The Blazor WebAssembly Dev Server (`Microsoft.AspNetCore.Components.WebAssembly.DevServer`) runs as a child process of `dotnet watch`. When you press Ctrl+C:

1. The signal is sent to the parent `dotnet watch` process
2. The parent tries to gracefully shut down child processes
3. Sometimes the dev server doesn't respond to the shutdown signal
4. The parent process hangs waiting for the child to exit
5. You're left with orphaned processes holding the ports

This is a known issue with the Blazor WASM dev server and has been reported to Microsoft.

## Prevention Tips

1. **Always use Ctrl+C** instead of closing the terminal
2. **Wait 5-10 seconds** for the process to shut down
3. **If it hangs**, use the kill scripts instead of force-closing the terminal
4. **Consider using VS Code's built-in terminal** - it handles process cleanup better than external terminals

## Port Reference

- **7213**: Blazor WebAssembly Dev Server (HTTPS)
- **7012**: API Server (HTTPS)
- **5054**: API Server (HTTP)

