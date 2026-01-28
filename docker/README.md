# Docker Configuration

This folder contains Docker-related configuration files for the Task Management System.

## Files

- **`docker-compose.yml`** - Main Docker Compose configuration
  - Defines all services: SQL Server, RabbitMQ, API, Frontend, Windows Service
  - Network configuration
  - Volume definitions

- **`docker-compose.override.yml.example`** - Example override file
  - Copy to `docker-compose.override.yml` to customize settings
  - This file is gitignored (not committed)

- **`.dockerignore`** - Files to exclude from Docker builds

## Usage

### Start Docker Services

From the project root:
```powershell
docker compose -f docker/docker-compose.yml up -d sqlserver rabbitmq
```

Or use the quick start scripts:
```powershell
.\scripts\quick-start\quick-start-docker-automated.ps1
```

### Stop Docker Services

```powershell
docker compose -f docker/docker-compose.yml down
```

Or use the utility script:
```powershell
.\scripts\stop-docker.ps1
```

## Services

- **SQL Server**: Port 1433
- **RabbitMQ**: Port 5672 (AMQP), 15672 (Management UI)
- **API**: Port 5063 (when running in Docker)
- **Frontend**: Port 5173 (when running in Docker)
- **Windows Service**: Runs in background (when running in Docker)

## Customization

To customize Docker settings, create `docker-compose.override.yml`:

```powershell
Copy-Item docker/docker-compose.override.yml.example docker/docker-compose.override.yml
```

Then edit `docker-compose.override.yml` with your custom settings.
