# Service Status and Fix Guide

## 🔍 Current Status Check

### Database (SQL Server LocalDB)
- **Status:** ✅ **FIXED** - LocalDB started
- **Location:** `(localdb)\mssqllocaldb`
- **Database:** `TaskManagementDb`
- **File Location:** `C:\Users\ofeki\AppData\Local\Microsoft\Microsoft SQL Server Local DB\Instances\MSSQLLocalDB\`

### RabbitMQ
- **Status:** ❌ **NOT RUNNING** - Docker Desktop not running
- **Required:** Docker Desktop must be started
- **Ports:** 5672 (AMQP), 15672 (Management UI)

### Windows Service
- **Status:** ⚠️ **NOT INSTALLED** - Running in console mode
- **Current Mode:** Development (console app)
- **Location:** `src\TaskManagement.WindowsService`

---

## 🛠️ How to Fix

### 1. Start RabbitMQ

**Option A: Using Docker Compose (Recommended)**

1. **Start Docker Desktop:**
   - Open Docker Desktop from Start menu
   - Wait for it to fully start (whale icon in system tray should be steady)
   - Usually takes 30-60 seconds

2. **Start RabbitMQ:**
   ```powershell
   docker compose up -d rabbitmq
   ```

3. **Verify:**
   ```powershell
   docker compose ps rabbitmq
   # Should show: Up (running)
   ```

4. **Check Management UI:**
   - Open browser: http://localhost:15672
   - Login: `guest` / `guest`
   - Should see RabbitMQ Management interface

**Option B: Using Docker (Standalone)**

```powershell
docker run -d --hostname my-rabbit --name some-rabbit -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

**Option C: Install RabbitMQ Locally (Not Recommended)**

1. Download from: https://www.rabbitmq.com/download.html
2. Install and start RabbitMQ service
3. Update `appsettings.json` if needed

---

### 2. Start Windows Service

**Option A: Run as Console App (Development - Current Setup)**

```powershell
cd src\TaskManagement.WindowsService
dotnet run
```

**What to look for:**
- Console window opens
- Shows: `[SERVICE] Task Reminder Service started and subscribed to 'Remainder' queue`
- Every minute: `[CHECK] No overdue tasks found` (or reminders if tasks are overdue)

**Option B: Install as Windows Service (Production)**

1. **Publish the service:**
   ```powershell
   cd src\TaskManagement.WindowsService
   dotnet publish -c Release
   ```

2. **Install as service:**
   ```powershell
   # Run PowerShell as Administrator
   New-Service -Name "TaskReminderService" `
     -BinaryPathName "C:\path\to\TaskManagement.WindowsService.exe" `
     -StartupType Automatic
   ```

3. **Start the service:**
   ```powershell
   Start-Service -Name "TaskReminderService"
   ```

---

## ✅ Verification Steps

### 1. Verify Database
```powershell
# Check LocalDB is running
sqllocaldb info mssqllocaldb
# Should show: State: Running

# Test connection
sqlcmd -S "(localdb)\mssqllocaldb" -Q "SELECT @@VERSION"
```

### 2. Verify RabbitMQ
```powershell
# Check container
docker ps --filter "name=rabbitmq"

# Check ports
Test-NetConnection localhost -Port 5672
Test-NetConnection localhost -Port 15672
# Both should show: TcpTestSucceeded : True

# Check Management UI
Start-Process "http://localhost:15672"
```

### 3. Verify Windows Service
```powershell
# If running as console app, check process
Get-Process | Where-Object {$_.ProcessName -like "*TaskManagement*"}

# If installed as service
Get-Service -Name "TaskReminderService"
# Should show: Status: Running
```

---

## 📋 Quick Start Script

Run this to start everything:

```powershell
# 1. Start LocalDB (if not running)
sqllocaldb start mssqllocaldb

# 2. Start Docker Desktop (manually - open from Start menu)
# Wait for Docker to start, then:

# 3. Start RabbitMQ
docker compose up -d rabbitmq

# 4. Start Windows Service (in separate terminal)
cd src\TaskManagement.WindowsService
dotnet run
```

---

## 🐛 Troubleshooting

### Docker Desktop Not Starting

**Problem:** Docker Desktop fails to start or shows errors

**Solutions:**
1. **Restart Docker Desktop:**
   - Right-click Docker icon in system tray → Restart
   - Or: Close and reopen Docker Desktop

2. **Check Windows Features:**
   - Enable "Virtual Machine Platform" and "Windows Subsystem for Linux"
   - Settings → Apps → Optional Features → More Windows Features

3. **Restart Computer:**
   - Sometimes Docker needs a full restart

4. **Check Docker Desktop Logs:**
   - Docker Desktop → Troubleshoot → View logs

### RabbitMQ Container Won't Start

**Problem:** `docker compose up -d rabbitmq` fails

**Solutions:**
```powershell
# Check Docker is running
docker info

# Check existing containers
docker ps -a

# Remove old container if exists
docker rm -f taskmanagement-rabbitmq

# Start fresh
docker compose up -d rabbitmq

# Check logs
docker compose logs rabbitmq
```

### Windows Service Won't Connect to RabbitMQ

**Problem:** Service shows connection errors

**Solutions:**
1. **Check RabbitMQ is running:**
   ```powershell
   docker ps --filter "name=rabbitmq"
   ```

2. **Check configuration:**
   - File: `src/TaskManagement.WindowsService/appsettings.json`
   - Should have: `"HostName": "localhost"`

3. **If using Docker network:**
   - Change to: `"HostName": "rabbitmq"` (only if service runs in Docker)

4. **Check ports:**
   ```powershell
   Test-NetConnection localhost -Port 5672
   ```

### Database Connection Issues

**Problem:** Cannot connect to LocalDB

**Solutions:**
```powershell
# Start LocalDB
sqllocaldb start mssqllocaldb

# Check status
sqllocaldb info mssqllocaldb

# If still issues, restart LocalDB
sqllocaldb stop mssqllocaldb
sqllocaldb start mssqllocaldb
```

---

## 📊 Expected Behavior When Everything Works

### Database
- ✅ LocalDB running
- ✅ Can connect via SSMS or sqlcmd
- ✅ Database `TaskManagementDb` exists

### RabbitMQ
- ✅ Container running: `docker ps` shows `taskmanagement-rabbitmq`
- ✅ Port 5672 accessible
- ✅ Port 15672 accessible
- ✅ Management UI works: http://localhost:15672

### Windows Service
- ✅ Console shows: `[SERVICE] Task Reminder Service started...`
- ✅ Every minute: `[CHECK] No overdue tasks found`
- ✅ If overdue tasks exist: `[PUBLISH] Reminder published...`

---

## 🎯 Next Steps

1. **Start Docker Desktop** (if not running)
2. **Start RabbitMQ:** `docker compose up -d rabbitmq`
3. **Start Windows Service:** `cd src\TaskManagement.WindowsService && dotnet run`
4. **Verify everything:** Run `.\verify-services.ps1` (after Docker is running)

---

## 📝 Notes

- **LocalDB** starts automatically when accessed, but it's good to start it explicitly
- **Docker Desktop** must be running for RabbitMQ
- **Windows Service** can run in console mode (development) or as a service (production)
- **Queue name:** `Remainder` (as defined in code)
