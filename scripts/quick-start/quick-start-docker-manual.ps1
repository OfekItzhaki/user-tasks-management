# Quick Start - Docker (Manual Steps)
# Shows manual commands for Docker setup
# Use this if you prefer to run commands step-by-step

$ErrorActionPreference = "Continue"

# Fix PATH for .NET SDK
$env:DOTNET_ROOT = "C:\Program Files\dotnet"
$env:PATH = "C:\Program Files\dotnet;$env:PATH"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Quick Start - Docker (Manual Steps)" -ForegroundColor Cyan
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

# Check .NET SDK
if (-not (Test-Command "dotnet")) {
    Write-Host "[X] .NET SDK not found" -ForegroundColor Red
    $prerequisitesOk = $false
} else {
    $version = dotnet --version
    Write-Host "[OK] .NET SDK: $version" -ForegroundColor Green
}

# Check Node.js
if (-not (Test-Command "node")) {
    Write-Host "[X] Node.js not found" -ForegroundColor Red
    $prerequisitesOk = $false
} else {
    $version = node --version
    Write-Host "[OK] Node.js: $version" -ForegroundColor Green
}

# Check Docker
if (-not (Test-Command "docker")) {
    Write-Host "[X] Docker not found" -ForegroundColor Red
    $prerequisitesOk = $false
} else {
    $version = docker --version
    Write-Host "[OK] Docker: $version" -ForegroundColor Green
}

# Check Docker Compose
$composeCommand = $null
if (Test-Command "docker compose") {
    $composeCommand = "docker compose"
    Write-Host "[OK] Docker Compose: Available" -ForegroundColor Green
} elseif (Test-Command "docker-compose") {
    $composeCommand = "docker-compose"
    Write-Host "[OK] Docker Compose: Available" -ForegroundColor Green
} else {
    Write-Host "[X] Docker Compose not found" -ForegroundColor Red
    $prerequisitesOk = $false
}

Write-Host ""

if (-not $prerequisitesOk) {
    Write-Host "Please install missing prerequisites before continuing." -ForegroundColor Red
    Write-Host ""
    Write-Host "Press any key to exit..." -ForegroundColor Yellow
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}

# Check if Docker is running
Write-Host "Checking if Docker is running..." -ForegroundColor Yellow
try {
    docker ps | Out-Null
    Write-Host "[OK] Docker is running" -ForegroundColor Green
} catch {
    Write-Host "[X] Docker is not running" -ForegroundColor Red
    Write-Host "Please start Docker Desktop and run this script again." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Press any key to exit..." -ForegroundColor Yellow
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Manual Steps to Run:" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Step 1: Start Docker services" -ForegroundColor Yellow
Write-Host "  Run this command:" -ForegroundColor White
Write-Host "  $composeCommand up -d sqlserver rabbitmq" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Press Enter after running the command..." -ForegroundColor Gray
$null = Read-Host

Write-Host ""
Write-Host "Step 2: Install dotnet-ef tool (if not installed)" -ForegroundColor Yellow
Write-Host "  Run this command:" -ForegroundColor White
Write-Host "  dotnet tool install --global dotnet-ef" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Press Enter after running the command..." -ForegroundColor Gray
$null = Read-Host

Write-Host ""
Write-Host "Step 3: Run database migrations" -ForegroundColor Yellow
Write-Host "  Run these commands:" -ForegroundColor White
Write-Host "  cd src\TaskManagement.API" -ForegroundColor Cyan
Write-Host "  dotnet ef database update --project ..\TaskManagement.Infrastructure" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Press Enter after running the commands..." -ForegroundColor Gray
$null = Read-Host

Write-Host ""
Write-Host "Step 4: Install frontend dependencies" -ForegroundColor Yellow
Write-Host "  Run these commands:" -ForegroundColor White
Write-Host "  cd ..\TaskManagement.Web" -ForegroundColor Cyan
Write-Host "  npm install" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Press Enter after running the commands..." -ForegroundColor Gray
$null = Read-Host

Write-Host ""
Write-Host "Step 5: Build solution" -ForegroundColor Yellow
Write-Host "  Run these commands:" -ForegroundColor White
Write-Host "  cd ..\.." -ForegroundColor Cyan
Write-Host "  dotnet build" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Press Enter after running the commands..." -ForegroundColor Gray
$null = Read-Host

Write-Host ""
Write-Host "Step 6: Start all services" -ForegroundColor Yellow
Write-Host "  Run this command:" -ForegroundColor White
Write-Host "  .\start-all.ps1" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Press Enter after running the command..." -ForegroundColor Gray
$null = Read-Host

Write-Host ""
Write-Host "Step 7: Verify Installation" -ForegroundColor Yellow
Write-Host "  Run these commands to verify:" -ForegroundColor White
Write-Host "  dotnet --version    # Should show 8.0.x" -ForegroundColor Cyan
Write-Host "  node --version      # Should show 20.x or 22.x" -ForegroundColor Cyan
Write-Host "  npm --version       # Should show 10.x or 11.x" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Press Enter after verifying..." -ForegroundColor Gray
$null = Read-Host

Write-Host ""
Write-Host "Step 8: Seed Database (Optional)" -ForegroundColor Yellow
Write-Host "  After the API starts, seed the database with sample data:" -ForegroundColor White
Write-Host "  POST http://localhost:5063/api/seed" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Or use Swagger UI:" -ForegroundColor White
Write-Host "  1. Open http://localhost:5063/swagger" -ForegroundColor Cyan
Write-Host "  2. Find POST /api/seed endpoint" -ForegroundColor Cyan
Write-Host "  3. Click 'Try it out' then 'Execute'" -ForegroundColor Cyan
Write-Host ""
Write-Host "  This will create sample users, tags, and tasks for testing." -ForegroundColor Gray
Write-Host "  Press Enter to continue..." -ForegroundColor Gray
$null = Read-Host

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "All steps completed!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Access points:" -ForegroundColor Cyan
Write-Host "  - Frontend: http://localhost:5173" -ForegroundColor White
Write-Host "  - API Swagger: http://localhost:5063/swagger" -ForegroundColor White
Write-Host "  - RabbitMQ Management: http://localhost:15672 (guest/guest)" -ForegroundColor White
Write-Host ""
