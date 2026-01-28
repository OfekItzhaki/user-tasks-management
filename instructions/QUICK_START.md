# Quick Start Guide

## For New Machines (First Time Setup)

Run the automated setup script that checks prerequisites, installs dependencies, and starts everything:

```powershell
.\setup.ps1
```

This script will:
1. ✅ Check for prerequisites (.NET SDK, Node.js, SQL Server LocalDB)
2. ✅ Install dotnet-ef tool if needed
3. ✅ Set up the database (create and run migrations)
4. ✅ Install frontend dependencies
5. ✅ Build the solution
6. ✅ Start all services (API, Frontend, Windows Service)

### Windows Service Mode Selection

The script will ask you to choose:
- **Development Mode (Option 1)**: Runs as console app, logs in terminal (default)
- **Production Mode (Option 2)**: Installs as Windows Service, runs in background, logs in Event Viewer

**Note**: Production Mode requires Administrator privileges. If you don't have admin rights, it will fall back to Development Mode.

### Prerequisites

If the script detects missing prerequisites, install them:

1. **.NET 8.0 SDK**: https://dotnet.microsoft.com/download
2. **Node.js 20.19+ or 22.12+**: https://nodejs.org/
3. **SQL Server LocalDB**: Usually comes with Visual Studio, or install SQL Server Express
4. **RabbitMQ** (optional): For Windows Service
   ```powershell
   docker run -d --hostname my-rabbit --name some-rabbit -p 5672:5672 -p 15672:15672 rabbitmq:3-management
   ```

## For Existing Setup (Quick Run)

If everything is already set up, just start the services:

```powershell
.\run.ps1
```

This will:
- Start the API
- Start the Frontend
- Optionally start the Windows Service

## Manual Steps (If Scripts Don't Work)

### 1. Database Setup

```powershell
cd src\TaskManagement.API
dotnet ef database update --project ..\TaskManagement.Infrastructure
```

### 2. Seed Database (Optional)

After starting the API, call:
```
POST http://localhost:5063/api/seed
```

Or use Swagger UI: http://localhost:5063/swagger

### 3. Start Services Manually

**API:**
```powershell
cd src\TaskManagement.API
dotnet run
```

**Frontend:**
```powershell
cd src\TaskManagement.Web
npm install  # First time only
npm run dev
```

**Windows Service:**
```powershell
cd src\TaskManagement.WindowsService
dotnet run
```

## Access Points

- **Frontend**: http://localhost:5173
- **API Swagger**: http://localhost:5063/swagger
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)

## Troubleshooting

### Script Execution Policy

If you get an execution policy error:

```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Port Already in Use

If ports 5063 or 5173 are in use:
- Stop the services using those ports
- Or update the ports in `launchSettings.json` and `vite.config.ts`

### Database Connection Issues

- Ensure SQL Server LocalDB is running
- Check connection string in `appsettings.json`
- Try: `sqllocaldb start mssqllocaldb`

### RabbitMQ Not Running

Start RabbitMQ Docker container:
```powershell
docker start some-rabbit
```

Or check if it's running:
```powershell
docker ps --filter "name=rabbit"
```
