# Quick Start - Local (Automated)
# Uses LocalDB instead of Docker
# This script checks prerequisites and runs everything automatically

$ErrorActionPreference = "Stop"

# Fix PATH for .NET SDK
$env:DOTNET_ROOT = "C:\Program Files\dotnet"
$env:PATH = "C:\Program Files\dotnet;$env:PATH"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Quick Start - Local (Automated)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Function to check if a command exists
function Test-Command {
    param([string]$Command)
    $null = Get-Command $Command -ErrorAction SilentlyContinue
    return $?
}

# Check prerequisites
Write-Host "Checking prerequisites..." -ForegroundColor Yellow
Write-Host ""

$prerequisitesOk = $true
$missingPrerequisites = @()

# Check .NET SDK
if (-not (Test-Command "dotnet")) {
    Write-Host "[X] .NET SDK not found" -ForegroundColor Red
    $prerequisitesOk = $false
    $missingPrerequisites += ".NET 8.0 SDK (https://dotnet.microsoft.com/download)"
} else {
    $version = dotnet --version
    Write-Host "[OK] .NET SDK: $version" -ForegroundColor Green
}

# Check Node.js
if (-not (Test-Command "node")) {
    Write-Host "[X] Node.js not found" -ForegroundColor Red
    $prerequisitesOk = $false
    $missingPrerequisites += "Node.js 20.19+ or 22.12+ (https://nodejs.org/)"
} else {
    $version = node --version
    Write-Host "[OK] Node.js: $version" -ForegroundColor Green
}

# Check SQL Server LocalDB
$localdbFound = $false
try {
    $localdbInfo = sqllocaldb info mssqllocaldb 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[OK] SQL Server LocalDB: Available" -ForegroundColor Green
        $localdbFound = $true
        # Start LocalDB if not running
        sqllocaldb start mssqllocaldb 2>&1 | Out-Null
    }
} catch {
    # Try to find LocalDB in common locations
    $localdbPaths = @(
        "C:\Program Files\Microsoft SQL Server\150\Tools\Binn\SqlLocalDB.exe",
        "C:\Program Files\Microsoft SQL Server\160\Tools\Binn\SqlLocalDB.exe"
    )
    foreach ($path in $localdbPaths) {
        if (Test-Path $path) {
            Write-Host "[OK] SQL Server LocalDB: Found at $path" -ForegroundColor Green
            $localdbFound = $true
            break
        }
    }
}

if (-not $localdbFound) {
    Write-Host "[!] SQL Server LocalDB: Not found" -ForegroundColor Yellow
    Write-Host "  LocalDB usually comes with Visual Studio" -ForegroundColor Gray
    Write-Host "  Or install SQL Server Express: https://www.microsoft.com/sql-server/sql-server-downloads" -ForegroundColor Gray
    Write-Host "  Continuing anyway (will try to use connection string)" -ForegroundColor Yellow
}

Write-Host ""

if (-not $prerequisitesOk) {
    Write-Host "Missing prerequisites:" -ForegroundColor Red
    foreach ($item in $missingPrerequisites) {
        Write-Host "  - $item" -ForegroundColor Red
    }
    Write-Host ""
    Write-Host "Please install the missing prerequisites and run this script again." -ForegroundColor Yellow
    exit 1
}

# Step 1: Database Setup
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Step 1: Setting up database..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if dotnet-ef is installed
Write-Host "Checking Entity Framework tools..." -ForegroundColor Yellow
if (-not (Test-Command "dotnet-ef")) {
    Write-Host "Installing dotnet-ef tool..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
    Write-Host "[OK] dotnet-ef installed" -ForegroundColor Green
} else {
    Write-Host "[OK] dotnet-ef found" -ForegroundColor Green
}
Write-Host ""

# Database setup paths
$apiPath = "src\TaskManagement.API"
$infrastructurePath = "src\TaskManagement.Infrastructure"
$webPath = "src\TaskManagement.Web"

