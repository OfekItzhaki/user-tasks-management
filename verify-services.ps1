# Service Verification Script
# Checks Windows Service and RabbitMQ status

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Service Verification" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 1. Check RabbitMQ Container
Write-Host "1. Checking RabbitMQ..." -ForegroundColor Yellow
$rabbitmqContainer = docker ps --filter "name=rabbitmq" --format "{{.Names}}" | Select-Object -First 1
if ($rabbitmqContainer) {
    Write-Host "   ✓ RabbitMQ container is running: $rabbitmqContainer" -ForegroundColor Green
} else {
    $rabbitmqStandalone = docker ps --filter "name=some-rabbit" --format "{{.Names}}" | Select-Object -First 1
    if ($rabbitmqStandalone) {
        Write-Host "   ✓ RabbitMQ container is running: $rabbitmqStandalone" -ForegroundColor Green
    } else {
        Write-Host "   ✗ RabbitMQ container is NOT running" -ForegroundColor Red
        Write-Host "     Start it with: docker compose up -d rabbitmq" -ForegroundColor Yellow
        Write-Host "     Or: docker run -d --hostname my-rabbit --name some-rabbit -p 5672:5672 -p 15672:15672 rabbitmq:3-management" -ForegroundColor Yellow
    }
}

# 2. Check RabbitMQ Ports
Write-Host ""
Write-Host "2. Checking RabbitMQ Ports..." -ForegroundColor Yellow
$port5672 = Test-NetConnection -ComputerName localhost -Port 5672 -WarningAction SilentlyContinue
$port15672 = Test-NetConnection -ComputerName localhost -Port 15672 -WarningAction SilentlyContinue

if ($port5672.TcpTestSucceeded) {
    Write-Host "   ✓ Port 5672 (AMQP) is accessible" -ForegroundColor Green
} else {
    Write-Host "   ✗ Port 5672 (AMQP) is NOT accessible" -ForegroundColor Red
}

if ($port15672.TcpTestSucceeded) {
    Write-Host "   ✓ Port 15672 (Management UI) is accessible" -ForegroundColor Green
    Write-Host "     Management UI: http://localhost:15672 (guest/guest)" -ForegroundColor Cyan
} else {
    Write-Host "   ✗ Port 15672 (Management UI) is NOT accessible" -ForegroundColor Red
}

# 3. Check Windows Service (if installed)
Write-Host ""
Write-Host "3. Checking Windows Service..." -ForegroundColor Yellow
$service = Get-Service -Name "TaskReminderService" -ErrorAction SilentlyContinue
if ($service) {
    if ($service.Status -eq "Running") {
        Write-Host "   ✓ Windows Service is running" -ForegroundColor Green
    } else {
        Write-Host "   ✗ Windows Service is installed but NOT running (Status: $($service.Status))" -ForegroundColor Red
        Write-Host "     Start it with: Start-Service -Name TaskReminderService" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ℹ Windows Service is not installed (running in console mode)" -ForegroundColor Cyan
    Write-Host "     Check if console window is open with 'dotnet run' in src\TaskManagement.WindowsService" -ForegroundColor Cyan
}

# 4. Check Database Connection
Write-Host ""
Write-Host "4. Checking Database Connection..." -ForegroundColor Yellow
try {
    $dbCheck = sqllocaldb info mssqllocaldb 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ✓ SQL Server LocalDB is available" -ForegroundColor Green
    } else {
        Write-Host "   ✗ SQL Server LocalDB is NOT available" -ForegroundColor Red
    }
} catch {
    Write-Host "   ✗ Could not check SQL Server LocalDB" -ForegroundColor Red
}

# 5. Check Queue Status (if RabbitMQ is accessible)
Write-Host ""
Write-Host "5. Checking Queue Status..." -ForegroundColor Yellow
if ($port15672.TcpTestSucceeded) {
    try {
        $credentials = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("guest:guest"))
        $headers = @{ Authorization = "Basic $credentials" }
        $response = Invoke-RestMethod -Uri "http://localhost:15672/api/queues/%2F/Remainder" -Headers $headers -ErrorAction SilentlyContinue
        
        if ($response) {
            Write-Host "   ✓ Queue 'Remainder' exists" -ForegroundColor Green
            Write-Host "     Messages Ready: $($response.messages_ready)" -ForegroundColor Cyan
            Write-Host "     Messages Unacked: $($response.messages_unacked)" -ForegroundColor Cyan
            if ($response.message_stats) {
                Write-Host "     Messages Published: $($response.message_stats.publish)" -ForegroundColor Cyan
                Write-Host "     Messages Consumed: $($response.message_stats.consume)" -ForegroundColor Cyan
            }
        } else {
            Write-Host "   ⚠ Queue 'Remainder' does not exist yet (will be created when service starts)" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "   ⚠ Could not check queue status (queue may not exist yet)" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ⚠ Cannot check queue status (RabbitMQ Management UI not accessible)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Verification Complete" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Quick Actions:" -ForegroundColor Yellow
Write-Host "  • Open RabbitMQ Management UI: http://localhost:15672 (guest/guest)" -ForegroundColor Cyan
Write-Host "  • Start Windows Service: cd src\TaskManagement.WindowsService && dotnet run" -ForegroundColor Cyan
Write-Host "  • Start RabbitMQ: docker compose up -d rabbitmq" -ForegroundColor Cyan
Write-Host ""
