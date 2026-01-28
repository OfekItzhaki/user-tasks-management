# Scripts Directory

This folder contains all PowerShell scripts for the Task Management System.

## Directory Structure

```
scripts/
├── quick-start/          # Quick start scripts (4 files)
├── setup.ps1            # Automated setup (LocalDB)
├── setup-docker.ps1     # Automated setup (Docker)
├── start-all.ps1        # Start all services
├── run.ps1              # Quick run script
├── run-all-tests.ps1    # Run all tests (backend + frontend)
├── stop-docker.ps1      # Stop Docker services
├── verify-services.ps1  # Verify service status
└── README.md            # This file
```

## Quick Start Scripts

Located in `scripts/quick-start/`:

1. **`quick-start-docker-automated.ps1`** - Fully automated Docker setup
2. **`quick-start-docker-manual.ps1`** - Manual Docker setup with guidance
3. **`quick-start-local-automated.ps1`** - Fully automated LocalDB setup
4. **`quick-start-local-manual.ps1`** - Manual LocalDB setup with guidance

See [quick-start/README.md](quick-start/README.md) for details.

## Setup Scripts

### `setup.ps1`
Automated setup script for LocalDB (no Docker required).
- Checks prerequisites
- Installs dotnet-ef tool
- Sets up database (migrations)
- Installs frontend dependencies
- Builds the solution
- Starts all services

**Usage:**
```powershell
.\scripts\setup.ps1
```

### `setup-docker.ps1`
Automated setup script using Docker for SQL Server and RabbitMQ.
- Checks prerequisites
- Starts Docker services
- Installs dotnet-ef tool
- Sets up database (migrations)
- Installs frontend dependencies
- Builds the solution
- Starts all services

**Usage:**
```powershell
.\scripts\setup-docker.ps1
```

## Run Scripts

### `start-all.ps1`
Starts all services (API, Frontend, Windows Service) in separate windows.

**Usage:**
```powershell
.\scripts\start-all.ps1
```

### `run.ps1`
Quick run script for existing setups (skips setup checks).

**Usage:**
```powershell
.\scripts\run.ps1
```

### `run-all-tests.ps1`
Runs both backend (.NET) and frontend (Vitest) tests.

**Usage:**
```powershell
.\scripts\run-all-tests.ps1
```

## Utility Scripts

### `stop-docker.ps1`
Stops all Docker containers (SQL Server and RabbitMQ).

**Usage:**
```powershell
.\scripts\stop-docker.ps1
```

### `verify-services.ps1`
Checks the status of all services (API, Frontend, RabbitMQ, SQL Server).

**Usage:**
```powershell
.\scripts\verify-services.ps1
```

## Quick Reference

### First Time Setup

**With Docker (Recommended):**
```powershell
.\scripts\quick-start\quick-start-docker-automated.ps1
```

**Without Docker:**
```powershell
.\scripts\quick-start\quick-start-local-automated.ps1
```

### Daily Use

**Start everything:**
```powershell
.\scripts\start-all.ps1
```

**Run tests:**
```powershell
.\scripts\run-all-tests.ps1
```

**Stop Docker:**
```powershell
.\scripts\stop-docker.ps1
```

## Notes

- All scripts automatically fix the .NET SDK PATH issue
- Scripts should be run from the project root directory
- Quick start scripts include database seeding options
- All scripts provide clear feedback and error messages