if (-not (Test-Path $apiPath)) {
    Write-Host "[X] API project not found at $apiPath" -ForegroundColor Red
    exit 1
}

# Ensure LocalDB is started
Write-Host "Starting LocalDB..." -ForegroundColor Yellow
try {
    sqllocaldb start mssqllocaldb 2>&1 | Out-Null
    Write-Host "[OK] LocalDB started" -ForegroundColor Green
} catch {
    Write-Host "[!] Could not start LocalDB automatically. Continuing..." -ForegroundColor Yellow
}
Write-Host ""

# Restore NuGet packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
$originalLocation = Get-Location
try {
    # Try to restore from solution file first
    $solutionFile = Get-ChildItem -Path "." -Filter "*.sln" -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($solutionFile) {
        Write-Host "  Restoring solution: $($solutionFile.Name)" -ForegroundColor Gray
        dotnet restore $solutionFile.FullName 2>&1 | Out-Null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "[OK] NuGet packages restored" -ForegroundColor Green
        } else {
            Write-Host "[!] Solution restore failed, trying project restore..." -ForegroundColor Yellow
            Set-Location $apiPath
            dotnet restore 2>&1 | Out-Null
            Set-Location "..\.."
            if ($LASTEXITCODE -eq 0) {
                Write-Host "[OK] NuGet packages restored" -ForegroundColor Green
            } else {
                Write-Host "[X] Failed to restore NuGet packages" -ForegroundColor Red
                Set-Location $originalLocation
                exit 1
            }
        }
    } else {
        Set-Location $apiPath
        dotnet restore 2>&1 | Out-Null
        Set-Location "..\.."
        if ($LASTEXITCODE -eq 0) {
            Write-Host "[OK] NuGet packages restored" -ForegroundColor Green
        } else {
            Write-Host "[X] Failed to restore NuGet packages" -ForegroundColor Red
            Set-Location $originalLocation
            exit 1
        }
    }
} catch {
    Write-Host "[X] Error restoring packages: $_" -ForegroundColor Red
    Set-Location $originalLocation
    exit 1
}
Write-Host ""

# Run migrations
Write-Host "Running database migrations..." -ForegroundColor Yellow

# Stop any running TaskManagement processes that might lock DLL files
Write-Host "  Stopping any running TaskManagement processes..." -ForegroundColor Gray
$runningProcesses = Get-Process | Where-Object {
    $_.ProcessName -like "*TaskManagement*" -or
    ($_.ProcessName -eq "dotnet" -and $_.Path -like "*TaskManagement*")
}

if ($runningProcesses) {
    Write-Host "  Found $($runningProcesses.Count) running process(es), stopping them..." -ForegroundColor Yellow
    $runningProcesses | Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 3
    Write-Host "  [OK] Stopped running processes" -ForegroundColor Green
} else {
    Write-Host "  [OK] No running processes found" -ForegroundColor Green
}

