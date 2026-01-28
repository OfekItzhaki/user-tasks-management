# How to Verify Windows Service and RabbitMQ are Running

This guide provides commands and methods to verify that both the Windows Service and RabbitMQ are running correctly.

## 🔍 Quick Verification Commands

### 1. Check Windows Service Status

#### If running as Console App (Development Mode)
The service runs as a console application when you execute:
```powershell
cd src\TaskManagement.WindowsService
dotnet run
```

**What to look for:**
- Console window should be open and showing logs
- You should see: `[SERVICE] Task Reminder Service started and subscribed to 'Remainder' queue`
- Every minute, you should see: `[CHECK] No overdue tasks found` (or task reminders if there are overdue tasks)

#### If installed as Windows Service (Production Mode)
```powershell
# Check if service is installed
Get-Service -Name "TaskReminderService" -ErrorAction SilentlyContinue

# Check service status
Get-Service -Name "TaskReminderService" | Select-Object Name, Status, DisplayName

# View service logs (Event Viewer)
# Open Event Viewer → Windows Logs → Application → Look for "TaskReminderService"
```

**Expected output:**
- `Status: Running` if the service is active
- If not found, the service is running in console mode (development)

---

### 2. Check RabbitMQ Status

#### Option A: Using Docker Compose (Recommended)

```powershell
# Check if RabbitMQ container is running
docker compose ps rabbitmq

# Check RabbitMQ logs
docker compose logs rabbitmq --tail 50

# Check if RabbitMQ port is accessible
Test-NetConnection -ComputerName localhost -Port 5672
Test-NetConnection -ComputerName localhost -Port 15672
```

**Expected output:**
- Container status: `Up` or `running`
- Port 5672 (AMQP): `TcpTestSucceeded : True`
- Port 15672 (Management UI): `TcpTestSucceeded : True`

#### Option B: Using Docker (Standalone)

```powershell
# Check if RabbitMQ container is running
docker ps --filter "name=some-rabbit"

# Check RabbitMQ logs
docker logs some-rabbit --tail 50

# Check if RabbitMQ port is accessible
Test-NetConnection -ComputerName localhost -Port 5672
Test-NetConnection -ComputerName localhost -Port 15672
```

#### Option C: Using RabbitMQ Management UI (Easiest)

1. **Open browser**: http://localhost:15672
2. **Login**: 
   - Username: `guest`
   - Password: `guest`
3. **Check Queues tab**:
   - You should see a queue named `Remainder`
   - Check if messages are being published/consumed

**What to look for:**
- Queue `Remainder` exists
- Messages are being published (Published column increases)
- Messages are being consumed (Consumed column increases)

---

## 🔧 Comprehensive Verification Script

Run this PowerShell script to check everything at once:

```powershell
# Verify Services Script
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
    Write-Host "     Check if console window is open with 'dotnet run'" -ForegroundColor Cyan
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

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Verification Complete" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
```

---

## 🐛 Troubleshooting

### Windows Service Not Running

**Problem**: Service console window is closed or service stopped

**Solution**:
```powershell
# If running in console mode (development)
cd src\TaskManagement.WindowsService
dotnet run

# If installed as Windows Service
Start-Service -Name "TaskReminderService"
```

**Check logs**:
- Console mode: Check the console window output
- Service mode: Check Event Viewer → Windows Logs → Application

---

### RabbitMQ Not Running

**Problem**: RabbitMQ container is stopped or not accessible

**Solution**:
```powershell
# Start RabbitMQ with Docker Compose
docker compose up -d rabbitmq

# OR start standalone RabbitMQ container
docker start some-rabbit

# OR create new RabbitMQ container
docker run -d --hostname my-rabbit --name some-rabbit -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

**Verify connection**:
```powershell
# Check container status
docker ps --filter "name=rabbitmq"

# Check logs for errors
docker compose logs rabbitmq
```

---

### Queue Not Receiving Messages

**Problem**: Queue exists but no messages are being published/consumed

**Check**:
1. **Verify Windows Service is running** (see above)
2. **Check RabbitMQ Management UI**: http://localhost:15672
   - Go to Queues tab
   - Look for `Remainder` queue
   - Check if messages are being published
3. **Check Windows Service logs** for errors:
   - Console mode: Check console output
   - Service mode: Check Event Viewer

**Common issues**:
- Windows Service can't connect to RabbitMQ (check `HostName` in `appsettings.json`)
- Database connection issues (check connection string)
- No overdue tasks in database (service will log: `[CHECK] No overdue tasks found`)

---

## 📊 Monitoring Queue Activity

### Using RabbitMQ Management UI

1. Open: http://localhost:15672
2. Login: `guest` / `guest`
3. Navigate to **Queues** tab
4. Click on `Remainder` queue
5. Monitor:
   - **Ready**: Messages waiting to be consumed
   - **Unacked**: Messages being processed
   - **Published**: Total messages published
   - **Consumed**: Total messages consumed

### Using PowerShell

```powershell
# Check queue status via RabbitMQ Management API
$credentials = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("guest:guest"))
$headers = @{ Authorization = "Basic $credentials" }
$response = Invoke-RestMethod -Uri "http://localhost:15672/api/queues/%2F/Remainder" -Headers $headers
Write-Host "Queue: $($response.name)"
Write-Host "Messages Ready: $($response.messages_ready)"
Write-Host "Messages Unacked: $($response.messages_unacked)"
Write-Host "Messages Published: $($response.message_stats.publish)"
Write-Host "Messages Consumed: $($response.message_stats.consume)"
```

---

## ✅ Quick Health Check

Run these commands to quickly verify everything:

```powershell
# 1. RabbitMQ running?
docker ps --filter "name=rabbitmq" --format "{{.Names}}"

# 2. RabbitMQ accessible?
Test-NetConnection localhost -Port 5672 | Select-Object TcpTestSucceeded

# 3. Windows Service running?
Get-Service -Name "TaskReminderService" -ErrorAction SilentlyContinue | Select-Object Status

# 4. Management UI accessible?
Start-Process "http://localhost:15672"
```

---

## 🎯 Expected Behavior

When everything is working correctly:

1. **Windows Service**:
   - Console shows: `[SERVICE] Task Reminder Service started and subscribed to 'Remainder' queue`
   - Every minute: `[CHECK] No overdue tasks found` (or reminder messages if tasks are overdue)

2. **RabbitMQ**:
   - Container is running
   - Ports 5672 and 15672 are accessible
   - Queue `Remainder` exists in Management UI
   - Messages are published when overdue tasks are found

3. **Queue Activity**:
   - When overdue tasks exist, messages are published to `Remainder` queue
   - Messages are immediately consumed and logged by the Windows Service

---

## 📝 Notes

- **Queue Name**: `Remainder` (as defined in `TaskReminderService.cs`)
- **Check Interval**: Every 1 minute (as defined in `TaskReminderService.cs`)
- **RabbitMQ Host**: `localhost` (or `rabbitmq` if using Docker network)
- **Configuration**: Check `src/TaskManagement.WindowsService/appsettings.json`
