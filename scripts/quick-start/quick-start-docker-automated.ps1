# Quick Start - Docker (Automated)
# Easiest option: Uses Docker for SQL Server and RabbitMQ
# This script checks prerequisites and runs everything automatically

$ErrorActionPreference = "Stop"

# Fix PATH for .NET SDK
$env:DOTNET_ROOT = "C:\Program Files\dotnet"
$env:PATH = "C:\Program Files\dotnet;$env:PATH"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Quick Start - Docker (Automated)" -ForegroundColor Cyan
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

# Check Docker
if (-not (Test-Command "docker")) {
    Write-Host "[X] Docker not found" -ForegroundColor Red
    $prerequisitesOk = $false
    $missingPrerequisites += "Docker Desktop (https://www.docker.com/products/docker-desktop)"
} else {
    $version = docker --version
    Write-Host "[OK] Docker: $version" -ForegroundColor Green
}

# Check Docker Compose
$composeCommand = $null
if (Test-Command "docker compose") {
    $composeCommand = "docker compose"
    Write-Host "[OK] Docker Compose: Available" -ForegroundColor Green
} elseif (Test-Command "docker-compose") {
    $composeCommand = "docker-compose"
    Write-Host "[OK] Docker Compose: Available" -ForegroundColor Green
} else {
    Write-Host "[X] Docker Compose not found" -ForegroundColor Red
    $prerequisitesOk = $false
    $missingPrerequisites += "Docker Compose (usually included with Docker Desktop)"
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

# Function to check if Docker Desktop is running
function Test-DockerRunning {
    try {
        # Use a timeout to prevent hanging when Docker Desktop is starting
        # docker ps should respond quickly when ready, but may hang when starting
        $job = Start-Job -ScriptBlock {
            docker ps 2>&1 | Out-Null
            return $LASTEXITCODE
        }
        
        # Wait max 2 seconds for response (Docker responds instantly when ready)
        $completed = Wait-Job -Job $job -Timeout 2
        
        if ($completed) {
            $result = Receive-Job -Job $job
            Remove-Job -Job $job -Force -ErrorAction SilentlyContinue
            return $result -eq 0
        } else {
            # Timeout - Docker daemon not ready yet (or hanging)
            Stop-Job -Job $job -ErrorAction SilentlyContinue
            Remove-Job -Job $job -Force -ErrorAction SilentlyContinue
            return $false
        }
    } catch {
        return $false
    }
}

# Function to start Docker Desktop
function Start-DockerDesktop {
    Write-Host "Docker Desktop is not running. Attempting to start it..." -ForegroundColor Yellow
    Write-Host ""
    
    # Common Docker Desktop installation paths
    $dockerPaths = @(
        "${env:ProgramFiles}\Docker\Docker\Docker Desktop.exe",
        "${env:ProgramFiles(x86)}\Docker\Docker\Docker Desktop.exe",
        "$env:LOCALAPPDATA\Docker\Docker Desktop.exe"
    )
    
    $dockerExe = $null
    foreach ($path in $dockerPaths) {
        if (Test-Path $path) {
            $dockerExe = $path
            break
        }
    }
    
    if (-not $dockerExe) {
        Write-Host "[X] Docker Desktop executable not found in common locations" -ForegroundColor Red
        Write-Host ""
        Write-Host "Please start Docker Desktop manually:" -ForegroundColor Yellow
        Write-Host "  1. Open Start menu and search for 'Docker Desktop'" -ForegroundColor White
        Write-Host "  2. Click to start Docker Desktop" -ForegroundColor White
        Write-Host "  3. Wait for it to fully start (whale icon in system tray)" -ForegroundColor White
        Write-Host "  4. Run this script again" -ForegroundColor White
        exit 1
    }
    
    Write-Host "Starting Docker Desktop..." -ForegroundColor Cyan
    try {
        Start-Process -FilePath $dockerExe -ErrorAction Stop
        Write-Host "[OK] Docker Desktop launch command executed" -ForegroundColor Green
    } catch {
        Write-Host "[X] Failed to start Docker Desktop: $_" -ForegroundColor Red
        Write-Host ""
        Write-Host "Please start Docker Desktop manually and run this script again." -ForegroundColor Yellow
        exit 1
    }
    
    # Wait for Docker Desktop to start
    Write-Host ""
    Write-Host "Waiting for Docker Desktop to start..." -ForegroundColor Yellow
    Write-Host "This may take 30-60 seconds. Please wait..." -ForegroundColor Gray
    Write-Host ""
    
    $maxWait = 120 # Maximum wait time in seconds
    $waited = 0
    $checkInterval = 3 # Check every 3 seconds
    $dockerReady = $false
    
    while (-not $dockerReady -and $waited -lt $maxWait) {
        Start-Sleep -Seconds $checkInterval
        $waited += $checkInterval
        
        if (Test-DockerRunning) {
            $dockerReady = $true
            Write-Host "[OK] Docker Desktop is now running!" -ForegroundColor Green
            break
        }
        
        # Show progress every 10 seconds
        if ($waited % 10 -eq 0) {
            Write-Host "  Still waiting... ($waited seconds)" -ForegroundColor Gray
        }
    }
    
    if (-not $dockerReady) {
        Write-Host ""
        Write-Host "[X] Docker Desktop did not start within $maxWait seconds" -ForegroundColor Red
        Write-Host ""
        Write-Host "Possible issues:" -ForegroundColor Yellow
        Write-Host "  - Virtualization may not be enabled in BIOS" -ForegroundColor White
        Write-Host "  - WSL 2 may not be installed or configured" -ForegroundColor White
        Write-Host "  - Docker Desktop may need manual configuration" -ForegroundColor White
        Write-Host ""
        Write-Host "Please:" -ForegroundColor Yellow
        Write-Host "  1. Check Docker Desktop window for error messages" -ForegroundColor White
        Write-Host "  2. Ensure virtualization is enabled in BIOS" -ForegroundColor White
        Write-Host "  3. Install WSL 2 if prompted by Docker Desktop" -ForegroundColor White
        Write-Host "  4. Start Docker Desktop manually and wait for it to fully start" -ForegroundColor White
        Write-Host "  5. Run this script again" -ForegroundColor White
        exit 1
    }
    
    # Give Docker a few more seconds to fully initialize
    Write-Host "  Giving Docker a moment to fully initialize..." -ForegroundColor Gray
    Start-Sleep -Seconds 5
}

# Function to check for stuck Docker Desktop state and fix it
function Test-AndFixDockerStuckState {
    $dockerProcess = Get-Process -Name "Docker Desktop" -ErrorAction SilentlyContinue
    $daemonAccessible = Test-DockerRunning
    
    # Check for stuck state: process exists but daemon not accessible, or multiple processes
    if ($dockerProcess -and -not $daemonAccessible) {
        $isStuck = $false
        $reason = ""
        
        if ($dockerProcess.Count -gt 1) {
            $isStuck = $true
            $reason = "Multiple Docker Desktop processes detected (stuck state)"
        } elseif (-not $daemonAccessible) {
            $isStuck = $true
            $reason = "Docker Desktop process running but daemon not accessible"
        }
        
        if ($isStuck) {
            Write-Host "[!] Detected stuck Docker Desktop state: $reason" -ForegroundColor Yellow
            Write-Host "  Automatically fixing by restarting Docker Desktop..." -ForegroundColor Cyan
            Write-Host ""
            
            # Stop all Docker Desktop processes
            Write-Host "  Stopping all Docker Desktop processes..." -ForegroundColor Gray
            Get-Process -Name "Docker Desktop" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
            Start-Sleep -Seconds 3
            
            # Restart Docker Desktop
            $dockerPaths = @(
                "${env:ProgramFiles}\Docker\Docker\Docker Desktop.exe",
                "${env:ProgramFiles(x86)}\Docker\Docker\Docker Desktop.exe",
                "$env:LOCALAPPDATA\Docker\Docker Desktop.exe"
            )
            
            $dockerExe = $null
            foreach ($path in $dockerPaths) {
                if (Test-Path $path) {
                    $dockerExe = $path
                    break
                }
            }
            
            if ($dockerExe) {
                Write-Host "  Starting Docker Desktop..." -ForegroundColor Gray
                Start-Process -FilePath $dockerExe -ErrorAction SilentlyContinue
                Write-Host "  [OK] Docker Desktop restart initiated" -ForegroundColor Green
                Write-Host ""
                return $true # Indicates we restarted Docker
            } else {
                Write-Host "  [X] Could not find Docker Desktop executable" -ForegroundColor Red
                return $false
            }
        }
    }
    
    return $false # No stuck state detected
}

# Check if Docker is running
Write-Host "Checking if Docker is running..." -ForegroundColor Yellow

# First, check for stuck state and fix it automatically
$wasRestarted = Test-AndFixDockerStuckState
if ($wasRestarted) {
    # Give Docker a moment to start after restart
    Start-Sleep -Seconds 5
    Write-Host "Waiting for Docker Desktop to initialize after restart..." -ForegroundColor Yellow
}

if (-not (Test-DockerRunning)) {
    Write-Host "[!] Docker Desktop is not running" -ForegroundColor Yellow
    Write-Host ""
    
    # Check if Docker Desktop process exists (might be starting)
    $dockerProcess = Get-Process -Name "Docker Desktop" -ErrorAction SilentlyContinue
    if ($dockerProcess) {
        Write-Host "Docker Desktop process detected but not ready yet..." -ForegroundColor Yellow
        Write-Host "Waiting for Docker Desktop to be ready (up to 90 seconds)..." -ForegroundColor Yellow
        Write-Host "  Docker Desktop can take time to fully initialize, especially after updates" -ForegroundColor Gray
        Write-Host ""
        
        $maxWait = 120 # 2 minutes max wait (Docker Desktop can be slow to start)
        $waited = 0
        $checkInterval = 5 # Check every 5 seconds (reduced frequency to avoid too many checks)
        
        Write-Host "  Note: Docker Desktop initialization can take 30-90 seconds" -ForegroundColor Gray
        Write-Host "  This is normal, especially on first start or after updates" -ForegroundColor Gray
        Write-Host ""
        
        while (-not (Test-DockerRunning) -and $waited -lt $maxWait) {
            Start-Sleep -Seconds $checkInterval
            $waited += $checkInterval
            
            # Show progress every 15 seconds
            if ($waited % 15 -eq 0) {
                Write-Host "  Still waiting... ($waited / $maxWait seconds)" -ForegroundColor Gray
                # Check if Docker Desktop process is still running
                $stillRunning = Get-Process -Name "Docker Desktop" -ErrorAction SilentlyContinue
                if (-not $stillRunning) {
                    Write-Host "  [!] Docker Desktop process disappeared - it may have crashed" -ForegroundColor Yellow
                    Write-Host "  Attempting to restart..." -ForegroundColor Yellow
                    Start-DockerDesktop
                    $waited = 0 # Reset wait counter after restart
                }
            }
        }
        
        if (-not (Test-DockerRunning)) {
            Write-Host ""
            Write-Host "[X] Docker Desktop is running but not responding after $maxWait seconds" -ForegroundColor Red
            Write-Host ""
            Write-Host "Troubleshooting steps:" -ForegroundColor Yellow
            Write-Host "  1. Check Docker Desktop window for error messages" -ForegroundColor White
            Write-Host "  2. Try restarting Docker Desktop manually" -ForegroundColor White
            Write-Host "  3. Check if WSL 2 is installed and configured" -ForegroundColor White
            Write-Host "  4. Ensure virtualization is enabled in BIOS" -ForegroundColor White
            Write-Host "  5. Try: docker version (in a separate terminal to test)" -ForegroundColor White
            Write-Host ""
            Write-Host "You can continue without Docker (using LocalDB instead) by running:" -ForegroundColor Cyan
            Write-Host "  .\scripts\quick-start\quick-start-local-automated.ps1" -ForegroundColor White
            exit 1
        }
        Write-Host "[OK] Docker Desktop is now ready!" -ForegroundColor Green
    } else {
        # Docker Desktop is not running, try to start it
        Start-DockerDesktop
    }
} else {
    Write-Host "[OK] Docker is running" -ForegroundColor Green
}

Write-Host ""

# Step 1: Start Docker services
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Step 1: Starting Docker services..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$projectRoot = Join-Path $PSScriptRoot "..\.."
$dockerComposePath = Join-Path $projectRoot "docker\docker-compose.yml"

Write-Host "Starting Docker services (this may take a few minutes on first run)..." -ForegroundColor Yellow

# Remove any existing containers to avoid name conflicts
Write-Host "Cleaning up any existing containers..." -ForegroundColor Gray

# Remove containers directly by name first (most reliable method)
$containerNames = @("taskmanagement-sqlserver", "taskmanagement-rabbitmq", "taskmanagement-api", "taskmanagement-frontend", "taskmanagement-service")
foreach ($containerName in $containerNames) {
    # Check if container exists first
    $exists = docker ps -a --filter "name=$containerName" --format "{{.Names}}" 2>$null
    if ($exists -and $exists.ToString().Trim() -eq $containerName) {
        Write-Host "  Removing existing container: $containerName" -ForegroundColor Yellow
        $removeOutput = docker rm -f $containerName 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "    [OK] Removed: $containerName" -ForegroundColor Green
        } else {
            Write-Host "    [X] Failed to remove: $containerName - $removeOutput" -ForegroundColor Red
        }
    }
}