Set-Location $apiPath
try {
    # Restore API project
    Write-Host "  Ensuring API project is restored..." -ForegroundColor Gray
    $apiProjectPath = Join-Path (Get-Location) "TaskManagement.API.csproj"
    $restoreOutput = dotnet restore $apiProjectPath 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[X] Failed to restore API project" -ForegroundColor Red
        Write-Host "Restore output: $restoreOutput" -ForegroundColor Red
        Set-Location $originalLocation
        exit 1
    }
    
    # Restore Infrastructure project
    Write-Host "  Ensuring Infrastructure project is restored..." -ForegroundColor Gray
    $infraProjectPath = Join-Path (Get-Location) "..\TaskManagement.Infrastructure\TaskManagement.Infrastructure.csproj"
    $infraProjectPath = Resolve-Path $infraProjectPath -ErrorAction SilentlyContinue
    
    if (-not $infraProjectPath) {
        Write-Host "[X] Infrastructure project not found" -ForegroundColor Red
        Set-Location $originalLocation
        exit 1
    }
    
    $restoreOutput = dotnet restore $infraProjectPath 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[X] Failed to restore Infrastructure project" -ForegroundColor Red
        Write-Host "Restore output: $restoreOutput" -ForegroundColor Red
        Set-Location $originalLocation
        exit 1
    }
    
    # Build both projects before migrations
    Write-Host "  Building API project..." -ForegroundColor Gray
    $apiBuildOutput = dotnet build $apiProjectPath --no-restore 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[X] Failed to build API project" -ForegroundColor Red
        Write-Host "Build output: $apiBuildOutput" -ForegroundColor Red
        Set-Location $originalLocation
        exit 1
    }
    
    Write-Host "  Building Infrastructure project..." -ForegroundColor Gray
    $infraBuildOutput = dotnet build $infraProjectPath --no-restore 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[X] Failed to build Infrastructure project" -ForegroundColor Red
        Write-Host "Build output: $infraBuildOutput" -ForegroundColor Red
        Set-Location $originalLocation
        exit 1
    }
    
    # Run migrations
    Write-Host "  Running migrations..." -ForegroundColor Gray
    $migrationOutput = dotnet ef database update --project $infraProjectPath --startup-project $apiProjectPath 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[OK] Database migrations applied" -ForegroundColor Green
    } else {
        Write-Host "[X] Database migration failed" -ForegroundColor Red
        Write-Host "Migration output:" -ForegroundColor Red
        $migrationLines = $migrationOutput -split "`n" | Select-Object -Last 20
        foreach ($line in $migrationLines) {
            Write-Host "  $line" -ForegroundColor Red
        }
        Write-Host ""
        Write-Host "Troubleshooting:" -ForegroundColor Yellow
        Write-Host "  1. Make sure LocalDB is running: sqllocaldb start mssqllocaldb" -ForegroundColor White
        Write-Host "  2. Check if there are build errors in the projects" -ForegroundColor White
        Set-Location $originalLocation
        exit 1
    }
} catch {
    Write-Host "[X] Error running migrations: $_" -ForegroundColor Red
    Write-Host "Make sure LocalDB is running: sqllocaldb start mssqllocaldb" -ForegroundColor Yellow
    Set-Location $originalLocation
    exit 1
}
Set-Location $originalLocation
Write-Host ""

# Frontend setup
Write-Host "Setting up frontend..." -ForegroundColor Yellow
if (-not (Test-Path $webPath)) {
    Write-Host "[X] Web project not found at $webPath" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path "$webPath\node_modules")) {
    Write-Host "Installing frontend dependencies..." -ForegroundColor Yellow
    Set-Location $webPath
    npm install
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[X] npm install failed" -ForegroundColor Red
        Set-Location "..\.."
        exit 1
    }
    Write-Host "[OK] Frontend dependencies installed" -ForegroundColor Green
    Set-Location "..\.."
} else {
    Write-Host "[OK] Frontend dependencies already installed" -ForegroundColor Green
}
Write-Host ""

# Build solution
Write-Host "Building solution..." -ForegroundColor Yellow
dotnet build
if ($LASTEXITCODE -ne 0) {
    Write-Host "[X] Build failed" -ForegroundColor Red
    exit 1
}
Write-Host "[OK] Build successful" -ForegroundColor Green
Write-Host ""

