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
    Write-Host "✗ .NET SDK not found" -ForegroundColor Red
    $prerequisitesOk = $false
    $missingPrerequisites += ".NET 8.0 SDK"
} else {
    $version = dotnet --version
    Write-Host "✓ .NET SDK found: $version" -ForegroundColor Green
}

if (-not (Test-Command "node")) {
    Write-Host "✗ Node.js not found" -ForegroundColor Red
    $prerequisitesOk = $false
    $missingPrerequisites += "Node.js 20.19+ or 22.12+"
} else {
    $version = node --version
    Write-Host "✓ Node.js found: $version" -ForegroundColor Green
}

if (-not (Test-Command "docker")) {
    Write-Host "✗ Docker not found" -ForegroundColor Red
    $prerequisitesOk = $false
    $missingPrerequisites += "Docker Desktop"
    Write-Host "  Download: https://www.docker.com/products/docker-desktop" -ForegroundColor Gray
} else {
    $version = docker --version
    Write-Host "✓ Docker found: $version" -ForegroundColor Green
}

if (-not (Test-Command "docker-compose")) {
    if (-not (Test-Command "docker compose")) {
        Write-Host "✗ Docker Compose not found" -ForegroundColor Red
        $prerequisitesOk = $false
        $missingPrerequisites += "Docker Compose (usually included with Docker Desktop)"
    } else {
        Write-Host "✓ Docker Compose found (docker compose)" -ForegroundColor Green
    }
} else {
    Write-Host "✓ Docker Compose found" -ForegroundColor Green
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

# Start Docker Compose services
Write-Host "Starting Docker services (SQL Server and RabbitMQ)..." -ForegroundColor Yellow
Write-Host "This may take a few minutes on first run..." -ForegroundColor Gray
Write-Host ""

# Use docker compose (newer) or docker-compose (older)
$composeCommand = if (Test-Command "docker compose") { "docker compose" } else { "docker-compose" }

try {
    & $composeCommand.Split(' ') up -d
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Failed to start Docker services" -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ Docker services started" -ForegroundColor Green
    Write-Host ""
    
    Write-Host "Waiting for services to be ready..." -ForegroundColor Yellow
    Start-Sleep -Seconds 15
    
    # Check if services are healthy
    $sqlserverHealth = docker inspect --format='{{.State.Health.Status}}' taskmanagement-sqlserver 2>$null
    $rabbitmqHealth = docker inspect --format='{{.State.Health.Status}}' taskmanagement-rabbitmq 2>$null
    
    if ($sqlserverHealth -eq "healthy" -or $sqlserverHealth -eq "starting") {
        Write-Host "✓ SQL Server is ready" -ForegroundColor Green
    } else {
        Write-Host "⚠ SQL Server may still be starting (this is normal)" -ForegroundColor Yellow
    }
    
    if ($rabbitmqHealth -eq "healthy" -or $rabbitmqHealth -eq "starting") {
        Write-Host "✓ RabbitMQ is ready" -ForegroundColor Green
    } else {
        Write-Host "⚠ RabbitMQ may still be starting (this is normal)" -ForegroundColor Yellow
    }
    
} catch {
    Write-Host "✗ Error starting Docker services: $_" -ForegroundColor Red
    Write-Host "Try running: docker compose up -d" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "Docker services are running:" -ForegroundColor Cyan
Write-Host "  - SQL Server: localhost:1433" -ForegroundColor White
Write-Host "  - RabbitMQ: localhost:5672" -ForegroundColor White
Write-Host "  - RabbitMQ Management: http://localhost:15672 (guest/guest)" -ForegroundColor White
Write-Host ""

# Check if dotnet-ef is installed
Write-Host "Checking Entity Framework tools..." -ForegroundColor Yellow
if (-not (Test-Command "dotnet-ef")) {
    Write-Host "Installing dotnet-ef tool..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
    Write-Host "✓ dotnet-ef installed" -ForegroundColor Green
} else {
    Write-Host "✓ dotnet-ef found" -ForegroundColor Green
}
Write-Host ""

# Database setup
Write-Host "Setting up database..." -ForegroundColor Yellow
$apiPath = "src\TaskManagement.API"
$infrastructurePath = "src\TaskManagement.Infrastructure"

if (-not (Test-Path $apiPath)) {
    Write-Host "✗ API project not found at $apiPath" -ForegroundColor Red
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
    Write-Host "✓ Connection string updated" -ForegroundColor Green
}

# Update Windows Service connection string
$serviceAppsettingsPath = "src\TaskManagement.WindowsService\appsettings.json"
if (Test-Path $serviceAppsettingsPath) {
    $serviceAppsettingsContent = Get-Content $serviceAppsettingsPath -Raw
    if ($serviceAppsettingsContent -notmatch "Server=localhost,1433") {
        $serviceAppsettingsContent = $serviceAppsettingsContent -replace '(?s)"ConnectionStrings":\s*\{[^}]*"DefaultConnection":\s*"[^"]*"', "`"ConnectionStrings`": {`n    `"DefaultConnection`": `"$dockerConnectionString`""
        $serviceAppsettingsContent = $serviceAppsettingsContent -replace '(?s)"RabbitMQ":\s*\{[^}]*"HostName":\s*"[^"]*"', "`"RabbitMQ`": {`n    `"HostName`": `"localhost`""
        Set-Content -Path $serviceAppsettingsPath -Value $serviceAppsettingsContent -NoNewline
        Write-Host "✓ Windows Service configuration updated" -ForegroundColor Green
    }
}

# Run migrations
Write-Host "Running database migrations..." -ForegroundColor Yellow
Set-Location $apiPath
try {
    dotnet ef database update --project "..\TaskManagement.Infrastructure" --no-build 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Database migrations applied" -ForegroundColor Green
    } else {
        Write-Host "Running migrations (first time)..." -ForegroundColor Yellow
        dotnet ef database update --project "..\TaskManagement.Infrastructure"
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ Database created and migrations applied" -ForegroundColor Green
        } else {
            Write-Host "✗ Database migration failed" -ForegroundColor Red
            Write-Host "Make sure SQL Server container is running: docker ps" -ForegroundColor Yellow
            Set-Location "..\.."
            exit 1
        }
    }
} catch {
    Write-Host "✗ Error running migrations: $_" -ForegroundColor Red
    Write-Host "Make sure SQL Server container is running: docker ps" -ForegroundColor Yellow
    Set-Location "..\.."
    exit 1
}
Set-Location "..\.."
Write-Host ""

# Frontend setup
Write-Host "Setting up frontend..." -ForegroundColor Yellow
$webPath = "src\TaskManagement.Web"

if (-not (Test-Path $webPath)) {
    Write-Host "✗ Web project not found at $webPath" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path "$webPath\node_modules")) {
    Write-Host "Installing frontend dependencies..." -ForegroundColor Yellow
    Set-Location $webPath
    npm install
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ npm install failed" -ForegroundColor Red
        Set-Location "..\.."
        exit 1
    }
    Write-Host "✓ Frontend dependencies installed" -ForegroundColor Green
    Set-Location "..\.."
} else {
    Write-Host "✓ Frontend dependencies already installed" -ForegroundColor Green
}
Write-Host ""

# Build solution
Write-Host "Building solution..." -ForegroundColor Yellow
dotnet build
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Build failed" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Build successful" -ForegroundColor Green
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

# Start API
Write-Host "Starting API..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PWD\src\TaskManagement.API'; dotnet run" -WindowStyle Normal

# Wait a bit for API to start
Start-Sleep -Seconds 3

# Start Frontend
Write-Host "Starting Frontend..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PWD\src\TaskManagement.Web'; npm run dev" -WindowStyle Normal

# Wait a bit
Start-Sleep -Seconds 2

# Start Windows Service in Development Mode
Write-Host "Starting Windows Service (Development Mode)..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PWD\src\TaskManagement.WindowsService'; Write-Host 'Windows Service (Development Mode)' -ForegroundColor Cyan; dotnet run" -WindowStyle Normal

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "All services are starting!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Access points:" -ForegroundColor Cyan
Write-Host "  - Frontend: http://localhost:5173" -ForegroundColor White
Write-Host "  - API Swagger: http://localhost:5063/swagger" -ForegroundColor White
Write-Host "  - RabbitMQ Management: http://localhost:15672 (guest/guest)" -ForegroundColor White
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
