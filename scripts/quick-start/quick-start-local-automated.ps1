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
    Write-Host "[X] .NET SDK not found" -ForegroundColor Red
    $prerequisitesOk = $false
    $missingPrerequisites += ".NET 8.0 SDK (https://dotnet.microsoft.com/download)"
} else {
    $version = dotnet --version
    Write-Host "[OK] .NET SDK: $version" -ForegroundColor Green
}

# Check Node.js
if (-not (Test-Command "node")) {
    Write-Host "[X] Node.js not found" -ForegroundColor Red
    $prerequisitesOk = $false
    $missingPrerequisites += "Node.js 20.19+ or 22.12+ (https://nodejs.org/)"
} else {
    $version = node --version
    Write-Host "[OK] Node.js: $version" -ForegroundColor Green
}

# Check SQL Server LocalDB
$localdbFound = $false
try {
    $localdbInfo = sqllocaldb info mssqllocaldb 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[OK] SQL Server LocalDB: Available" -ForegroundColor Green
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
            Write-Host "[OK] SQL Server LocalDB: Found at $path" -ForegroundColor Green
            $localdbFound = $true
            break
        }
    }
}

if (-not $localdbFound) {
    Write-Host "[!] SQL Server LocalDB: Not found" -ForegroundColor Yellow
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
        Write-Host "[X] Setup script failed" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "[X] Setup script not found: $setupScript" -ForegroundColor Red
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
    Write-Host "[X] Start script not found: $startScript" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 3: Verify Installation
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Step 3: Verifying Installation..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Checking installed versions:" -ForegroundColor Yellow
$dotnetVersion = dotnet --version
$nodeVersion = node --version
$npmVersion = npm --version

Write-Host "  [OK] .NET SDK: $dotnetVersion" -ForegroundColor Green
Write-Host "  [OK] Node.js: $nodeVersion" -ForegroundColor Green
Write-Host "  [OK] npm: $npmVersion" -ForegroundColor Green

Write-Host ""
Write-Host "Verifying database setup..." -ForegroundColor Yellow
$apiPath = Join-Path $PSScriptRoot "..\..\src\TaskManagement.API"
Push-Location $apiPath
try {
    # Check if migrations are applied
    dotnet ef migrations list --project ..\TaskManagement.Infrastructure 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  [OK] Database migrations verified" -ForegroundColor Green
    } else {
        Write-Host "  [!] Database migrations may need to be applied" -ForegroundColor Yellow
    }
} catch {
    Write-Host "  [!] Could not verify database migrations" -ForegroundColor Yellow
}
Pop-Location

Write-Host ""
Write-Host "Verifying frontend dependencies..." -ForegroundColor Yellow
$webPath = Join-Path $PSScriptRoot "..\..\src\TaskManagement.Web"
if (Test-Path (Join-Path $webPath "node_modules")) {
    Write-Host "  [OK] Frontend dependencies installed" -ForegroundColor Green
} else {
    Write-Host "  [!] Frontend dependencies not found" -ForegroundColor Yellow
}

Write-Host ""

# Step 4: Offer to seed database
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Step 4: Database Seeding (Optional)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Would you like to seed the database with sample data?" -ForegroundColor Yellow
Write-Host "This will create sample users, tags, and tasks for testing." -ForegroundColor Gray
Write-Host ""
$seedChoice = Read-Host "Seed database? (y/n)"

if ($seedChoice -eq "y" -or $seedChoice -eq "Y") {
    Write-Host ""
    Write-Host "Waiting for API to be ready..." -ForegroundColor Yellow
    $apiReady = $false
    $maxWait = 30
    $waited = 0
    
    while (-not $apiReady -and $waited -lt $maxWait) {
        try {
            $response = Invoke-WebRequest -Uri "http://localhost:5063/api/seed" -Method POST -TimeoutSec 2 -ErrorAction SilentlyContinue
            if ($response.StatusCode -eq 200) {
                $apiReady = $true
                Write-Host "  [OK] Database seeded successfully!" -ForegroundColor Green
            }
        } catch {
            Start-Sleep -Seconds 2
            $waited += 2
            Write-Host "." -NoNewline -ForegroundColor Gray
        }
    }
    
    if (-not $apiReady) {
        Write-Host ""
        Write-Host "  [!] Could not seed database automatically" -ForegroundColor Yellow
        Write-Host "  You can seed it manually after the API starts:" -ForegroundColor Gray
        Write-Host "    POST http://localhost:5063/api/seed" -ForegroundColor Cyan
        Write-Host "    Or use Swagger UI: http://localhost:5063/swagger" -ForegroundColor Cyan
    }
} else {
    Write-Host ""
    Write-Host "Skipping database seeding." -ForegroundColor Gray
    Write-Host "You can seed it later:" -ForegroundColor Gray
    Write-Host "  POST http://localhost:5063/api/seed" -ForegroundColor Cyan
    Write-Host "  Or use Swagger UI: http://localhost:5063/swagger" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "All done! Services are starting..." -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Access points:" -ForegroundColor Cyan
Write-Host "  - Frontend: http://localhost:5173" -ForegroundColor White
Write-Host "  - API Swagger: http://localhost:5063/swagger" -ForegroundColor White
Write-Host ""
Write-Host "Installation verified:" -ForegroundColor Cyan
Write-Host "  - .NET SDK: $dotnetVersion" -ForegroundColor White
Write-Host "  - Node.js: $nodeVersion" -ForegroundColor White
Write-Host "  - npm: $npmVersion" -ForegroundColor White
Write-Host ""
Write-Host "Note: RabbitMQ is optional. Windows Service will work without it." -ForegroundColor Yellow
Write-Host ""
