# Task Management System - Automated Setup and Run Script
# This script checks prerequisites, installs if needed, and runs all services

param(
    [switch]$SkipChecks = $false,
    [switch]$InstallPrerequisites = $false
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Task Management System - Setup & Run" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Function to check if a command exists
function Test-Command {
    param([string]$Command)
    $null = Get-Command $Command -ErrorAction SilentlyContinue
    return $?
}

# Function to check .NET SDK version
function Test-DotNetSDK {
    if (Test-Command "dotnet") {
        $version = dotnet --version
        Write-Host "✓ .NET SDK found: $version" -ForegroundColor Green
        return $true
    }
    return $false
}

# Function to check Node.js version
function Test-NodeJS {
    if (Test-Command "node") {
        $version = node --version
        Write-Host "✓ Node.js found: $version" -ForegroundColor Green
        return $true
    }
    return $false
}

# Function to check SQL Server LocalDB
function Test-SqlLocalDB {
    try {
        $sqllocaldb = Get-Command sqllocaldb -ErrorAction SilentlyContinue
        if ($sqllocaldb) {
            Write-Host "✓ SQL Server LocalDB found" -ForegroundColor Green
            return $true
        }
    } catch {
        # LocalDB might be installed but not in PATH
        $localdbPath = "C:\Program Files\Microsoft SQL Server\150\Tools\Binn\SqlLocalDB.exe"
        if (Test-Path $localdbPath) {
            Write-Host "✓ SQL Server LocalDB found" -ForegroundColor Green
            return $true
        }
    }
    return $false
}

# Function to check RabbitMQ
function Test-RabbitMQ {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:15672" -TimeoutSec 2 -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) {
            Write-Host "✓ RabbitMQ is running" -ForegroundColor Green
            return $true
        }
    } catch {
        # Check if Docker container is running
        if (Test-Command "docker") {
            $containers = docker ps --filter "name=rabbit" --format "{{.Names}}" 2>$null
            if ($containers) {
                Write-Host "✓ RabbitMQ Docker container is running" -ForegroundColor Green
                return $true
            }
        }
    }
    return $false
}

# Check prerequisites
Write-Host "Checking prerequisites..." -ForegroundColor Yellow
Write-Host ""

$prerequisitesOk = $true
$missingPrerequisites = @()

if (-not (Test-DotNetSDK)) {
    Write-Host "✗ .NET SDK not found" -ForegroundColor Red
    $prerequisitesOk = $false
    $missingPrerequisites += ".NET 8.0 SDK"
}

if (-not (Test-NodeJS)) {
    Write-Host "✗ Node.js not found" -ForegroundColor Red
    $prerequisitesOk = $false
    $missingPrerequisites += "Node.js 20.19+ or 22.12+"
}

if (-not (Test-SqlLocalDB)) {
    Write-Host "✗ SQL Server LocalDB not found" -ForegroundColor Red
    $prerequisitesOk = $false
    $missingPrerequisites += "SQL Server LocalDB"
}

if (-not (Test-RabbitMQ)) {
    Write-Host "⚠ RabbitMQ not running (optional for Windows Service)" -ForegroundColor Yellow
    Write-Host "  You can start it later with: docker start some-rabbit" -ForegroundColor Gray
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
    Write-Host "  3. SQL Server LocalDB: Usually comes with Visual Studio" -ForegroundColor White
    Write-Host "     Or install SQL Server Express: https://www.microsoft.com/sql-server/sql-server-downloads" -ForegroundColor White
    Write-Host ""
    Write-Host "After installing, run this script again." -ForegroundColor Yellow
    exit 1
}

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

