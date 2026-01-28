# First Setup Script
# This script prompts the user to choose between Docker or Local setup
# Double-click "First setup.bat" or run: .\First setup.ps1

$ErrorActionPreference = "Continue"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Task Management System - First Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Choose your setup method:" -ForegroundColor Yellow
Write-Host ""
Write-Host "  [1] Docker (Recommended)" -ForegroundColor White
Write-Host "      - Uses Docker for SQL Server and RabbitMQ" -ForegroundColor Gray
Write-Host "      - Easiest setup, no local database installation needed" -ForegroundColor Gray
Write-Host ""
Write-Host "  [2] Local (Without Docker)" -ForegroundColor White
Write-Host "      - Uses LocalDB for SQL Server" -ForegroundColor Gray
Write-Host "      - Requires LocalDB to be installed" -ForegroundColor Gray
Write-Host ""

$choice = Read-Host "Enter your choice (1 or 2)"

# Get the script directory (where this file is located)
$scriptRoot = $PSScriptRoot
if (-not $scriptRoot) {
    $scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
}

$quickStartScript = $null

if ($choice -eq "1") {
    $quickStartScript = Join-Path $scriptRoot "scripts\quick-start\quick-start-docker-automated.ps1"
    Write-Host ""
    Write-Host "Starting Docker setup..." -ForegroundColor Green
} elseif ($choice -eq "2") {
    $quickStartScript = Join-Path $scriptRoot "scripts\quick-start\quick-start-local-automated.ps1"
    Write-Host ""
    Write-Host "Starting Local setup..." -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "[X] Invalid choice. Please enter 1 or 2." -ForegroundColor Red
    Write-Host ""
    pause
    exit 1
}

# Check if the quick-start script exists
if (-not (Test-Path $quickStartScript)) {
    Write-Host "[X] Quick-start script not found at: $quickStartScript" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please make sure you're running this from the project root folder." -ForegroundColor Yellow
    Write-Host ""
    pause
    exit 1
}

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