# Step 2: Start all services
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Step 2: Starting all services..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$startScript = Join-Path $PSScriptRoot "..\start-all.ps1"
if (Test-Path $startScript) {
    & $startScript
} else {
    Write-Host "[X] Start script not found: $startScript" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 3: Verify Installation
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Step 3: Verifying Installation..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Checking installed versions:" -ForegroundColor Yellow
$dotnetVersion = dotnet --version
$nodeVersion = node --version
$npmVersion = npm --version

Write-Host "  [OK] .NET SDK: $dotnetVersion" -ForegroundColor Green
Write-Host "  [OK] Node.js: $nodeVersion" -ForegroundColor Green
Write-Host "  [OK] npm: $npmVersion" -ForegroundColor Green

Write-Host ""
Write-Host "Verifying database setup..." -ForegroundColor Yellow
$apiPath = Join-Path $PSScriptRoot "..\..\src\TaskManagement.API"
Push-Location $apiPath
try {
    # Check if migrations are applied
    dotnet ef migrations list --project ..\TaskManagement.Infrastructure 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  [OK] Database migrations verified" -ForegroundColor Green
    } else {
        Write-Host "  [!] Database migrations may need to be applied" -ForegroundColor Yellow
    }
} catch {
    Write-Host "  [!] Could not verify database migrations" -ForegroundColor Yellow
}
Pop-Location

Write-Host ""
Write-Host "Verifying frontend dependencies..." -ForegroundColor Yellow
$webPath = Join-Path $PSScriptRoot "..\..\src\TaskManagement.Web"
if (Test-Path (Join-Path $webPath "node_modules")) {
    Write-Host "  [OK] Frontend dependencies installed" -ForegroundColor Green
} else {
    Write-Host "  [!] Frontend dependencies not found" -ForegroundColor Yellow
}

Write-Host ""

# Step 4: Offer to seed database
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Step 4: Database Seeding (Optional)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Would you like to seed the database with sample data?" -ForegroundColor Yellow
Write-Host "This will create sample users, tags, and tasks for testing." -ForegroundColor Gray
Write-Host ""
$seedChoice = Read-Host "Seed database? (y/n)"

if ($seedChoice -eq "y" -or $seedChoice -eq "Y") {
    Write-Host ""
    Write-Host "Waiting for API to be ready..." -ForegroundColor Yellow
    $apiReady = $false
    $maxWait = 30
    $waited = 0
    
    while (-not $apiReady -and $waited -lt $maxWait) {
        try {
            $response = Invoke-WebRequest -Uri "http://localhost:5063/api/seed" -Method POST -TimeoutSec 2 -UseBasicParsing -ErrorAction SilentlyContinue
            if ($response.StatusCode -eq 200) {
                $apiReady = $true
                Write-Host "  [OK] Database seeded successfully!" -ForegroundColor Green
            }
        } catch {
            Start-Sleep -Seconds 2
            $waited += 2
            Write-Host "." -NoNewline -ForegroundColor Gray
        }
    }
    
    if (-not $apiReady) {
        Write-Host ""
        Write-Host "  [!] Could not seed database automatically" -ForegroundColor Yellow
        Write-Host "  You can seed it manually after the API starts:" -ForegroundColor Gray
        Write-Host "    POST http://localhost:5063/api/seed" -ForegroundColor Cyan
        Write-Host "    Or use Swagger UI: http://localhost:5063/swagger" -ForegroundColor Cyan
    }
} else {
    Write-Host ""
    Write-Host "Skipping database seeding." -ForegroundColor Gray
    Write-Host "You can seed it later:" -ForegroundColor Gray
    Write-Host "  POST http://localhost:5063/api/seed" -ForegroundColor Cyan
    Write-Host "  Or use Swagger UI: http://localhost:5063/swagger" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "All done! Services are starting..." -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Access points:" -ForegroundColor Cyan
Write-Host "  - Frontend: http://localhost:5173" -ForegroundColor White
Write-Host "  - API Swagger: http://localhost:5063/swagger" -ForegroundColor White
Write-Host ""
Write-Host "Installation verified:" -ForegroundColor Cyan
Write-Host "  - .NET SDK: $dotnetVersion" -ForegroundColor White
Write-Host "  - Node.js: $nodeVersion" -ForegroundColor White
Write-Host "  - npm: $npmVersion" -ForegroundColor White
Write-Host ""
Write-Host "Note: RabbitMQ is optional. Windows Service will work without it." -ForegroundColor Yellow
Write-Host ""
