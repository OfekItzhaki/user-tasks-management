# Project Structure

This document describes the organization of the Task Management System project.

## Root Directory

The root directory contains only **essential files** needed to understand and run the project:

### Essential Documentation
- **`README.md`** - Main project documentation and overview
- **`INSTALL.md`** - Installation and setup guide
- **`RUN.md`** - How to run the application
- **`TROUBLESHOOTING.md`** - Common issues and solutions
- **`ARCHITECTURE.md`** - Project architecture and layer structure

### Project Files
- **`TaskManagement.sln`** - Visual Studio solution file

## Organized Folders

### `src/` - Source Code
Contains all application source code:
- `TaskManagement.API/` - REST API layer
- `TaskManagement.Application/` - Business logic (CQRS)
- `TaskManagement.Domain/` - Domain entities
- `TaskManagement.Infrastructure/` - Data access layer
- `TaskManagement.Web/` - React frontend
- `TaskManagement.WindowsService/` - Background service
- `TaskManagement.Tests/` - Unit and integration tests

### `scripts/` - PowerShell Scripts
All automation scripts organized by purpose:

**`scripts/quick-start/`** - Quick start scripts (4 files)
- `quick-start-docker-automated.ps1` - Fully automated Docker setup
- `quick-start-docker-manual.ps1` - Manual Docker setup with guidance
- `quick-start-local-automated.ps1` - Fully automated LocalDB setup
- `quick-start-local-manual.ps1` - Manual LocalDB setup with guidance

**`scripts/`** - Other utility scripts (7 files)
- `setup.ps1` - Automated setup (LocalDB)
- `setup-docker.ps1` - Automated setup (Docker)
- `start-all.ps1` - Start all services
- `run.ps1` - Quick run script
- `run-all-tests.ps1` - Run all tests
- `stop-docker.ps1` - Stop Docker services
- `verify-services.ps1` - Verify service status

### `docker/` - Docker Configuration
Docker-related files:
- `docker-compose.yml` - Main Docker Compose configuration
- `docker-compose.override.yml.example` - Example override file
- `.dockerignore` - Docker build exclusions
- `README.md` - Docker documentation

### `instructions/` - Essential Developer Guides
**INCLUDED in git commits** - Essential guides for developers:
- `DATABASE_ACCESS_GUIDE.md` - How to access the database
- `DOCKER_SETUP.md` - Docker setup guide
- `SERVICE_STATUS_AND_FIX.md` - Service troubleshooting
- `VERIFY_SERVICES.md` - Service verification guide
- `fix-dotnet-path.ps1` - Quick PATH fix utility
- `fix-path-permanent.ps1` - Permanent PATH fix utility

### `Extra instructions/` - Development Notes
**EXCLUDED from git commits** - Historical and development notes:
- Development history and implementation notes
- Detailed technical documentation
- Redundant guides (superseded by main docs)
- See `Extra instructions/README.md` for details

## File Organization Principles

1. **Root Directory**: Only essential files that new developers need immediately
2. **Organized Folders**: Related files grouped logically
3. **Clear Separation**: Essential vs. optional documentation
4. **Easy Navigation**: Logical folder structure

## Quick Access

### For New Developers
1. Start with `README.md`
2. Follow `INSTALL.md` for setup
3. Use `scripts/quick-start/` for automated setup
4. Refer to `instructions/` for specific guides

### For Daily Development
- Run: `.\scripts\start-all.ps1`
- Test: `.\scripts\run-all-tests.ps1`
- Stop: `.\scripts\stop-docker.ps1`

### For Troubleshooting
- Check `TROUBLESHOOTING.md`
- See `instructions/SERVICE_STATUS_AND_FIX.md`
- Run `.\scripts\verify-services.ps1`
