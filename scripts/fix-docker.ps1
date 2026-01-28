# Fix Docker Desktop Script
# Helps diagnose and fix Docker Desktop issues

$ErrorActionPreference = "Continue"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Docker Desktop Troubleshooting" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if Docker Desktop process is running
Write-Host "1. Checking Docker Desktop process..." -ForegroundColor Yellow
$dockerProcess = Get-Process -Name "Docker Desktop" -ErrorAction SilentlyContinue
if ($dockerProcess) {
    Write-Host "   [OK] Docker Desktop process is running (PID: $($dockerProcess.Id))" -ForegroundColor Green
    Write-Host "   Started: $($dockerProcess.StartTime)" -ForegroundColor Gray
} else {
    Write-Host "   [X] Docker Desktop process is NOT running" -ForegroundColor Red
}

Write-Host ""

# Check if Docker daemon is accessible
Write-Host "2. Checking Docker daemon..." -ForegroundColor Yellow
try {
    $dockerCheck = docker ps 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   [OK] Docker daemon is accessible!" -ForegroundColor Green
    } else {
        Write-Host "   [X] Docker daemon is NOT accessible" -ForegroundColor Red
        Write-Host "   Error: $dockerCheck" -ForegroundColor Gray
    }
} catch {
    Write-Host "   [X] Docker daemon is NOT accessible" -ForegroundColor Red
    Write-Host "   Error: $_" -ForegroundColor Gray
}

Write-Host ""

# Check WSL 2
Write-Host "3. Checking WSL 2..." -ForegroundColor Yellow
try {
    $wslVersion = wsl --version 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   [OK] WSL is installed" -ForegroundColor Green
        $wslList = wsl --list --verbose 2>&1
        Write-Host "   $wslList" -ForegroundColor Gray
    } else {
        Write-Host "   [!] WSL may not be properly installed" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   [!] Could not check WSL" -ForegroundColor Yellow
}

Write-Host ""

# Offer to restart Docker Desktop
if ($dockerProcess) {
    Write-Host "4. Docker Desktop is running but not responding" -ForegroundColor Yellow
    if ($dockerProcess.Count -gt 1) {
        Write-Host "   [!] Multiple Docker Desktop processes detected - this may indicate a stuck state" -ForegroundColor Yellow
    }
    Write-Host ""
    Write-Host "   Options:" -ForegroundColor Cyan
    Write-Host "   1. Restart Docker Desktop (recommended)" -ForegroundColor White
    Write-Host "   2. Restart WSL 2 and Docker Desktop (if option 1 doesn't work)" -ForegroundColor White
    Write-Host "   3. Skip (manual troubleshooting)" -ForegroundColor White
    Write-Host ""
    $choice = Read-Host "   Enter choice (1/2/3)"
    
    if ($choice -eq "1" -or $choice -eq "2") {
        Write-Host ""
        Write-Host "   Stopping all Docker Desktop processes..." -ForegroundColor Yellow
        Get-Process -Name "Docker Desktop" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 3
        
        if ($choice -eq "2") {
            Write-Host "   Shutting down WSL 2..." -ForegroundColor Yellow
            wsl --shutdown 2>&1 | Out-Null
            Start-Sleep -Seconds 2
            Write-Host "   [OK] WSL 2 shut down" -ForegroundColor Green
        }
        
        Write-Host "   Starting Docker Desktop..." -ForegroundColor Yellow
        $dockerPaths = @(
            "${env:ProgramFiles}\Docker\Docker\Docker Desktop.exe",
            "${env:ProgramFiles(x86)}\Docker\Docker\Docker Desktop.exe",
            "$env:LOCALAPPDATA\Docker\Docker Desktop.exe"
        )
        
        $dockerExe = $null
        foreach ($path in $dockerPaths) {
            if (Test-Path $path) {
                $dockerExe = $path
                break
            }
        }
        
        if ($dockerExe) {
            Start-Process -FilePath $dockerExe
            Write-Host "   [OK] Docker Desktop restart initiated" -ForegroundColor Green
            Write-Host ""
            Write-Host "   Waiting for Docker Desktop to start (this may take 30-60 seconds)..." -ForegroundColor Yellow
            
            $maxWait = 90
            $waited = 0
            $ready = $false
            
            while (-not $ready -and $waited -lt $maxWait) {
                Start-Sleep -Seconds 3
                $waited += 3
                
                try {
                    $null = docker ps 2>&1 | Out-Null
                    if ($LASTEXITCODE -eq 0) {
                        $ready = $true
                        Write-Host "   [OK] Docker Desktop is now ready!" -ForegroundColor Green
                    }
                } catch {
                    # Continue waiting
                }
                
                if ($waited % 15 -eq 0 -and -not $ready) {
                    Write-Host "   Still waiting... ($waited seconds)" -ForegroundColor Gray
                }
            }
            
            if (-not $ready) {
                Write-Host "   [!] Docker Desktop did not become ready after $maxWait seconds" -ForegroundColor Yellow
                Write-Host "   Please check Docker Desktop window for errors" -ForegroundColor Yellow
            }
        } else {
            Write-Host "   [X] Could not find Docker Desktop executable" -ForegroundColor Red
        }
    }
} else {
    Write-Host "4. Docker Desktop is not running" -ForegroundColor Yellow
    Write-Host ""
    $start = Read-Host "   Would you like to start Docker Desktop? (y/n)"
    
    if ($start -eq "y" -or $start -eq "Y") {
        Write-Host ""
        Write-Host "   Starting Docker Desktop..." -ForegroundColor Yellow
        $dockerPaths = @(
            "${env:ProgramFiles}\Docker\Docker\Docker Desktop.exe",
            "${env:ProgramFiles(x86)}\Docker\Docker\Docker Desktop.exe",
            "$env:LOCALAPPDATA\Docker\Docker Desktop.exe"
        )
        
        $dockerExe = $null
        foreach ($path in $dockerPaths) {
            if (Test-Path $path) {
                $dockerExe = $path
                break
            }
        }
        
        if ($dockerExe) {
            Start-Process -FilePath $dockerExe
            Write-Host "   [OK] Docker Desktop start initiated" -ForegroundColor Green
        } else {
            Write-Host "   [X] Could not find Docker Desktop executable" -ForegroundColor Red
        }
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Troubleshooting Complete" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "If Docker Desktop still doesn't work:" -ForegroundColor Yellow
Write-Host "  1. Check Docker Desktop window for error messages" -ForegroundColor White
Write-Host "  2. Try: wsl --shutdown (then restart Docker Desktop)" -ForegroundColor White
Write-Host "  3. Check Windows Event Viewer for Docker errors" -ForegroundColor White
Write-Host "  4. As a last resort, restart your computer" -ForegroundColor White
Write-Host ""
