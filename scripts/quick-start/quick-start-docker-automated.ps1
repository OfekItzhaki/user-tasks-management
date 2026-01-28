# Quick Start - Docker (Automated)
# Easiest option: Uses Docker for SQL Server and RabbitMQ
# This script checks prerequisites and runs everything automatically

$ErrorActionPreference = "Stop"

# Fix PATH for .NET SDK
$env:DOTNET_ROOT = "C:\Program Files\dotnet"
$env:PATH = "C:\Program Files\dotnet;$env:PATH"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Quick Start - Docker (Automated)" -ForegroundColor Cyan
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

# Check Docker
if (-not (Test-Command "docker")) {
    Write-Host "✗ Docker not found" -ForegroundColor Red
    $prerequisitesOk = $false
    $missingPrerequisites += "Docker Desktop (https://www.docker.com/products/docker-desktop)"
} else {
    $version = docker --version
    Write-Host "✓ Docker: $version" -ForegroundColor Green
}

# Check Docker Compose
$composeCommand = $null
if (Test-Command "docker compose") {
    $composeCommand = "docker compose"
    Write-Host "✓ Docker Compose: Available" -ForegroundColor Green
} elseif (Test-Command "docker-compose") {
    $composeCommand = "docker-compose"
    Write-Host "✓ Docker Compose: Available" -ForegroundColor Green
} else {
    Write-Host "✗ Docker Compose not found" -ForegroundColor Red
    $prerequisitesOk = $false
    $missingPrerequisites += "Docker Compose (usually included with Docker Desktop)"
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

# Check if Docker is running
Write-Host "Checking if Docker is running..." -ForegroundColor Yellow
try {
    docker ps | Out-Null
    Write-Host "✓ Docker is running" -ForegroundColor Green
} catch {
    Write-Host "✗ Docker is not running" -ForegroundColor Red
    Write-Host "Please start Docker Desktop and try again." -ForegroundColor Yellow
    exit 1
}

Write-Host ""

# Step 1: Start Docker services
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Step 1: Starting Docker services..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$projectRoot = Join-Path $PSScriptRoot "..\.."
$dockerComposePath = Join-Path $projectRoot "docker\docker-compose.yml"
try {
    Push-Location $projectRoot
    if ($composeCommand -eq "docker compose") {
        docker compose -f $dockerComposePath up -d sqlserver rabbitmq
    } else {
        docker-compose -f $dockerComposePath up -d sqlserver rabbitmq
    }
    Pop-Location
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Failed to start Docker services" -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ Docker services started" -ForegroundColor Green
    Write-Host "  Waiting for services to be ready..." -ForegroundColor Yellow
    Start-Sleep -Seconds 15
} catch {
    Write-Host "✗ Error starting Docker services: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 2: Run setup script
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Step 2: Running setup script..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$setupScript = Join-Path $PSScriptRoot "..\setup-docker.ps1"
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

# Step 3: Start all services
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Step 3: Starting all services..." -ForegroundColor Cyan
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

# Step 4: Verify Installation
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Step 4: Verifying Installation..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Checking installed versions:" -ForegroundColor Yellow
$dotnetVersion = dotnet --version
$nodeVersion = node --version
$npmVersion = npm --version

Write-Host "  ✓ .NET SDK: $dotnetVersion" -ForegroundColor Green
Write-Host "  ✓ Node.js: $nodeVersion" -ForegroundColor Green
Write-Host "  ✓ npm: $npmVersion" -ForegroundColor Green

Write-Host ""
Write-Host "Verifying database setup..." -ForegroundColor Yellow
$apiPath = Join-Path $PSScriptRoot "..\..\src\TaskManagement.API"
Push-Location $apiPath
try {
    # Check if migrations are applied
    dotnet ef migrations list --project ..\TaskManagement.Infrastructure 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✓ Database migrations verified" -ForegroundColor Green
    } else {
        Write-Host "  ⚠ Database migrations may need to be applied" -ForegroundColor Yellow
    }
} catch {
    Write-Host "  ⚠ Could not verify database migrations" -ForegroundColor Yellow
}
Pop-Location

Write-Host ""
Write-Host "Verifying frontend dependencies..." -ForegroundColor Yellow
$webPath = Join-Path $PSScriptRoot "..\..\src\TaskManagement.Web"
if (Test-Path (Join-Path $webPath "node_modules")) {
    Write-Host "  ✓ Frontend dependencies installed" -ForegroundColor Green
} else {
    Write-Host "  ⚠ Frontend dependencies not found" -ForegroundColor Yellow
}

Write-Host ""

# Step 5: Offer to seed database
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Step 5: Database Seeding (Optional)" -ForegroundColor Cyan
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
                Write-Host "  ✓ Database seeded successfully!" -ForegroundColor Green
            }
        } catch {
            Start-Sleep -Seconds 2
            $waited += 2
            Write-Host "." -NoNewline -ForegroundColor Gray
        }
    }
    
    if (-not $apiReady) {
        Write-Host ""
        Write-Host "  ⚠ Could not seed database automatically" -ForegroundColor Yellow
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
Write-Host "  - RabbitMQ Management: http://localhost:15672 (guest/guest)" -ForegroundColor White
Write-Host ""
Write-Host "Installation verified:" -ForegroundColor Cyan
Write-Host "  - .NET SDK: $dotnetVersion" -ForegroundColor White
Write-Host "  - Node.js: $nodeVersion" -ForegroundColor White
Write-Host "  - npm: $npmVersion" -ForegroundColor White
Write-Host ""
