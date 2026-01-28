# Stop Docker Services
# Stops all Docker containers for this project

$ErrorActionPreference = "Continue"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Stopping Docker Services" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Function to check if a command exists
function Test-Command {
    param([string]$Command)
    $null = Get-Command $Command -ErrorAction SilentlyContinue
    return $?
}

# Check Docker
if (-not (Test-Command "docker")) {
    Write-Host "[X] Docker not found" -ForegroundColor Red
    Write-Host "Docker is not installed or not in PATH." -ForegroundColor Yellow
    exit 1
}

# Check Docker Compose
$composeCommand = $null
if (Test-Command "docker compose") {
    $composeCommand = "docker compose"
} elseif (Test-Command "docker-compose") {
    $composeCommand = "docker-compose"
} else {
    Write-Host "[X] Docker Compose not found" -ForegroundColor Red
    exit 1
}

# Check if Docker is running
Write-Host "Checking if Docker is running..." -ForegroundColor Yellow
try {
    docker ps | Out-Null
    Write-Host "[OK] Docker is running" -ForegroundColor Green
} catch {
    Write-Host "[X] Docker is not running" -ForegroundColor Red
    Write-Host "Please start Docker Desktop first." -ForegroundColor Yellow
    exit 1
}

Write-Host ""

# Get project root directory (where docker-compose.yml is)
$projectRoot = Join-Path $PSScriptRoot ".."
$dockerComposePath = Join-Path $projectRoot "docker\docker-compose.yml"

Push-Location $projectRoot

Write-Host "Stopping Docker containers..." -ForegroundColor Yellow
try {
    if ($composeCommand -eq "docker compose") {
        docker compose -f $dockerComposePath --project-directory $projectRoot down
    } else {
        docker-compose -f $dockerComposePath --project-directory $projectRoot down
    }
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[OK] Docker containers stopped" -ForegroundColor Green
    } else {
        Write-Host "[!] Some containers may not have stopped" -ForegroundColor Yellow
    }
} catch {
    Write-Host "[X] Error stopping Docker containers: $_" -ForegroundColor Red
    Pop-Location
    exit 1
}

Pop-Location

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Docker services stopped!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "To start them again:" -ForegroundColor Cyan
Write-Host "  docker compose up -d" -ForegroundColor White
Write-Host ""