# Also try docker compose down to remove containers created by compose
Write-Host "  Running docker compose down..." -ForegroundColor Gray
try {
    Push-Location $projectRoot
    if ($composeCommand -eq "docker compose") {
        $composeOutput = docker compose -f $dockerComposePath --project-directory $projectRoot down 2>&1
    } else {
        $composeOutput = docker-compose -f $dockerComposePath --project-directory $projectRoot down 2>&1
    }
    Pop-Location
} catch {
    # Ignore errors - containers might not exist or already removed
}

# Give Docker a moment to fully remove containers
Start-Sleep -Seconds 2

# Verify containers are removed before proceeding
$remainingContainers = docker ps -a --filter "name=taskmanagement" --format "{{.Names}}" 2>$null
if ($remainingContainers) {
    Write-Host "  [X] Warning: Some containers still exist, attempting force removal..." -ForegroundColor Yellow
    foreach ($container in $remainingContainers) {
        docker rm -f $container 2>&1 | Out-Null
    }
    Start-Sleep -Seconds 1
}

Write-Host "Pulling images and starting containers..." -ForegroundColor Gray
Write-Host ""

# Docker Compose uses the compose file's directory as base for relative paths
# We need to specify --project-directory to use the project root
Push-Location $projectRoot
try {
    # Run docker compose - let output show naturally
    # Use --project-directory to ensure build contexts resolve correctly
    # Use --force-recreate to handle any remaining container conflicts
    if ($composeCommand -eq "docker compose") {
        docker compose -f $dockerComposePath --project-directory $projectRoot up -d --force-recreate sqlserver rabbitmq
    } else {
        docker-compose -f $dockerComposePath --project-directory $projectRoot up -d --force-recreate sqlserver rabbitmq
    }
    $dockerExitCode = $LASTEXITCODE
} catch {
    # Only catch actual exceptions, not output
    $dockerExitCode = 1
    Write-Host ""
    Write-Host "[X] Exception occurred: $_" -ForegroundColor Red
} finally {
    Pop-Location
}

