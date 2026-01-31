# First Setup Script
# This script prompts the user to choose between Docker or Local setup
# Double-click "First setup.bat" or run: .\First setup.ps1

$ErrorActionPreference = "Continue"

# Wait for user to press Enter or Esc before closing
function Wait-EnterOrEscape {
    Write-Host "Press Enter or Esc to exit..." -ForegroundColor Gray
    do {
        $key = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    } while ($key.VirtualKeyCode -ne 13 -and $key.VirtualKeyCode -ne 27)
}

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
    # Option 1 requires Docker – check before running
    $dockerOk = $null -ne (Get-Command docker -ErrorAction SilentlyContinue)
    if (-not $dockerOk) {
        Write-Host "========================================" -ForegroundColor Red
        Write-Host "  DOCKER IS REQUIRED FOR THIS OPTION" -ForegroundColor Red
        Write-Host "========================================" -ForegroundColor Red
        Write-Host ""
        Write-Host "You chose option 1 (Docker), but Docker is not installed or not in PATH." -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Please install Docker Desktop, then run this setup again:" -ForegroundColor White
        Write-Host "  https://www.docker.com/products/docker-desktop" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "Alternatively, choose option 2 (Local) if you have SQL Server LocalDB installed." -ForegroundColor Gray
        Write-Host ""
        Wait-EnterOrEscape
        exit 1
    }
    $dockerResponding = $false
    try {
        $job = Start-Job -ScriptBlock { docker ps 2>&1 | Out-Null; return $LASTEXITCODE }
        $completed = Wait-Job -Job $job -Timeout 3
        if ($completed) { $dockerResponding = (Receive-Job -Job $job) -eq 0 }
        Remove-Job -Job $job -Force -ErrorAction SilentlyContinue
    } catch { }
    if (-not $dockerResponding) {
        Write-Host "========================================" -ForegroundColor Red
        Write-Host "  DOCKER IS NOT RUNNING" -ForegroundColor Red
        Write-Host "========================================" -ForegroundColor Red
        Write-Host ""
        Write-Host "Docker is installed but the Docker daemon is not responding." -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Please start Docker Desktop and wait until it is fully running," -ForegroundColor White
        Write-Host "then run this setup again." -ForegroundColor White
        Write-Host ""
        Wait-EnterOrEscape
        exit 1
    }
    Write-Host "Starting Docker setup..." -ForegroundColor Green
} elseif ($choice -eq "2") {
    $quickStartScript = Join-Path $scriptRoot "scripts\quick-start\quick-start-local-automated.ps1"
    Write-Host ""
    # Option 2 requires SQL Server: LocalDB OR full SQL Server (e.g. MSSQLSERVER)
    $sqlServerOk = $false
    if ($null -ne (Get-Command sqllocaldb -ErrorAction SilentlyContinue)) {
        $out = sqllocaldb info 2>&1
        if ($LASTEXITCODE -eq 0) { $sqlServerOk = $true }
    }
    if (-not $sqlServerOk) {
        $localdbPaths = @(
            "C:\Program Files\Microsoft SQL Server\150\Tools\Binn\SqlLocalDB.exe",
            "C:\Program Files\Microsoft SQL Server\160\Tools\Binn\SqlLocalDB.exe"
        )
        foreach ($p in $localdbPaths) { if (Test-Path $p) { $sqlServerOk = $true; break } }
    }
    if (-not $sqlServerOk) {
        $fullSql = Get-Service -Name "MSSQLSERVER" -ErrorAction SilentlyContinue
        if ($fullSql -and $fullSql.Status -eq "Running") { $sqlServerOk = $true }
    }
    if (-not $sqlServerOk) {
        Write-Host "========================================" -ForegroundColor Red
        Write-Host "  SQL SERVER IS REQUIRED" -ForegroundColor Red
        Write-Host "========================================" -ForegroundColor Red
        Write-Host ""
        Write-Host "You chose option 2 (Local). You need one of:" -ForegroundColor Yellow
        Write-Host "  - SQL Server LocalDB (Express with LocalDB), or" -ForegroundColor White
        Write-Host "  - Full SQL Server (e.g. SQL Server Express) with the MSSQLSERVER service running" -ForegroundColor White
        Write-Host ""
        Write-Host "Download: https://www.microsoft.com/sql-server/sql-server-downloads" -ForegroundColor Cyan
        Write-Host "  (Express + LocalDB, or full Express)" -ForegroundColor Gray
        Write-Host ""
        Write-Host "Alternatively, choose option 1 (Docker) if you have Docker Desktop installed." -ForegroundColor Gray
        Write-Host ""
        Wait-EnterOrEscape
        exit 1
    }
    Write-Host "Starting Local setup..." -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "[X] Invalid choice. Please enter 1 or 2." -ForegroundColor Red
    Write-Host ""
    Wait-EnterOrEscape
    exit 1
}

# Check if the quick-start script exists
if (-not (Test-Path $quickStartScript)) {
    Write-Host "[X] Quick-start script not found at: $quickStartScript" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please make sure you're running this from the project root folder." -ForegroundColor Yellow
    Write-Host ""
    Wait-EnterOrEscape
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
    Wait-EnterOrEscape
    exit 1
}

Write-Host ""
Wait-EnterOrEscape
