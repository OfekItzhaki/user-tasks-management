# Task Management System - Docker Setup Script
# This script sets up the project using Docker Compose for SQL Server and RabbitMQ

param(
    [switch]$SkipChecks = $false,
    [switch]$UseDocker = $true
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Task Management System - Docker Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "This setup uses Docker Compose for SQL Server and RabbitMQ" -ForegroundColor Yellow
Write-Host "No need to install SQL Server or RabbitMQ locally!" -ForegroundColor Green
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

if (-not (Test-Command "dotnet")) {
    Write-Host "[X] .NET SDK not found" -ForegroundColor Red
    $prerequisitesOk = $false
    $missingPrerequisites += ".NET 8.0 SDK"
} else {
    $version = dotnet --version
    Write-Host "[OK] .NET SDK found: $version" -ForegroundColor Green
}

if (-not (Test-Command "node")) {
    Write-Host "[X] Node.js not found" -ForegroundColor Red
    $prerequisitesOk = $false
    $missingPrerequisites += "Node.js 20.19+ or 22.12+"
} else {
    $version = node --version
    Write-Host "[OK] Node.js found: $version" -ForegroundColor Green
}

if (-not (Test-Command "docker")) {
    Write-Host "[X] Docker not found" -ForegroundColor Red
    $prerequisitesOk = $false
    $missingPrerequisites += "Docker Desktop"
    Write-Host "  Download: https://www.docker.com/products/docker-desktop" -ForegroundColor Gray
} else {
    $version = docker --version
    Write-Host "[OK] Docker found: $version" -ForegroundColor Green
}

if (-not (Test-Command "docker-compose")) {
    if (-not (Test-Command "docker compose")) {
        Write-Host "[X] Docker Compose not found" -ForegroundColor Red
        $prerequisitesOk = $false
        $missingPrerequisites += "Docker Compose (usually included with Docker Desktop)"
    } else {
        Write-Host "[OK] Docker Compose found (docker compose)" -ForegroundColor Green
    }
} else {
    Write-Host "[OK] Docker Compose found" -ForegroundColor Green
}

Write-Host ""

if (-not $prerequisitesOk) {
    Write-Host "Missing prerequisites:" -ForegroundColor Red
    foreach ($item in $missingPrerequisites) {
        Write-Host "  - $item" -ForegroundColor Red
    }
    Write-Host ""
    Write-Host "Please install the missing prerequisites:" -ForegroundColor Yellow
    Write-Host "  1. .NET 8.0 SDK: https://dotnet.microsoft.com/download" -ForegroundColor White
    Write-Host "  2. Node.js 20.19+: https://nodejs.org/" -ForegroundColor White
    Write-Host "  3. Docker Desktop: https://www.docker.com/products/docker-desktop" -ForegroundColor White
    Write-Host ""
    Write-Host "After installing, run this script again." -ForegroundColor Yellow
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
        Write-Host "[X] Docker Desktop executable not found" -ForegroundColor Red
        Write-Host "Please start Docker Desktop manually and run this script again." -ForegroundColor Yellow
        exit 1
    }
    
    Write-Host "Starting Docker Desktop..." -ForegroundColor Cyan
    try {
        Start-Process -FilePath $dockerExe -ErrorAction Stop
        Write-Host "[OK] Docker Desktop launch command executed" -ForegroundColor Green
    } catch {
        Write-Host "[X] Failed to start Docker Desktop: $_" -ForegroundColor Red
        Write-Host "Please start Docker Desktop manually and run this script again." -ForegroundColor Yellow
        exit 1
    }
    
    # Wait for Docker Desktop to start
    Write-Host ""
    Write-Host "Waiting for Docker Desktop to start (this may take 30-60 seconds)..." -ForegroundColor Yellow
    $maxWait = 120
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
        Write-Host "[X] Docker Desktop did not start within $maxWait seconds" -ForegroundColor Red
        Write-Host "Please start Docker Desktop manually and ensure virtualization is enabled." -ForegroundColor Yellow
        exit 1
    }
    
    Write-Host "[OK] Docker Desktop is now running!" -ForegroundColor Green
    Start-Sleep -Seconds 5 # Give Docker a moment to fully initialize
}