# Check if database exists and run migrations
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
            exit 1
        }
    }
} catch {
    Write-Host "✗ Error running migrations: $_" -ForegroundColor Red
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

# Ask about Windows Service mode
Write-Host ""
Write-Host "Windows Service Options:" -ForegroundColor Cyan
Write-Host "  1. Development Mode (Console App) - Easy to test, logs in console" -ForegroundColor White
Write-Host "  2. Production Mode (Windows Service) - Runs in background, logs in Event Viewer" -ForegroundColor White
Write-Host ""
$serviceMode = Read-Host "Choose mode (1=Development, 2=Production, or press Enter for Development)"

if ($serviceMode -eq "2") {
    Write-Host ""
    Write-Host "Installing as Windows Service (Production Mode)..." -ForegroundColor Yellow
    Write-Host "This requires Administrator privileges." -ForegroundColor Yellow
    Write-Host ""
    
    # Check if running as admin
    $isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
    
    if (-not $isAdmin) {
        Write-Host "⚠ Administrator privileges required for Windows Service installation." -ForegroundColor Yellow
        Write-Host "Please run PowerShell as Administrator and run this script again." -ForegroundColor Yellow
        Write-Host "Or choose Development Mode (option 1) to run as console app." -ForegroundColor Yellow
        Write-Host ""
        $continueDev = Read-Host "Continue with Development Mode instead? (y/n)"
        if ($continueDev -eq "y" -or $continueDev -eq "Y") {
            Write-Host "Starting Windows Service in Development Mode..." -ForegroundColor Yellow
            Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PWD\src\TaskManagement.WindowsService'; Write-Host 'Windows Service (Development Mode)' -ForegroundColor Cyan; dotnet run" -WindowStyle Normal
        }
    } else {
        # Install as Windows Service
        $serviceName = "TaskReminderService"
        $servicePath = "$PWD\src\TaskManagement.WindowsService"
        $publishPath = "C:\Services\TaskReminderService"
        
        Write-Host "Publishing service..." -ForegroundColor Yellow
        Set-Location $servicePath
        dotnet publish -c Release -o $publishPath
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ Service published successfully" -ForegroundColor Green
            
            # Check if service already exists
            $existingService = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
            if ($existingService) {
                Write-Host "Service already exists. Stopping and removing..." -ForegroundColor Yellow
                Stop-Service -Name $serviceName -Force -ErrorAction SilentlyContinue
                sc.exe delete $serviceName | Out-Null
                Start-Sleep -Seconds 2
            }
            
            Write-Host "Creating Windows Service..." -ForegroundColor Yellow
            $exePath = "$publishPath\TaskManagement.WindowsService.exe"
            sc.exe create $serviceName binPath= "`"$exePath`"" start= auto | Out-Null
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✓ Windows Service created successfully" -ForegroundColor Green
                Write-Host "Starting service..." -ForegroundColor Yellow
                Start-Service -Name $serviceName
                Write-Host "✓ Windows Service started" -ForegroundColor Green
                Write-Host ""
                Write-Host "Service is now running in the background!" -ForegroundColor Green
                Write-Host "View logs in Event Viewer: eventvwr.msc" -ForegroundColor Cyan
            } else {
                Write-Host "✗ Failed to create Windows Service" -ForegroundColor Red
                Write-Host "Falling back to Development Mode..." -ForegroundColor Yellow
                Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PWD'; Write-Host 'Windows Service (Development Mode)' -ForegroundColor Cyan; dotnet run" -WindowStyle Normal
            }
        } else {
            Write-Host "✗ Failed to publish service" -ForegroundColor Red
            Write-Host "Falling back to Development Mode..." -ForegroundColor Yellow
            Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PWD'; Write-Host 'Windows Service (Development Mode)' -ForegroundColor Cyan; dotnet run" -WindowStyle Normal
        }
        
        Set-Location "..\.."
    }
} else {
    # Development Mode (default)
    Write-Host "Starting Windows Service in Development Mode (Console)..." -ForegroundColor Yellow
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PWD\src\TaskManagement.WindowsService'; Write-Host 'Windows Service (Development Mode)' -ForegroundColor Cyan; dotnet run" -WindowStyle Normal
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "All services are starting!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Access points:" -ForegroundColor Cyan
Write-Host "  - Frontend: http://localhost:5173" -ForegroundColor White
Write-Host "  - API Swagger: http://localhost:5063/swagger" -ForegroundColor White
Write-Host ""
Write-Host "Note: You may need to seed the database first:" -ForegroundColor Yellow
Write-Host "  POST http://localhost:5063/api/seed" -ForegroundColor Gray
Write-Host "  Or use Swagger UI to call the seed endpoint" -ForegroundColor Gray
Write-Host ""
