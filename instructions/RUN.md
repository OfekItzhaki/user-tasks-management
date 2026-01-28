# How to Run

## Start Everything (Easiest)

```powershell
.\start-all.ps1
```

This starts:
- ✅ API (http://localhost:5063)
- ✅ Frontend (http://localhost:5173)
- ✅ Windows Service (console mode)

## Individual Services

### API Only

```powershell
cd src\TaskManagement.API
dotnet run
```

### Frontend Only

```powershell
cd src\TaskManagement.Web
npm run dev
```

### Windows Service

```powershell
cd src\TaskManagement.WindowsService
dotnet run
```

## Access Points

- **Frontend**: http://localhost:5173
- **API Swagger**: http://localhost:5063/swagger
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)

## Run Tests

```powershell
.\run-all-tests.ps1
```

Runs both backend (.NET) and frontend (Vitest) tests.

## Verify Services

```powershell
.\verify-services.ps1
```

Checks if SQL Server, RabbitMQ, and services are running.

## Troubleshooting

If something doesn't work, see [TROUBLESHOOTING.md](./TROUBLESHOOTING.md).