# Check if Docker is running
Write-Host "Checking if Docker is running..." -ForegroundColor Yellow
if (-not (Test-DockerRunning)) {
    $dockerProcess = Get-Process -Name "Docker Desktop" -ErrorAction SilentlyContinue
    if ($dockerProcess) {
        Write-Host "Docker Desktop process detected, waiting for it to be ready..." -ForegroundColor Yellow
        $maxWait = 60
        $waited = 0
        while (-not (Test-DockerRunning) -and $waited -lt $maxWait) {
            Start-Sleep -Seconds 3
            $waited += 3
        }
        if (-not (Test-DockerRunning)) {
            Write-Host "[X] Docker Desktop is not responding" -ForegroundColor Red
            exit 1
        }
    } else {
        Start-DockerDesktop
    }
} else {
    Write-Host "[OK] Docker is running" -ForegroundColor Green
}
Write-Host ""

# Start Docker Compose services
Write-Host "Starting Docker services (SQL Server and RabbitMQ)..." -ForegroundColor Yellow
Write-Host "This may take a few minutes on first run..." -ForegroundColor Gray
Write-Host ""

# Use docker compose (newer) or docker-compose (older)
$composeCommand = if (Test-Command "docker compose") { "docker compose" } else { "docker-compose" }

# Get project root (script is in scripts/ folder, so go up one level)
$projectRoot = Join-Path $PSScriptRoot ".."
$dockerComposePath = Join-Path $projectRoot "docker\docker-compose.yml"