if ($dockerExitCode -ne 0) {
        Write-Host "[X] Failed to start Docker services" -ForegroundColor Red
        Write-Host ""
        
        # Check if Docker is still running
        if (-not (Test-DockerRunning)) {
            Write-Host "Docker Desktop appears to have stopped or crashed." -ForegroundColor Yellow
            Write-Host "Attempting to restart Docker Desktop..." -ForegroundColor Yellow
            Write-Host ""
            Start-DockerDesktop
            Write-Host ""
            Write-Host "Retrying Docker services startup..." -ForegroundColor Yellow
            
            # Retry starting services
            Write-Host "Pulling images and starting containers..." -ForegroundColor Gray
            Push-Location $projectRoot
            try {
                if ($composeCommand -eq "docker compose") {
                    docker compose -f $dockerComposePath --project-directory $projectRoot up -d --force-recreate sqlserver rabbitmq
                } else {
                    docker-compose -f $dockerComposePath --project-directory $projectRoot up -d --force-recreate sqlserver rabbitmq
                }
                $retryExitCode = $LASTEXITCODE
            } catch {
                $retryExitCode = 1
            } finally {
                Pop-Location
            }
            
            if ($retryExitCode -ne 0) {
                Write-Host "[X] Still failed to start Docker services after restart" -ForegroundColor Red
                Write-Host ""
                Write-Host "Common issues:" -ForegroundColor Yellow
                Write-Host "  - Port conflicts (check if ports 1433, 5672, 15672 are in use)" -ForegroundColor White
                Write-Host "  - Virtualization not enabled in BIOS" -ForegroundColor White
                Write-Host "  - WSL 2 not installed or configured" -ForegroundColor White
                Write-Host ""
                Write-Host "Please check Docker Desktop for error messages and try again." -ForegroundColor Yellow
                exit 1
            }
        } else {
            Write-Host "Common issues:" -ForegroundColor Yellow
            Write-Host "  - Port conflicts (check if ports 1433, 5672, 15672 are in use)" -ForegroundColor White
            Write-Host "  - Docker Desktop may need a restart" -ForegroundColor White
            Write-Host ""
            Write-Host "Try:" -ForegroundColor Yellow
            Write-Host "  1. Check Docker Desktop for error messages" -ForegroundColor White
            Write-Host "  2. Restart Docker Desktop" -ForegroundColor White
            Write-Host "  3. Run this script again" -ForegroundColor White
            exit 1
        }
    }
    Write-Host "[OK] Docker services started" -ForegroundColor Green
    Write-Host "  Waiting for SQL Server to be healthy (this may take up to 2 minutes)..." -ForegroundColor Yellow
    
    # Wait for SQL Server to be healthy
    $maxWait = 120
    $waited = 0
    $checkInterval = 5
    $sqlHealthy = $false
    
    while (-not $sqlHealthy -and $waited -lt $maxWait) {
        Start-Sleep -Seconds $checkInterval
        $waited += $checkInterval
        
        # Check SQL Server container health
        $healthStatus = docker inspect --format='{{.State.Health.Status}}' taskmanagement-sqlserver 2>$null
        if ($healthStatus -eq "healthy") {
            $sqlHealthy = $true
            Write-Host "  [OK] SQL Server is healthy" -ForegroundColor Green
        } elseif ($waited % 15 -eq 0) {
            Write-Host "  Still waiting... ($waited seconds)" -ForegroundColor Gray
        }
    }
    
    if (-not $sqlHealthy) {
        Write-Host "  [!] Warning: SQL Server healthcheck timeout, but continuing..." -ForegroundColor Yellow
        Write-Host "  Check SQL Server logs if you encounter database connection issues" -ForegroundColor Yellow
    }

