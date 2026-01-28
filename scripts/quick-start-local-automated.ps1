# Quick Start - Local (Automated)
# Uses LocalDB instead of Docker
# This script checks prerequisites and runs everything automatically

$ErrorActionPreference = "Stop"

# Fix PATH for .NET SDK
$env:DOTNET_ROOT = "C:\Program Files\dotnet"
$env:PATH = "C:\Program Files\dotnet;$env:PATH"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Quick Start - Local (Automated)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Function to check if a command exists
function Test-Command {
    param([string]$Command)
    $null = Get-Command $Command -ErrorAction SilentlyContinue
    return $?
}

# Check prerequisites
Write-Host "Checking prerequisites..." -ForegroundColor Yellow
Write-Host ""

$prerequisitesOk = $true
$missingPrerequisites = @()

# Check .NET SDK
if (-not (Test-Command "dotnet")) {
    Write-Host "✗ .NET SDK not found" -ForegroundColor Red
    $prerequisitesOk = $false
    $missingPrerequisites += ".NET 8.0 SDK (https://dotnet.microsoft.com/download)"
} else {
    $version = dotnet --version
    Write-Host "✓ .NET SDK: $version" -ForegroundColor Green
}

# Check Node.js
if (-not (Test-Command "node")) {
    Write-Host "✗ Node.js not found" -ForegroundColor Red
    $prerequisitesOk = $false
    $missingPrerequisites += "Node.js 20.19+ or 22.12+ (https://nodejs.org/)"
} else {
    $version = node --version
    Write-Host "✓ Node.js: $version" -ForegroundColor Green
}

# Check SQL Server LocalDB
$localdbFound = $false
try {
    $localdbInfo = sqllocaldb info mssqllocaldb 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ SQL Server LocalDB: Available" -ForegroundColor Green
        $localdbFound = $true
        # Start LocalDB if not running
        sqllocaldb start mssqllocaldb 2>&1 | Out-Null
    }
} catch {
    # Try to find LocalDB in common locations
    $localdbPaths = @(
        "C:\Program Files\Microsoft SQL Server\150\Tools\Binn\SqlLocalDB.exe",
        "C:\Program Files\Microsoft SQL Server\160\Tools\Binn\SqlLocalDB.exe"
    )
    foreach ($path in $localdbPaths) {
        if (Test-Path $path) {
            Write-Host "✓ SQL Server LocalDB: Found at $path" -ForegroundColor Green
            $localdbFound = $true
            break
        }
    }
}

if (-not $localdbFound) {
    Write-Host "⚠ SQL Server LocalDB: Not found" -ForegroundColor Yellow
    Write-Host "  LocalDB usually comes with Visual Studio" -ForegroundColor Gray
    Write-Host "  Or install SQL Server Express: https://www.microsoft.com/sql-server/sql-server-downloads" -ForegroundColor Gray
    Write-Host "  Continuing anyway (will try to use connection string)" -ForegroundColor Yellow
}

Write-Host ""

if (-not $prerequisitesOk) {
    Write-Host "Missing prerequisites:" -ForegroundColor Red
    foreach ($item in $missingPrerequisites) {
        Write-Host "  - $item" -ForegroundColor Red
    }
    Write-Host ""
    Write-Host "Please install the missing prerequisites and run this script again." -ForegroundColor Yellow
    exit 1
}

# Step 1: Run setup script
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Step 1: Running setup script..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$setupScript = Join-Path $PSScriptRoot "..\setup.ps1"
if (Test-Path $setupScript) {
    & $setupScript
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Setup script failed" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "✗ Setup script not found: $setupScript" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 2: Start all services
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Step 2: Starting all services..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$startScript = Join-Path $PSScriptRoot "..\start-all.ps1"
if (Test-Path $startScript) {
    & $startScript
} else {
    Write-Host "✗ Start script not found: $startScript" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "All done! Services are starting..." -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Access points:" -ForegroundColor Cyan
Write-Host "  • Frontend: http://localhost:5173" -ForegroundColor White
Write-Host "  • API Swagger: http://localhost:5063/swagger" -ForegroundColor White
Write-Host ""
Write-Host "Note: RabbitMQ is optional. Windows Service will work without it." -ForegroundColor Yellow
Write-Host ""
