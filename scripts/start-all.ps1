# Start All Services Script
# Automatically starts API, Frontend, Windows Service, and verifies dependencies

$ErrorActionPreference = "Continue"

# Fix PATH for .NET SDK
$env:DOTNET_ROOT = "C:\Program Files\dotnet"
$env:PATH = "C:\Program Files\dotnet;$env:PATH"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Task Management System - Start All Services" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Function to check if a port is in use
function Test-Port {
    param([int]$Port)
    $connection = Test-NetConnection -ComputerName localhost -Port $Port -WarningAction SilentlyContinue
    return $connection.TcpTestSucceeded
}

# Function to wait for a service to be ready
function Wait-ForService {
    param(
        [string]$ServiceName,
        [int]$Port,
        [int]$MaxWaitSeconds = 30
    )
    Write-Host "Waiting for $ServiceName to be ready on port $Port..." -ForegroundColor Yellow
    $waited = 0
    while (-not (Test-Port -Port $Port) -and $waited -lt $MaxWaitSeconds) {
        Start-Sleep -Seconds 2
        $waited += 2
        Write-Host "." -NoNewline -ForegroundColor Gray
    }
    Write-Host ""
    if (Test-Port -Port $Port) {
        Write-Host "[OK] $ServiceName is ready!" -ForegroundColor Green
        return $true
    } else {
        Write-Host "[X] $ServiceName failed to start within $MaxWaitSeconds seconds" -ForegroundColor Red
        return $false
    }
}

# Check prerequisites
Write-Host "Checking prerequisites..." -ForegroundColor Yellow
Write-Host ""

# Check .NET SDK
try {
    $dotnetVersion = dotnet --version
    Write-Host "[OK] .NET SDK: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "[X] .NET SDK not found. Please install .NET 8.0 SDK" -ForegroundColor Red
    exit 1
}

# Check Node.js
try {
    $nodeVersion = node --version
    Write-Host "[OK] Node.js: $nodeVersion" -ForegroundColor Green
} catch {
    Write-Host "[X] Node.js not found. Please install Node.js 20.19+ or 22.12+" -ForegroundColor Red
    exit 1
}

# Check LocalDB
try {
    $localdbInfo = sqllocaldb info mssqllocaldb 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[OK] SQL Server LocalDB: Available" -ForegroundColor Green
        # Start LocalDB if not running
        sqllocaldb start mssqllocaldb 2>&1 | Out-Null
    } else {
        Write-Host "[!] SQL Server LocalDB: Not found (will try to continue)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "[!] SQL Server LocalDB: Could not check (will try to continue)" -ForegroundColor Yellow
}

# Check RabbitMQ (optional)
$rabbitmqRunning = Test-Port -Port 5672
if ($rabbitmqRunning) {
    Write-Host "[OK] RabbitMQ: Running on port 5672" -ForegroundColor Green
} else {
    Write-Host "[!] RabbitMQ: Not running (Windows Service will work but won't process reminders)" -ForegroundColor Yellow
    Write-Host "  Start with: docker compose up -d rabbitmq" -ForegroundColor Cyan
}

Write-Host ""

# Stop any existing processes
Write-Host "Stopping any existing services..." -ForegroundColor Yellow
Get-Process | Where-Object {
    $_.ProcessName -like "*TaskManagement*" -or
    ($_.ProcessName -eq "dotnet" -and $_.Path -like "*TaskManagement*")
} | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2
Write-Host "[OK] Cleaned up existing processes" -ForegroundColor Green
Write-Host ""

# Start services in separate windows
Write-Host "Starting services..." -ForegroundColor Yellow
Write-Host ""

# 1. Start API
Write-Host "1. Starting API (port 5063/7000)..." -ForegroundColor Cyan
$apiScript = @"
`$env:DOTNET_ROOT = 'C:\Program Files\dotnet'
`$env:PATH = 'C:\Program Files\dotnet;' + `$env:PATH
cd '$PSScriptRoot\src\TaskManagement.API'
Write-Host 'API starting...' -ForegroundColor Green
dotnet run
"@
Start-Process powershell -ArgumentList "-NoExit", "-Command", $apiScript
Start-Sleep -Seconds 3

# 2. Start Frontend
Write-Host "2. Starting Frontend (port 5173)..." -ForegroundColor Cyan
$frontendScript = @"
cd '$PSScriptRoot\src\TaskManagement.Web'
Write-Host 'Frontend starting...' -ForegroundColor Green
npm run dev
"@
Start-Process powershell -ArgumentList "-NoExit", "-Command", $frontendScript
Start-Sleep -Seconds 3

# 3. Start Windows Service
Write-Host "3. Starting Windows Service..." -ForegroundColor Cyan
$serviceScript = @"
`$env:DOTNET_ROOT = 'C:\Program Files\dotnet'
`$env:PATH = 'C:\Program Files\dotnet;' + `$env:PATH
cd '$PSScriptRoot\src\TaskManagement.WindowsService'
Write-Host 'Windows Service starting...' -ForegroundColor Green
Write-Host 'Service will check for overdue tasks every minute' -ForegroundColor Cyan
dotnet run
"@
Start-Process powershell -ArgumentList "-NoExit", "-Command", $serviceScript
Start-Sleep -Seconds 3

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Services Starting..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Services are starting in separate windows:" -ForegroundColor Yellow
Write-Host "  - API: http://localhost:5063 (Swagger: http://localhost:5063/swagger)" -ForegroundColor White
Write-Host "  - Frontend: http://localhost:5173" -ForegroundColor White
Write-Host "  - Windows Service: Console window (checking for overdue tasks)" -ForegroundColor White
Write-Host ""
Write-Host "Waiting for services to be ready..." -ForegroundColor Yellow
Write-Host ""

# Wait for services
$apiReady = Wait-ForService -ServiceName "API" -Port 5063 -MaxWaitSeconds 30
$frontendReady = Wait-ForService -ServiceName "Frontend" -Port 5173 -MaxWaitSeconds 30

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Status Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if ($apiReady) {
    Write-Host "[OK] API: Running" -ForegroundColor Green
    Write-Host "  Swagger: http://localhost:5063/swagger" -ForegroundColor Cyan
} else {
    Write-Host "[X] API: Not responding (check the API window for errors)" -ForegroundColor Red
}

if ($frontendReady) {
    Write-Host "[OK] Frontend: Running" -ForegroundColor Green
    Write-Host "  URL: http://localhost:5173" -ForegroundColor Cyan
} else {
    Write-Host "[X] Frontend: Not responding (check the Frontend window for errors)" -ForegroundColor Red
}

Write-Host "[OK] Windows Service: Started (check the service window)" -ForegroundColor Green

if ($rabbitmqRunning) {
    Write-Host "[OK] RabbitMQ: Running" -ForegroundColor Green
} else {
    Write-Host "[!] RabbitMQ: Not running (reminders won't be processed)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Quick Actions" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "- Open Frontend: http://localhost:5173" -ForegroundColor Cyan
Write-Host "- Open API Swagger: http://localhost:5063/swagger" -ForegroundColor Cyan
Write-Host "- Stop all services: Close the PowerShell windows" -ForegroundColor Cyan
Write-Host "- Start RabbitMQ: docker compose up -d rabbitmq" -ForegroundColor Cyan
Write-Host ""
