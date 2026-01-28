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

try {
    & $composeCommand.Split(' ') up -d sqlserver rabbitmq
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
Write-Host "========================================" -ForegroundColor Green
Write-Host "All done! Services are starting..." -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Access points:" -ForegroundColor Cyan
Write-Host "  • Frontend: http://localhost:5173" -ForegroundColor White
Write-Host "  • API Swagger: http://localhost:5063/swagger" -ForegroundColor White
Write-Host "  • RabbitMQ Management: http://localhost:15672 (guest/guest)" -ForegroundColor White
Write-Host ""