Write-Host ""

# Step 2: Database Setup
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Step 2: Setting up database..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if dotnet-ef is installed
Write-Host "Checking Entity Framework tools..." -ForegroundColor Yellow
function Test-Command {
    param($command)
    $null = Get-Command $command -ErrorAction SilentlyContinue
    return $?
}

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

# Wait a bit more for SQL Server to be fully ready
Write-Host "Waiting for SQL Server to be fully ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Update connection string to use Docker SQL Server (both appsettings.json and appsettings.Development.json)
$dockerConnectionString = "Server=localhost,1433;Database=TaskManagementDb;User Id=sa;Password=YourStrong@Passw0rd123;TrustServerCertificate=True;MultipleActiveResultSets=true"

$apiAppsettingsPaths = @(
    "$apiPath\appsettings.json",
    "$apiPath\appsettings.Development.json"
)

foreach ($appsettingsPath in $apiAppsettingsPaths) {
    if (Test-Path $appsettingsPath) {
        $appsettingsContent = Get-Content $appsettingsPath -Raw
        # Check if connection string needs updating
        if ($appsettingsContent -notmatch "Server=localhost,1433") {
            Write-Host "Updating API connection string for Docker SQL Server: $(Split-Path $appsettingsPath -Leaf)..." -ForegroundColor Yellow
            $appsettingsContent = $appsettingsContent -replace '(?s)"ConnectionStrings":\s*\{[^}]*"DefaultConnection":\s*"[^"]*"', "`"ConnectionStrings`": {`n    `"DefaultConnection`": `"$dockerConnectionString`""
            Set-Content -Path $appsettingsPath -Value $appsettingsContent -NoNewline
            Write-Host "[OK] API connection string updated: $(Split-Path $appsettingsPath -Leaf)" -ForegroundColor Green
        }
    }
}

# Update Windows Service connection string (both appsettings.json and appsettings.Development.json)
$serviceAppsettingsPaths = @(
    "src\TaskManagement.WindowsService\appsettings.json",
    "src\TaskManagement.WindowsService\appsettings.Development.json"
)

foreach ($serviceAppsettingsPath in $serviceAppsettingsPaths) {
    if (Test-Path $serviceAppsettingsPath) {
        try {
            # Read the JSON file
            $jsonContent = Get-Content $serviceAppsettingsPath -Raw | ConvertFrom-Json
            
            # Update connection string
            if (-not $jsonContent.ConnectionStrings) {
                $jsonContent | Add-Member -MemberType NoteProperty -Name "ConnectionStrings" -Value @{} -Force
            }
            $jsonContent.ConnectionStrings.DefaultConnection = $dockerConnectionString
            
            # Update RabbitMQ HostName
            if (-not $jsonContent.RabbitMQ) {
                $jsonContent | Add-Member -MemberType NoteProperty -Name "RabbitMQ" -Value @{} -Force
            }
            $jsonContent.RabbitMQ.HostName = "localhost"
            
            # Write back to file with proper formatting
            $jsonContent | ConvertTo-Json -Depth 10 | Set-Content -Path $serviceAppsettingsPath
            Write-Host "[OK] Windows Service configuration updated: $(Split-Path $serviceAppsettingsPath -Leaf)" -ForegroundColor Green
        } catch {
            Write-Host "[!] Warning: Could not update $serviceAppsettingsPath : $_" -ForegroundColor Yellow
            # Fallback to regex replacement
            $serviceAppsettingsContent = Get-Content $serviceAppsettingsPath -Raw
            $serviceAppsettingsContent = $serviceAppsettingsContent -replace '(?s)"ConnectionStrings"\s*:\s*\{[^}]*"DefaultConnection"\s*:\s*"[^"]*"', "`"ConnectionStrings`": {`n    `"DefaultConnection`": `"$dockerConnectionString`""
            $serviceAppsettingsContent = $serviceAppsettingsContent -replace '(?s)"RabbitMQ"\s*:\s*\{[^}]*"HostName"\s*:\s*"[^"]*"', "`"RabbitMQ`": {`n    `"HostName`": `"localhost`""
            Set-Content -Path $serviceAppsettingsPath -Value $serviceAppsettingsContent -NoNewline
            Write-Host "[OK] Windows Service configuration updated (fallback method): $(Split-Path $serviceAppsettingsPath -Leaf)" -ForegroundColor Green
        }
    }
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
        Write-Host "  1. Make sure SQL Server container is running: docker ps" -ForegroundColor White
        Write-Host "  2. Check if there are build errors in the projects" -ForegroundColor White
        Set-Location $originalLocation
        exit 1
    }
} catch {
    Write-Host "[X] Error running migrations: $_" -ForegroundColor Red
    Write-Host "Make sure SQL Server container is running: docker ps" -ForegroundColor Yellow
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

# Step 3: Start all services
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Step 3: Starting all services..." -ForegroundColor Cyan
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

# Step 4: Verify Installation
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Step 4: Verifying Installation..." -ForegroundColor Cyan
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

# Step 5: Offer to seed database
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Step 5: Database Seeding (Optional)" -ForegroundColor Cyan
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
Write-Host "  - RabbitMQ Management: http://localhost:15672
    Username: guest
    Password: guest" -ForegroundColor White
Write-Host ""
Write-Host "Installation verified:" -ForegroundColor Cyan
Write-Host "  - .NET SDK: $dotnetVersion" -ForegroundColor White
Write-Host "  - Node.js: $nodeVersion" -ForegroundColor White
Write-Host "  - npm: $npmVersion" -ForegroundColor White
Write-Host ""
