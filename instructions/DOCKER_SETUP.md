# Docker Setup Guide

This guide explains how to use Docker Compose for the easiest setup experience.

## Why Docker?

- ✅ **No local SQL Server installation** - SQL Server runs in a container
- ✅ **No local RabbitMQ installation** - RabbitMQ runs in a container
- ✅ **Consistent environment** - Same setup on every machine
- ✅ **Easy cleanup** - Remove everything with one command
- ✅ **Isolated** - Doesn't affect your local system

## Quick Start

### 1. Install Docker Desktop

Download and install [Docker Desktop](https://www.docker.com/products/docker-desktop) for Windows.

Verify installation:
```powershell
docker --version
docker compose version
```

### 2. Run Setup Script

```powershell
.\setup-docker.ps1
```

That's it! The script will:
- Start SQL Server and RabbitMQ in Docker containers
- Set up the database
- Install dependencies
- Start all services

## Manual Docker Setup

If you prefer to set up manually:

### 1. Start Docker Services

```powershell
docker compose up -d
```

This starts:
- SQL Server on port 1433
- RabbitMQ on port 5672 (Management UI: http://localhost:15672)

### 2. Verify Services Are Running

```powershell
docker compose ps
```

You should see both services with "Up" status.

### 3. Check Service Logs

```powershell
# SQL Server logs
docker compose logs sqlserver

# RabbitMQ logs
docker compose logs rabbitmq

# Follow logs in real-time
docker compose logs -f
```

### 4. Update Connection Strings

The setup script automatically updates connection strings. If setting up manually, update:

**API** (`src/TaskManagement.API/appsettings.json`):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=TaskManagementDb;User Id=sa;Password=YourStrong@Passw0rd123;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

**Windows Service** (`src/TaskManagement.WindowsService/appsettings.json`):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=TaskManagementDb;User Id=sa;Password=YourStrong@Passw0rd123;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "RabbitMQ": {
    "HostName": "localhost"
  }
}
```

### 5. Run Database Migrations

```powershell
cd src\TaskManagement.API
dotnet ef database update --project ..\TaskManagement.Infrastructure
```

### 6. Start Application Services

Follow the regular setup instructions for starting the API, Frontend, and Windows Service.

## Docker Commands Reference

### Start Services
```powershell
docker compose up -d
```

### Stop Services
```powershell
docker compose down
```

### Stop and Remove Volumes (Clean Slate)
```powershell
docker compose down -v
```

### Restart Services
```powershell
docker compose restart
```

### View Logs
```powershell
# All services
docker compose logs -f

# Specific service
docker compose logs -f sqlserver
docker compose logs -f rabbitmq
```

### Check Service Status
```powershell
docker compose ps
```

### Access SQL Server

Using SQL Server Management Studio or Azure Data Studio:
- Server: `localhost,1433`
- Authentication: SQL Server Authentication
- Username: `sa`
- Password: `YourStrong@Passw0rd123`

Or using sqlcmd:
```powershell
docker exec -it taskmanagement-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd123
```

### Access RabbitMQ Management UI

Open in browser: http://localhost:15672
- Username: `guest`
- Password: `guest`

## Customization

### Change Passwords

1. Copy `docker-compose.override.yml.example` to `docker-compose.override.yml`
2. Edit the file to change passwords
3. Restart services: `docker compose down && docker compose up -d`

### Change Ports

If ports 1433 or 5672 are already in use, edit `docker-compose.override.yml`:

```yaml
services:
  sqlserver:
    ports:
      - "1434:1433"  # Use 1434 instead of 1433
  rabbitmq:
    ports:
      - "5673:5672"   # Use 5673 instead of 5672
      - "15673:15672" # Use 15673 instead of 15672
```

Then update connection strings accordingly.

## Troubleshooting

### Services Won't Start

1. **Check Docker is running**:
   ```powershell
   docker ps
   ```

2. **Check port conflicts**:
   ```powershell
   netstat -ano | findstr :1433
   netstat -ano | findstr :5672
   ```

3. **View error logs**:
   ```powershell
   docker compose logs
   ```

### SQL Server Connection Issues

1. **Wait for SQL Server to be ready** (can take 30-60 seconds on first start):
   ```powershell
   docker compose logs -f sqlserver
   ```
   Wait for: "SQL Server is now ready for client connections"

2. **Check health status**:
   ```powershell
   docker inspect --format='{{.State.Health.Status}}' taskmanagement-sqlserver
   ```

3. **Restart SQL Server**:
   ```powershell
   docker compose restart sqlserver
   ```

### RabbitMQ Connection Issues

1. **Check RabbitMQ is running**:
   ```powershell
   docker compose ps rabbitmq
   ```

2. **View RabbitMQ logs**:
   ```powershell
   docker compose logs rabbitmq
   ```

3. **Restart RabbitMQ**:
   ```powershell
   docker compose restart rabbitmq
   ```

### Data Persistence

Data is stored in Docker volumes:
- `sqlserver_data` - SQL Server database files
- `rabbitmq_data` - RabbitMQ data

To remove all data:
```powershell
docker compose down -v
```

To backup data:
```powershell
# SQL Server backup
docker exec taskmanagement-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd123 -Q "BACKUP DATABASE TaskManagementDb TO DISK='/var/opt/mssql/backup/TaskManagementDb.bak'"
```

## Performance Tips

1. **Allocate more resources to Docker Desktop**:
   - Docker Desktop → Settings → Resources
   - Increase CPU and Memory allocation

2. **Use named volumes** (already configured):
   - Data persists between container restarts
   - Better performance than bind mounts

3. **Monitor resource usage**:
   ```powershell
   docker stats
   ```

## Security Notes

⚠️ **Important**: The default passwords in `docker-compose.yml` are for development only!

For production:
1. Use strong passwords
2. Store passwords in environment variables or secrets
3. Use `docker-compose.override.yml` (not committed to git) for custom passwords
4. Restrict network access
5. Use Docker secrets for sensitive data

## Next Steps

After Docker services are running, continue with the regular setup:
1. Run database migrations
2. Seed the database (optional)
3. Start the API
4. Start the Frontend
5. Start the Windows Service

See the main [README.md](README.md) for complete instructions.
