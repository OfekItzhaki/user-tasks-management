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

# Check Docker
if (-not (Test-Command "docker")) {
    Write-Host "[X] Docker not found" -ForegroundColor Red
    $prerequisitesOk = $false
    $missingPrerequisites += "Docker Desktop (https://www.docker.com/products/docker-desktop)"
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

# Function to check if Docker Desktop is running
function Test-DockerRunning {
    try {
        $dockerInfo = docker info 2>&1 | Out-Null
        return $LASTEXITCODE -eq 0
    } catch {
        return $false
    }
}

# Function to start Docker Desktop
function Start-DockerDesktop {
    Write-Host "Docker Desktop is not running. Attempting to start it..." -ForegroundColor Yellow
    Write-Host ""
    
    # Common Docker Desktop installation paths
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
    
    if (-not $dockerExe) {
        Write-Host "[X] Docker Desktop executable not found in common locations" -ForegroundColor Red
        Write-Host ""
        Write-Host "Please start Docker Desktop manually:" -ForegroundColor Yellow
        Write-Host "  1. Open Start menu and search for 'Docker Desktop'" -ForegroundColor White
        Write-Host "  2. Click to start Docker Desktop" -ForegroundColor White
        Write-Host "  3. Wait for it to fully start (whale icon in system tray)" -ForegroundColor White
        Write-Host "  4. Run this script again" -ForegroundColor White
        exit 1
    }
    
    Write-Host "Starting Docker Desktop..." -ForegroundColor Cyan
    try {
        Start-Process -FilePath $dockerExe -ErrorAction Stop
        Write-Host "[OK] Docker Desktop launch command executed" -ForegroundColor Green
    } catch {
        Write-Host "[X] Failed to start Docker Desktop: $_" -ForegroundColor Red
        Write-Host ""
        Write-Host "Please start Docker Desktop manually and run this script again." -ForegroundColor Yellow
        exit 1
    }
    
    # Wait for Docker Desktop to start
    Write-Host ""
    Write-Host "Waiting for Docker Desktop to start..." -ForegroundColor Yellow
    Write-Host "This may take 30-60 seconds. Please wait..." -ForegroundColor Gray
    Write-Host ""
    
    $maxWait = 120 # Maximum wait time in seconds
    $waited = 0
    $checkInterval = 3 # Check every 3 seconds
    $dockerReady = $false
    
    while (-not $dockerReady -and $waited -lt $maxWait) {
        Start-Sleep -Seconds $checkInterval
        $waited += $checkInterval
        
        if (Test-DockerRunning) {
            $dockerReady = $true
            Write-Host "[OK] Docker Desktop is now running!" -ForegroundColor Green
            break
        }
        
        # Show progress every 10 seconds
        if ($waited % 10 -eq 0) {
            Write-Host "  Still waiting... ($waited seconds)" -ForegroundColor Gray
        }
    }
    
    if (-not $dockerReady) {
        Write-Host ""
        Write-Host "[X] Docker Desktop did not start within $maxWait seconds" -ForegroundColor Red
        Write-Host ""
        Write-Host "Possible issues:" -ForegroundColor Yellow
        Write-Host "  - Virtualization may not be enabled in BIOS" -ForegroundColor White
        Write-Host "  - WSL 2 may not be installed or configured" -ForegroundColor White
        Write-Host "  - Docker Desktop may need manual configuration" -ForegroundColor White
        Write-Host ""
        Write-Host "Please:" -ForegroundColor Yellow
        Write-Host "  1. Check Docker Desktop window for error messages" -ForegroundColor White
        Write-Host "  2. Ensure virtualization is enabled in BIOS" -ForegroundColor White
        Write-Host "  3. Install WSL 2 if prompted by Docker Desktop" -ForegroundColor White
        Write-Host "  4. Start Docker Desktop manually and wait for it to fully start" -ForegroundColor White
        Write-Host "  5. Run this script again" -ForegroundColor White
        exit 1
    }
    
    # Give Docker a few more seconds to fully initialize
    Write-Host "  Giving Docker a moment to fully initialize..." -ForegroundColor Gray
    Start-Sleep -Seconds 5
}

# Check if Docker is running
Write-Host "Checking if Docker is running..." -ForegroundColor Yellow
if (-not (Test-DockerRunning)) {
    Write-Host "[!] Docker Desktop is not running" -ForegroundColor Yellow
    Write-Host ""
    
    # Check if Docker Desktop process exists (might be starting)
    $dockerProcess = Get-Process -Name "Docker Desktop" -ErrorAction SilentlyContinue
    if ($dockerProcess) {
        Write-Host "Docker Desktop process detected but not ready yet..." -ForegroundColor Yellow
        Write-Host "Waiting for Docker Desktop to be ready..." -ForegroundColor Yellow
        Write-Host ""
        
        $maxWait = 60
        $waited = 0
        $checkInterval = 3
        
        while (-not (Test-DockerRunning) -and $waited -lt $maxWait) {
            Start-Sleep -Seconds $checkInterval
            $waited += $checkInterval
            if ($waited % 10 -eq 0) {
                Write-Host "  Still waiting... ($waited seconds)" -ForegroundColor Gray
            }
        }
        
        if (-not (Test-DockerRunning)) {
            Write-Host "[X] Docker Desktop is running but not responding" -ForegroundColor Red
            Write-Host "Please check Docker Desktop for errors and try again." -ForegroundColor Yellow
            exit 1
        }
    } else {
        # Docker Desktop is not running, try to start it
        Start-DockerDesktop
    }
} else {
    Write-Host "[OK] Docker is running" -ForegroundColor Green
}

Write-Host ""

# Step 1: Start Docker services
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Step 1: Starting Docker services..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$projectRoot = Join-Path $PSScriptRoot "..\.."
$dockerComposePath = Join-Path $projectRoot "docker\docker-compose.yml"

Write-Host "Starting Docker services (this may take a few minutes on first run)..." -ForegroundColor Yellow

# Remove any existing containers to avoid name conflicts
Write-Host "Cleaning up any existing containers..." -ForegroundColor Gray

# Remove containers directly by name first (most reliable method)
$containerNames = @("taskmanagement-sqlserver", "taskmanagement-rabbitmq", "taskmanagement-api", "taskmanagement-frontend", "taskmanagement-service")
foreach ($containerName in $containerNames) {
    # Try to remove container - docker rm -f is safe even if container doesn't exist
    $removeOutput = docker rm -f $containerName 2>&1
    if ($LASTEXITCODE -eq 0 -or $removeOutput -match "No such container") {
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  Removed existing container: $containerName" -ForegroundColor Yellow
        }
    } else {
        # Show error but continue - might be a different issue
        Write-Host "  Warning: Could not remove $containerName : $removeOutput" -ForegroundColor Yellow
    }
}

