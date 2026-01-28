# Installation Guide

## Prerequisites

Install these before running the project:

1. **.NET 8.0 SDK** - [Download](https://dotnet.microsoft.com/download)
2. **Node.js 20.19+ or 22.12+** - [Download](https://nodejs.org/)
3. **SQL Server LocalDB** - Usually comes with Visual Studio, or install [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads)
4. **Docker Desktop** (Optional) - [Download](https://www.docker.com/products/docker-desktop) - For SQL Server and RabbitMQ

## Quick Setup (Recommended)

Run the automated setup script:

```powershell
.\setup.ps1
```

This will:
- ✅ Check prerequisites
- ✅ Install dotnet-ef tool
- ✅ Set up database (migrations)
- ✅ Install frontend dependencies
- ✅ Build the solution
- ✅ Start all services

## Docker Setup (Alternative)

If you prefer Docker for SQL Server and RabbitMQ:

```powershell
.\setup-docker.ps1
```

This uses Docker Compose to run SQL Server and RabbitMQ in containers.

## Manual Setup

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

### 3. Frontend Dependencies

```powershell
cd src\TaskManagement.Web
npm install
```

## Verify Installation

```powershell
dotnet --version    # Should show 8.0.x
node --version      # Should show 20.x or 22.x
npm --version       # Should show 10.x or 11.x
```

## Next Steps

After installation, see [RUN.md](RUN.md) to start the application.
