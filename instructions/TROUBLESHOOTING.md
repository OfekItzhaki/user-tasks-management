# Troubleshooting

## Common Issues

### "dotnet command not found"

**Fix**: Add .NET SDK to PATH
```powershell
$env:PATH = "C:\Program Files\dotnet;" + $env:PATH
```

For permanent fix, close and reopen PowerShell.

### "Script execution policy error"

**Fix**: Allow scripts to run
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### "Port already in use"

**Fix**: Stop the service using the port, or change ports in:
- API: `src/TaskManagement.API/Properties/launchSettings.json`
- Frontend: `src/TaskManagement.Web/vite.config.ts`

### "Database connection failed"

**Fix**: Start SQL Server LocalDB
```powershell
sqllocaldb start mssqllocaldb
```

Or use Docker:
```powershell
docker compose up -d sqlserver
```

### "RabbitMQ not running"

**Fix**: Start RabbitMQ
```powershell
docker compose up -d rabbitmq
```

Or check if running:
```powershell
docker ps --filter "name=rabbit"
```

### "npm install fails"

**Fix**: Clear cache and reinstall
```powershell
cd src\TaskManagement.Web
Remove-Item -Recurse -Force node_modules
Remove-Item package-lock.json
npm install
```

### "Frontend tests fail"

**Fix**: Install dependencies first
```powershell
cd src\TaskManagement.Web
npm install
npm run test
```

## Still Having Issues?

1. Check service status: `.\verify-services.ps1`
2. Review logs in console output
3. Check [instructions/](instructions/) folder for detailed guides
