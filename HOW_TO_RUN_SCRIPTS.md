# How to Run PowerShell Scripts (.ps1 files)

This guide explains how to run the PowerShell scripts in this project.

## 🚀 Quick Start

### Method 1: Run from PowerShell (Recommended)

1. **Open PowerShell**:
   - Press `Win + X` and select "Windows PowerShell" or "Terminal"
   - Or search for "PowerShell" in the Start menu

2. **Navigate to the project directory**:
   ```powershell
   cd C:\Users\ofeki\Desktop\UserTasks
   ```

3. **Run the script**:
   ```powershell
   .\verify-services.ps1
   ```
   or
   ```powershell
   .\setup.ps1
   ```
   or
   ```powershell
   .\setup-docker.ps1
   ```

### Method 2: Run from Command Prompt

1. **Open Command Prompt** (cmd)
2. **Navigate to the project directory**
3. **Run with PowerShell**:
   ```cmd
   powershell -ExecutionPolicy Bypass -File .\verify-services.ps1
   ```

### Method 3: Right-Click and Run

1. **Right-click** on the `.ps1` file in File Explorer
2. Select **"Run with PowerShell"**

---

## ⚠️ Execution Policy Issues

If you get an error like:
```
.\verify-services.ps1 : File cannot be loaded because running scripts is disabled on this system.
```

This means your PowerShell execution policy is blocking scripts. Here's how to fix it:

### Option 1: Bypass for Current Session (Temporary)

Run this command before running the script:
```powershell
Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process
```

Then run your script:
```powershell
.\verify-services.ps1
```

**Note**: This only affects the current PowerShell session. You'll need to do this again in a new session.

### Option 2: Bypass for Current User (Recommended)

This allows you to run scripts without the bypass command each time:
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

**What this does**:
- Allows you to run local scripts (like ours)
- Still requires signed scripts from the internet
- Only affects your user account (not system-wide)

### Option 3: Run with Bypass Flag

Run the script with the bypass flag directly:
```powershell
powershell -ExecutionPolicy Bypass -File .\verify-services.ps1
```

---

## 📋 Available Scripts in This Project

### 1. `verify-services.ps1`
**Purpose**: Check if Windows Service and RabbitMQ are running

**Usage**:
```powershell
.\verify-services.ps1
```

**What it checks**:
- Docker Desktop status
- RabbitMQ container and ports
- Windows Service status
- Database connection
- Queue status

---

### 2. `setup.ps1`
**Purpose**: Automated setup for new machines (local setup)

**Usage**:
```powershell
.\setup.ps1
```

**What it does**:
- Checks prerequisites (.NET SDK, Node.js, SQL Server LocalDB)
- Installs dotnet-ef tool if needed
- Sets up database (migrations)
- Installs frontend dependencies
- Builds the solution
- Starts all services

---

### 3. `setup-docker.ps1`
**Purpose**: Automated setup using Docker (recommended)

**Usage**:
```powershell
.\setup-docker.ps1
```

**What it does**:
- Checks prerequisites (.NET SDK, Node.js, Docker Desktop)
- Starts Docker services (SQL Server and RabbitMQ)
- Sets up database (migrations)
- Installs frontend dependencies
- Builds the solution
- Starts all services

---

### 4. `run.ps1`
**Purpose**: Quick start for existing setup

**Usage**:
```powershell
.\run.ps1
```

**What it does**:
- Starts all services without running setup checks
- Assumes everything is already configured

---

## 🔍 Check Current Execution Policy

To see your current execution policy:
```powershell
Get-ExecutionPolicy
```

**Common values**:
- `Restricted` - Scripts are blocked (default on some systems)
- `RemoteSigned` - Local scripts allowed, remote scripts must be signed
- `Unrestricted` - All scripts allowed (not recommended)
- `Bypass` - No restrictions (temporary, per session)

---

## 💡 Tips

1. **Always run from the project root directory**:
   ```powershell
   cd C:\Users\ofeki\Desktop\UserTasks
   ```

2. **Use relative paths** (the `.\` prefix):
   ```powershell
   .\verify-services.ps1  # ✅ Correct
   verify-services.ps1    # ✅ Also works
   C:\Users\ofeki\Desktop\UserTasks\verify-services.ps1  # ✅ Also works
   ```

3. **Run as Administrator** (if needed):
   - Right-click PowerShell → "Run as Administrator"
   - Some scripts may need admin rights (e.g., installing Windows Service)

4. **Check script output**:
   - Scripts will show colored output (green ✓, red ✗, yellow ⚠)
   - Read any error messages carefully
   - Check the troubleshooting section in README.md if issues occur

---

## 🐛 Troubleshooting

### "Script cannot be loaded because running scripts is disabled"

**Solution**: See [Execution Policy Issues](#-execution-policy-issues) above.

### "The term 'docker' is not recognized"

**Solution**: 
- Install Docker Desktop: https://www.docker.com/products/docker-desktop
- Make sure Docker Desktop is running
- Restart PowerShell after installation

### "The term 'dotnet' is not recognized"

**Solution**:
- Install .NET 8.0 SDK: https://dotnet.microsoft.com/download/dotnet/8.0
- Restart PowerShell after installation

### "Access Denied" or "Permission Denied"

**Solution**:
- Run PowerShell as Administrator
- Right-click PowerShell → "Run as Administrator"

---

## 📚 Additional Resources

- [PowerShell Execution Policies](https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_execution_policies)
- [PowerShell Scripting Guide](https://learn.microsoft.com/en-us/powershell/scripting/overview)