try {
    Push-Location $projectRoot
    if ($composeCommand -eq "docker compose") {
        docker compose -f $dockerComposePath --project-directory $projectRoot up -d
    } else {
        docker-compose -f $dockerComposePath --project-directory $projectRoot up -d
    }
    if ($LASTEXITCODE -ne 0) {
        Pop-Location
        Write-Host "[X] Failed to start Docker services" -ForegroundColor Red
        exit 1
    }
    Pop-Location
    Write-Host "[OK] Docker services started" -ForegroundColor Green
    Write-Host ""
    
    Write-Host "Waiting for services to be ready..." -ForegroundColor Yellow
    Start-Sleep -Seconds 15
    
    # Check if services are healthy
    $sqlserverHealth = docker inspect --format='{{.State.Health.Status}}' taskmanagement-sqlserver 2>$null
    $rabbitmqHealth = docker inspect --format='{{.State.Health.Status}}' taskmanagement-rabbitmq 2>$null
    
    if ($sqlserverHealth -eq "healthy" -or $sqlserverHealth -eq "starting") {
        Write-Host "[OK] SQL Server is ready" -ForegroundColor Green
    } else {
        Write-Host "[!] SQL Server may still be starting (this is normal)" -ForegroundColor Yellow
    }
    
    if ($rabbitmqHealth -eq "healthy" -or $rabbitmqHealth -eq "starting") {
        Write-Host "[OK] RabbitMQ is ready" -ForegroundColor Green
    } else {
        Write-Host "[!] RabbitMQ may still be starting (this is normal)" -ForegroundColor Yellow
    }
    
} catch {
    Write-Host "[X] Error starting Docker services: $_" -ForegroundColor Red
    Write-Host "Try running: docker compose up -d" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "Docker services are running:" -ForegroundColor Cyan
Write-Host "  - SQL Server: localhost:1433" -ForegroundColor White
Write-Host "  - RabbitMQ: localhost:5672" -ForegroundColor White
Write-Host "  - RabbitMQ Management: http://localhost:15672
    Username: guest
    Password: guest" -ForegroundColor White
Write-Host ""

# Check if dotnet-ef is installed
Write-Host "Checking Entity Framework tools..." -ForegroundColor Yellow
if (-not (Test-Command "dotnet-ef")) {
    Write-Host "Installing dotnet-ef tool..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
    Write-Host "[OK] dotnet-ef installed" -ForegroundColor Green
} else {
    Write-Host "[OK] dotnet-ef found" -ForegroundColor Green
}
Write-Host ""

# Database setup
Write-Host "Setting up database..." -ForegroundColor Yellow
$apiPath = "src\TaskManagement.API"
$infrastructurePath = "src\TaskManagement.Infrastructure"

if (-not (Test-Path $apiPath)) {
    Write-Host "[X] API project not found at $apiPath" -ForegroundColor Red
    exit 1
}

# Wait a bit more for SQL Server to be fully ready
Write-Host "Waiting for SQL Server to be fully ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Update connection string to use Docker SQL Server
$appsettingsPath = "$apiPath\appsettings.json"
$appsettingsContent = Get-Content $appsettingsPath -Raw
$dockerConnectionString = "Server=localhost,1433;Database=TaskManagementDb;User Id=sa;Password=YourStrong@Passw0rd123;TrustServerCertificate=True;MultipleActiveResultSets=true"

# Check if connection string needs updating
if ($appsettingsContent -notmatch "Server=localhost,1433") {
    Write-Host "Updating connection string for Docker SQL Server..." -ForegroundColor Yellow
    $appsettingsContent = $appsettingsContent -replace '(?s)"ConnectionStrings":\s*\{[^}]*"DefaultConnection":\s*"[^"]*"', "`"ConnectionStrings`": {`n    `"DefaultConnection`": `"$dockerConnectionString`""
    Set-Content -Path $appsettingsPath -Value $appsettingsContent -NoNewline
    Write-Host "[OK] Connection string updated" -ForegroundColor Green
}

# Update Windows Service connection string (both appsettings.json and appsettings.Development.json)
$serviceAppsettingsPaths = @(
    "src\TaskManagement.WindowsService\appsettings.json",
    "src\TaskManagement.WindowsService\appsettings.Development.json"
)

foreach ($serviceAppsettingsPath in $serviceAppsettingsPaths) {
    if (Test-Path $serviceAppsettingsPath) {
        $serviceAppsettingsContent = Get-Content $serviceAppsettingsPath -Raw
        # Update if it doesn't already have the Docker connection string
        if ($serviceAppsettingsContent -notmatch "Server=localhost,1433") {
            $serviceAppsettingsContent = $serviceAppsettingsContent -replace '(?s)"ConnectionStrings":\s*\{[^}]*"DefaultConnection":\s*"[^"]*"', "`"ConnectionStrings`": {`n    `"DefaultConnection`": `"$dockerConnectionString`""
            $serviceAppsettingsContent = $serviceAppsettingsContent -replace '(?s)"RabbitMQ":\s*\{[^}]*"HostName":\s*"[^"]*"', "`"RabbitMQ`": {`n    `"HostName`": `"localhost`""
            Set-Content -Path $serviceAppsettingsPath -Value $serviceAppsettingsContent -NoNewline
            Write-Host "[OK] Windows Service configuration updated: $(Split-Path $serviceAppsettingsPath -Leaf)" -ForegroundColor Green
        }
    }
}

# Restore NuGet packages first (restore entire solution from project root)
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
$originalLocation = Get-Location
try {
    # Try to restore from solution file first
    $solutionFile = Get-ChildItem -Path "." -Filter "*.sln" -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($solutionFile) {
        Write-Host "  Restoring solution: $($solutionFile.Name)" -ForegroundColor Gray
        dotnet restore $solutionFile.FullName 2>&1 | Out-Null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "[OK] NuGet packages restored" -ForegroundColor Green
        } else {
            Write-Host "[!] Solution restore failed, trying project restore..." -ForegroundColor Yellow
            # Fallback to project restore
            Set-Location $apiPath
            dotnet restore 2>&1 | Out-Null
            Set-Location "..\.."
            if ($LASTEXITCODE -eq 0) {
                Write-Host "[OK] NuGet packages restored" -ForegroundColor Green
            } else {
                Write-Host "[X] Failed to restore NuGet packages" -ForegroundColor Red
                exit 1
            }
        }
    } else {
        # No solution file, restore from API project
        Write-Host "  No solution file found, restoring from API project..." -ForegroundColor Gray
        Set-Location $apiPath
        dotnet restore 2>&1 | Out-Null
        Set-Location "..\.."
        if ($LASTEXITCODE -eq 0) {
            Write-Host "[OK] NuGet packages restored" -ForegroundColor Green
        } else {
            Write-Host "[X] Failed to restore NuGet packages" -ForegroundColor Red
            exit 1
        }
    }
} catch {
    Write-Host "[X] Error restoring packages: $_" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Run migrations
Write-Host "Running database migrations..." -ForegroundColor Yellow
$originalLocation = Get-Location
Set-Location $apiPath
try {
    # EF Core needs both API and Infrastructure projects restored
    # Restore API project (startup project for EF Core)
    Write-Host "  Ensuring API project is restored..." -ForegroundColor Gray
    $apiProjectPath = Join-Path (Get-Location) "TaskManagement.API.csproj"
    $restoreOutput = dotnet restore $apiProjectPath 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[X] Failed to restore API project" -ForegroundColor Red
        Write-Host "Restore output: $restoreOutput" -ForegroundColor Red
        Set-Location $originalLocation
        exit 1
    }
    
    # Restore Infrastructure project
    Write-Host "  Ensuring Infrastructure project is restored..." -ForegroundColor Gray
    $infraProjectPath = Join-Path (Get-Location) "..\TaskManagement.Infrastructure\TaskManagement.Infrastructure.csproj"
    $infraProjectPath = Resolve-Path $infraProjectPath -ErrorAction SilentlyContinue
    
    if (-not $infraProjectPath) {
        Write-Host "[X] Infrastructure project not found" -ForegroundColor Red
        Set-Location $originalLocation
        exit 1
    }
    
    $restoreOutput = dotnet restore $infraProjectPath 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[X] Failed to restore Infrastructure project" -ForegroundColor Red
        Write-Host "Restore output: $restoreOutput" -ForegroundColor Red
        Set-Location $originalLocation
        exit 1
    }
    
    # Build Infrastructure project to ensure it's ready
    Write-Host "  Building Infrastructure project..." -ForegroundColor Gray
    $buildOutput = dotnet build $infraProjectPath --no-restore 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[X] Failed to build Infrastructure project" -ForegroundColor Red
        Write-Host "Build output: $buildOutput" -ForegroundColor Red
        Set-Location $originalLocation
        exit 1
    }
    
    # Now run migrations (EF Core uses API as startup project, Infrastructure as migration project)
    Write-Host "  Running migrations..." -ForegroundColor Gray
    $migrationOutput = dotnet ef database update --project $infraProjectPath 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[OK] Database migrations applied" -ForegroundColor Green
    } else {
        Write-Host "[X] Database migration failed" -ForegroundColor Red
        Write-Host "Migration output: $migrationOutput" -ForegroundColor Red
        Write-Host "Make sure SQL Server container is running: docker ps" -ForegroundColor Yellow
        Set-Location $originalLocation
        exit 1
    }
} catch {
    Write-Host "[X] Error running migrations: $_" -ForegroundColor Red
    Write-Host "Make sure SQL Server container is running: docker ps" -ForegroundColor Yellow
    Set-Location $originalLocation
    exit 1
}
Set-Location $originalLocation
Write-Host ""

# Frontend setup
Write-Host "Setting up frontend..." -ForegroundColor Yellow
$webPath = "src\TaskManagement.Web"

if (-not (Test-Path $webPath)) {
    Write-Host "[X] Web project not found at $webPath" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path "$webPath\node_modules")) {
    Write-Host "Installing frontend dependencies..." -ForegroundColor Yellow
    Set-Location $webPath
    npm install
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[X] npm install failed" -ForegroundColor Red
        Set-Location "..\.."
        exit 1
    }
    Write-Host "[OK] Frontend dependencies installed" -ForegroundColor Green
    Set-Location "..\.."
} else {
    Write-Host "[OK] Frontend dependencies already installed" -ForegroundColor Green
}
Write-Host ""

# Build solution
Write-Host "Building solution..." -ForegroundColor Yellow
dotnet build
if ($LASTEXITCODE -ne 0) {
    Write-Host "[X] Build failed" -ForegroundColor Red
    exit 1
}
Write-Host "[OK] Build successful" -ForegroundColor Green
Write-Host ""

# Start services
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Starting all services..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Services will start in separate windows:" -ForegroundColor Yellow
Write-Host "  1. API (http://localhost:5063)" -ForegroundColor White
Write-Host "  2. Frontend (http://localhost:5173)" -ForegroundColor White
Write-Host "  3. Windows Service (console)" -ForegroundColor White
Write-Host ""
Write-Host "Docker services are running in the background:" -ForegroundColor Green
Write-Host "  - SQL Server: localhost:1433" -ForegroundColor White
Write-Host "  - RabbitMQ: localhost:5672" -ForegroundColor White
Write-Host "  - RabbitMQ Management: http://localhost:15672" -ForegroundColor White
Write-Host ""
Write-Host "Press Ctrl+C in any window to stop that service" -ForegroundColor Gray
Write-Host ""

# Services are started by start-all.ps1 (called from quick-start script)
# Don't start services here to avoid duplicates
Write-Host "Note: Services will be started by the quick-start script" -ForegroundColor Gray

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "All services are starting!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Access points:" -ForegroundColor Cyan
Write-Host "  - Frontend: http://localhost:5173" -ForegroundColor White
Write-Host "  - API Swagger: http://localhost:5063/swagger" -ForegroundColor White
Write-Host "  - RabbitMQ Management: http://localhost:15672
    Username: guest
    Password: guest" -ForegroundColor White
Write-Host ""
Write-Host "Docker services:" -ForegroundColor Cyan
Write-Host "  - View logs: docker compose logs -f" -ForegroundColor Gray
Write-Host "  - Stop services: docker compose down" -ForegroundColor Gray
Write-Host "  - Restart services: docker compose restart" -ForegroundColor Gray
Write-Host ""
Write-Host "Note: You may need to seed the database first:" -ForegroundColor Yellow
Write-Host "  POST http://localhost:5063/api/seed" -ForegroundColor Gray
Write-Host "  Or use Swagger UI to call the seed endpoint" -ForegroundColor Gray
Write-Host ""