# Also try docker compose down to remove containers created by compose
try {
    Push-Location $projectRoot
    if ($composeCommand -eq "docker compose") {
        docker compose -f $dockerComposePath --project-directory $projectRoot down 2>&1 | Out-Null
    } else {
        docker-compose -f $dockerComposePath --project-directory $projectRoot down 2>&1 | Out-Null
    }
    Pop-Location
} catch {
    # Ignore errors - containers might not exist or already removed
}

Write-Host "Pulling images and starting containers..." -ForegroundColor Gray
Write-Host ""

# Docker Compose uses the compose file's directory as base for relative paths
# We need to specify --project-directory to use the project root
Push-Location $projectRoot
try {
    # Run docker compose - let output show naturally
    # Use --project-directory to ensure build contexts resolve correctly
    # Use --force-recreate to handle any remaining container conflicts
    if ($composeCommand -eq "docker compose") {
        docker compose -f $dockerComposePath --project-directory $projectRoot up -d --force-recreate sqlserver rabbitmq
    } else {
        docker-compose -f $dockerComposePath --project-directory $projectRoot up -d --force-recreate sqlserver rabbitmq
    }
    $dockerExitCode = $LASTEXITCODE
} catch {
    # Only catch actual exceptions, not output
    $dockerExitCode = 1
    Write-Host ""
    Write-Host "[X] Exception occurred: $_" -ForegroundColor Red
} finally {
    Pop-Location
}

if ($dockerExitCode -ne 0) {
        Write-Host "[X] Failed to start Docker services" -ForegroundColor Red
        Write-Host ""
        
        # Check if Docker is still running
        if (-not (Test-DockerRunning)) {
            Write-Host "Docker Desktop appears to have stopped or crashed." -ForegroundColor Yellow
            Write-Host "Attempting to restart Docker Desktop..." -ForegroundColor Yellow
            Write-Host ""
            Start-DockerDesktop
            Write-Host ""
            Write-Host "Retrying Docker services startup..." -ForegroundColor Yellow
            
            # Retry starting services
            Write-Host "Pulling images and starting containers..." -ForegroundColor Gray
            Push-Location $projectRoot
            try {
                if ($composeCommand -eq "docker compose") {
                    docker compose -f $dockerComposePath --project-directory $projectRoot up -d --force-recreate sqlserver rabbitmq
                } else {
                    docker-compose -f $dockerComposePath --project-directory $projectRoot up -d --force-recreate sqlserver rabbitmq
                }
                $retryExitCode = $LASTEXITCODE
            } catch {
                $retryExitCode = 1
            } finally {
                Pop-Location
            }
            
            if ($retryExitCode -ne 0) {
                Write-Host "[X] Still failed to start Docker services after restart" -ForegroundColor Red
                Write-Host ""
                Write-Host "Common issues:" -ForegroundColor Yellow
                Write-Host "  - Port conflicts (check if ports 1433, 5672, 15672 are in use)" -ForegroundColor White
                Write-Host "  - Virtualization not enabled in BIOS" -ForegroundColor White
                Write-Host "  - WSL 2 not installed or configured" -ForegroundColor White
                Write-Host ""
                Write-Host "Please check Docker Desktop for error messages and try again." -ForegroundColor Yellow
                exit 1
            }
        } else {
            Write-Host "Common issues:" -ForegroundColor Yellow
            Write-Host "  - Port conflicts (check if ports 1433, 5672, 15672 are in use)" -ForegroundColor White
            Write-Host "  - Docker Desktop may need a restart" -ForegroundColor White
            Write-Host ""
            Write-Host "Try:" -ForegroundColor Yellow
            Write-Host "  1. Check Docker Desktop for error messages" -ForegroundColor White
            Write-Host "  2. Restart Docker Desktop" -ForegroundColor White
            Write-Host "  3. Run this script again" -ForegroundColor White
            exit 1
        }
    }
    Write-Host "[OK] Docker services started" -ForegroundColor Green
    Write-Host "  Waiting for services to be ready..." -ForegroundColor Yellow
    Start-Sleep -Seconds 15

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
        Write-Host "[X] Setup script failed" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "[X] Setup script not found: $setupScript" -ForegroundColor Red
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
    Write-Host "[X] Start script not found: $startScript" -ForegroundColor Red
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
Write-Host "  - RabbitMQ Management: http://localhost:15672 (guest/guest)" -ForegroundColor White
Write-Host ""
Write-Host "Installation verified:" -ForegroundColor Cyan
Write-Host "  - .NET SDK: $dotnetVersion" -ForegroundColor White
Write-Host "  - Node.js: $nodeVersion" -ForegroundColor White
Write-Host "  - npm: $npmVersion" -ForegroundColor White
Write-Host ""
