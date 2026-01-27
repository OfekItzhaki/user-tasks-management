# Task Management System - Quick Run Script
# Assumes everything is already set up, just starts the services

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Task Management System - Quick Start" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if we're in the right directory
if (-not (Test-Path "src\TaskManagement.API")) {
    Write-Host "✗ Please run this script from the project root directory" -ForegroundColor Red
    exit 1
}

Write-Host "Starting all services..." -ForegroundColor Yellow
Write-Host ""

# Start API
Write-Host "Starting API (http://localhost:5063)..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PWD\src\TaskManagement.API'; Write-Host 'API Starting...' -ForegroundColor Cyan; dotnet run" -WindowStyle Normal

# Wait for API to start
Start-Sleep -Seconds 3

# Start Frontend
Write-Host "Starting Frontend (http://localhost:5173)..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PWD\src\TaskManagement.Web'; Write-Host 'Frontend Starting...' -ForegroundColor Cyan; npm run dev" -WindowStyle Normal

# Wait a bit
Start-Sleep -Seconds 2

# Check if Windows Service is installed and running
$serviceName = "TaskReminderService"
$service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue

if ($service) {
    Write-Host ""
    Write-Host "Windows Service Status:" -ForegroundColor Cyan
    if ($service.Status -eq 'Running') {
        Write-Host "  ✓ Windows Service is running (Production Mode)" -ForegroundColor Green
        Write-Host "  View logs: eventvwr.msc > Windows Logs > Application" -ForegroundColor Gray
    } else {
        Write-Host "  ⚠ Windows Service exists but is not running" -ForegroundColor Yellow
        $startService = Read-Host "Start the Windows Service? (y/n)"
        if ($startService -eq "y" -or $startService -eq "Y") {
            Start-Service -Name $serviceName
            Write-Host "  ✓ Windows Service started" -ForegroundColor Green
        } else {
            Write-Host "Starting in Development Mode (Console)..." -ForegroundColor Yellow
            Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PWD\src\TaskManagement.WindowsService'; Write-Host 'Windows Service (Development Mode)' -ForegroundColor Cyan; dotnet run" -WindowStyle Normal
        }
    }
} else {
    # No service installed, ask if they want to run in dev mode
    Write-Host ""
    $runService = Read-Host "Start Windows Service in Development Mode (Console)? (y/n)"
    if ($runService -eq "y" -or $runService -eq "Y") {
        Write-Host "Starting Windows Service (Development Mode)..." -ForegroundColor Green
        Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PWD\src\TaskManagement.WindowsService'; Write-Host 'Windows Service (Development Mode)' -ForegroundColor Cyan; dotnet run" -WindowStyle Normal
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Services are starting!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Access:" -ForegroundColor Cyan
Write-Host "  Frontend: http://localhost:5173" -ForegroundColor White
Write-Host "  API Swagger: http://localhost:5063/swagger" -ForegroundColor White
Write-Host ""
Write-Host "To stop services, close the PowerShell windows" -ForegroundColor Gray
Write-Host ""
