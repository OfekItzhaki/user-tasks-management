# Start RabbitMQ Script
# Starts RabbitMQ using Docker Compose

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Starting RabbitMQ" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if Docker is running
Write-Host "Checking Docker..." -ForegroundColor Yellow
try {
    docker ps | Out-Null
    Write-Host "[OK] Docker is running" -ForegroundColor Green
} catch {
    Write-Host "[X] Docker is not running" -ForegroundColor Red
    Write-Host "Please start Docker Desktop and try again." -ForegroundColor Yellow
    exit 1
}

Write-Host ""

# Get project root directory
$projectRoot = Join-Path $PSScriptRoot ".."
$dockerComposePath = Join-Path $projectRoot "docker\docker-compose.yml"

if (-not (Test-Path $dockerComposePath)) {
    Write-Host "[X] docker-compose.yml not found at: $dockerComposePath" -ForegroundColor Red
    exit 1
}

# Start RabbitMQ
Write-Host "Starting RabbitMQ container..." -ForegroundColor Yellow
Push-Location $projectRoot
try {
    docker compose -f $dockerComposePath --project-directory $projectRoot up -d rabbitmq
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[OK] RabbitMQ started successfully" -ForegroundColor Green
        Write-Host ""
        Write-Host "Waiting for RabbitMQ to be ready..." -ForegroundColor Yellow
        Start-Sleep -Seconds 10
        
        # Check if RabbitMQ is accessible
        $rabbitmqReady = $false
        $maxWait = 30
        $waited = 0
        
        while (-not $rabbitmqReady -and $waited -lt $maxWait) {
            try {
                $connection = Test-NetConnection -ComputerName localhost -Port 5672 -WarningAction SilentlyContinue
                if ($connection.TcpTestSucceeded) {
                    $rabbitmqReady = $true
                }
            } catch {
                Start-Sleep -Seconds 2
                $waited += 2
                Write-Host "." -NoNewline -ForegroundColor Gray
            }
        }
        
        Write-Host ""
        if ($rabbitmqReady) {
            Write-Host "[OK] RabbitMQ is ready!" -ForegroundColor Green
            Write-Host ""
            Write-Host "RabbitMQ Management UI: http://localhost:15672 (guest/guest)" -ForegroundColor Cyan
            Write-Host "AMQP Port: localhost:5672" -ForegroundColor Cyan
        } else {
            Write-Host "[!] RabbitMQ may still be starting (this is normal)" -ForegroundColor Yellow
            Write-Host "  Check status with: docker ps --filter name=rabbitmq" -ForegroundColor Gray
        }
    } else {
        Write-Host "[X] Failed to start RabbitMQ" -ForegroundColor Red
        Pop-Location
        exit 1
    }
} catch {
    Write-Host "[X] Error starting RabbitMQ: $_" -ForegroundColor Red
    Pop-Location
    exit 1
}
Pop-Location

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Done!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
