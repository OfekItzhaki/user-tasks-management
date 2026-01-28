# Fresh Machine Setup Guide

This guide walks you through setting up the Task Management System on a completely new machine from scratch.

## Step 1: Install Prerequisites

Before cloning the repository, install these required tools:

### Required Prerequisites

1. **Git** - [Download Git for Windows](https://git-scm.com/download/win)
   - Verify: `git --version`

2. **.NET 8.0 SDK** - [Download .NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
   - Verify: `dotnet --version` (should show 8.0.x)

3. **Node.js 20.19+ or 22.12+** - [Download Node.js](https://nodejs.org/)
   - This includes npm automatically
   - Verify: `node --version` and `npm --version`

### Choose Your Database Option

#### Option A: Docker (Recommended - Easiest)
4. **Docker Desktop** - [Download Docker Desktop](https://www.docker.com/products/docker-desktop)
   - Includes Docker Compose
   - Verify: `docker --version` and `docker compose version`
   - **No need to install SQL Server or RabbitMQ separately** - Docker handles it!

#### Option B: LocalDB (Traditional)
4. **SQL Server LocalDB** - Usually comes with Visual Studio
   - Or install [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads)
   - Verify: `sqllocaldb info mssqllocaldb`
5. **RabbitMQ** (Optional) - Use Docker: `docker run -d --hostname my-rabbit --name some-rabbit -p 5672:5672 -p 15672:15672 rabbitmq:3-management`

---

## Step 2: Clone the Repository

Open PowerShell and run:

```powershell
git clone https://github.com/OfekItzhaki/user-tasks-management.git
cd user-tasks-management
```

Or if you have SSH configured:

```powershell
git clone git@github.com:OfekItzhaki/user-tasks-management.git
cd user-tasks-management
```

---

## Step 3: Run Quick Start Script

Choose one of these automated scripts based on your setup:

### Option A: Docker Automated (Recommended)

If you installed Docker Desktop, run:

```powershell
.\scripts\quick-start\quick-start-docker-automated.ps1
```

**What it does:**
- ✅ Checks all prerequisites
- ✅ Starts Docker services (SQL Server + RabbitMQ)
- ✅ Installs dotnet-ef tool
- ✅ Runs database migrations
- ✅ Installs frontend dependencies
- ✅ Builds the solution
- ✅ Starts all services (API, Frontend, Windows Service)
- ✅ Optionally seeds the database

### Option B: LocalDB Automated

If you're using LocalDB instead of Docker:

```powershell
.\scripts\quick-start\quick-start-local-automated.ps1
```

**What it does:**
- ✅ Checks all prerequisites
- ✅ Installs dotnet-ef tool
- ✅ Runs database migrations (uses LocalDB)
- ✅ Installs frontend dependencies
- ✅ Builds the solution
- ✅ Starts all services
- ✅ Optionally seeds the database

### Option C: Manual Steps

If you prefer step-by-step guidance:

**Docker:**
```powershell
.\scripts\quick-start\quick-start-docker-manual.ps1
```

**LocalDB:**
```powershell
.\scripts\quick-start\quick-start-local-manual.ps1
```

---

## Step 4: Verify Everything Works

After the quick-start script completes, verify:

1. **API is running**: Open http://localhost:5063/swagger
2. **Frontend is running**: Open http://localhost:5173
3. **RabbitMQ** (if using Docker): Open http://localhost:15672 (guest/guest)

---

## Troubleshooting

If something doesn't work:

1. **Check prerequisites**: Run `.\scripts\verify-services.ps1`
2. **See troubleshooting guide**: Read [TROUBLESHOOTING.md](TROUBLESHOOTING.md)
3. **Check logs**: Look at the PowerShell windows for error messages

---

## Quick Reference

| What | Command |
|------|---------|
| **Clone repository** | `git clone https://github.com/OfekItzhaki/user-tasks-management.git` |
| **Docker quick start** | `.\scripts\quick-start\quick-start-docker-automated.ps1` |
| **LocalDB quick start** | `.\scripts\quick-start\quick-start-local-automated.ps1` |
| **Verify services** | `.\scripts\verify-services.ps1` |
| **Run all tests** | `.\scripts\run-all-tests.ps1` |
| **Stop Docker** | `.\scripts\stop-docker.ps1` |

---

## Next Steps

Once everything is running:
- Read [ARCHITECTURE.md](ARCHITECTURE.md) to understand the project structure
- Read [RUN.md](RUN.md) for daily usage instructions
- Check [TROUBLESHOOTING.md](TROUBLESHOOTING.md) if you encounter issues

---

## Summary: First Step on Fresh Machine

**The very first step is:**

1. **Install Git** (if not already installed)
2. **Install .NET 8.0 SDK**
3. **Install Node.js 20.19+ or 22.12+**
4. **Install Docker Desktop** (recommended) OR **SQL Server LocalDB**
5. **Clone the repository**
6. **Run the quick-start script**

That's it! The quick-start script handles everything else automatically.
