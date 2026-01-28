# Quick Start Scripts

This folder contains scripts to quickly set up and run the Task Management System.

## Available Scripts

### Docker Setup (Recommended)

1. **`quick-start-docker-automated.ps1`** - Fully automated Docker setup
   - Checks all prerequisites
   - Starts Docker services (SQL Server + RabbitMQ)
   - Runs setup script
   - Starts all services
   - **Best for**: First-time setup or when you want everything done automatically

2. **`quick-start-docker-manual.ps1`** - Manual Docker setup with guided steps
   - Checks prerequisites
   - Shows you each command to run
   - Waits for you to complete each step
   - **Best for**: Learning the process or troubleshooting

### Local Setup (Without Docker)

3. **`quick-start-local-automated.ps1`** - Fully automated LocalDB setup
   - Checks prerequisites
   - Uses SQL Server LocalDB (no Docker needed)
   - Runs setup script
   - Starts all services
   - **Best for**: When Docker is not available or preferred

4. **`quick-start-local-manual.ps1`** - Manual LocalDB setup with guided steps
   - Checks prerequisites
   - Shows you each command to run
   - Waits for you to complete each step
   - **Best for**: Learning the process or troubleshooting

### Utility Scripts

5. **`stop-docker.ps1`** - Stops all Docker containers
   - Stops SQL Server and RabbitMQ containers
   - **Best for**: When you want to free up resources

6. **`run-all-tests.ps1`** - Runs all tests (located in root directory)
   - Runs backend (.NET) tests
   - Runs frontend (Vitest) tests
   - Shows summary of results
   - **Usage**: `.\run-all-tests.ps1` (from project root)

## How to Use

### Quick Start (Recommended)

**If you have Docker Desktop:**
```powershell
.\scripts\quick-start-docker-automated.ps1
```

**If you don't have Docker:**
```powershell
.\scripts\quick-start-local-automated.ps1
```

### Manual Steps (For Learning)

**Docker:**
```powershell
.\scripts\quick-start-docker-manual.ps1
```

**Local:**
```powershell
.\scripts\quick-start-local-manual.ps1
```

### Stop Docker Services

```powershell
.\scripts\stop-docker.ps1
```

## Prerequisites

All scripts check prerequisites automatically, but you'll need:

### For Docker Setup:
- .NET 8.0 SDK
- Node.js 20.19+ or 22.12+
- Docker Desktop

### For Local Setup:
- .NET 8.0 SDK
- Node.js 20.19+ or 22.12+
- SQL Server LocalDB (usually comes with Visual Studio)

## What Each Script Does

### Automated Scripts:
1. ✅ Check prerequisites (.NET, Node.js, Docker/LocalDB)
2. ✅ Start Docker services (if using Docker)
3. ✅ Install dotnet-ef tool (if needed)
4. ✅ Run database migrations
5. ✅ Install frontend dependencies
6. ✅ Build the solution
7. ✅ Start all services (API, Frontend, Windows Service)

### Manual Scripts:
- Guide you through each step
- Show you the exact commands to run
- Wait for you to complete each step before continuing

## Troubleshooting

If a script fails:
1. Check the error message
2. Verify prerequisites are installed
3. Try the manual version to see which step fails
4. Check [TROUBLESHOOTING.md](../TROUBLESHOOTING.md) in the root directory

## Notes

- All scripts fix the .NET SDK PATH issue automatically
- Scripts are designed to be run from the project root directory
- Automated scripts will exit if prerequisites are missing
- Manual scripts will guide you through installation if needed
