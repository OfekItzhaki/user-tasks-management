# Configuration

## Connection strings and secrets

**Do not commit real passwords.** Connection strings in `appsettings*.json` use placeholders. Set the real value using one of the following.

### Option 1: Environment variable (recommended for production and Docker)

- **API / Windows Service:** set `ConnectionStrings__DefaultConnection` to your full connection string.

Example (PowerShell):

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=localhost,1433;Database=TaskManagementDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

Example (Docker / docker-compose): use `environment` or an env file and set `ConnectionStrings__DefaultConnection`.

### Option 2: User Secrets (recommended for local development)

From the project directory (API or Windows Service):

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=TaskManagementDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

User Secrets override values in `appsettings.json` and `appsettings.Development.json`.

### Option 3: Local override file (not committed)

Use `appsettings.Development.json` or a custom file that is in `.gitignore` and set `ConnectionStrings:DefaultConnection` there. Do not commit files that contain real passwords.

---

## Seed and Clear Database (API)

The `/api/seed` endpoints (seed and clear database) are **only available in Development**. In Production they return 404. This prevents accidental data loss.
