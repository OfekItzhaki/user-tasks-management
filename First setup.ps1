# First Setup Script
# This script automatically runs the Docker automated quick-start setup
# Double-click this file or run: .\First setup.ps1

$ErrorActionPreference = "Continue"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Task Management System - First Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "This will set up everything automatically using Docker." -ForegroundColor Yellow
Write-Host ""

# Get the script directory (where this file is located)
$scriptRoot = $PSScriptRoot
if (-not $scriptRoot) {
    $scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
}

# Path to the quick-start script
$quickStartScript = Join-Path $scriptRoot "scripts\quick-start\quick-start-docker-automated.ps1"

# Check if the quick-start script exists
if (-not (Test-Path $quickStartScript)) {
    Write-Host "[X] Quick-start script not found at: $quickStartScript" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please make sure you're running this from the project root folder." -ForegroundColor Yellow
    Write-Host "Expected location: scripts\quick-start\quick-start-docker-automated.ps1" -ForegroundColor Yellow
    Write-Host ""
    pause
    exit 1
}

Write-Host "Starting automated setup..." -ForegroundColor Green
Write-Host ""
Write-Host "Note: This may take several minutes on first run." -ForegroundColor Gray
Write-Host ""

# Run the quick-start script
try {
    & $quickStartScript
    $exitCode = $LASTEXITCODE
    
    if ($exitCode -eq 0) {
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "Setup completed successfully!" -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Green
    } else {
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Red
        Write-Host "Setup completed with errors (exit code: $exitCode)" -ForegroundColor Red
        Write-Host "========================================" -ForegroundColor Red
        Write-Host ""
        Write-Host "Please check the output above for error messages." -ForegroundColor Yellow
    }
} catch {
    Write-Host ""
    Write-Host "[X] Error running setup script: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please check the error message above and try again." -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "Press any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
