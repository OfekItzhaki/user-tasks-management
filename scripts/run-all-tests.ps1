# Run All Tests Script
# Runs both backend (.NET) and frontend (Vitest) tests

$ErrorActionPreference = "Continue"

# Fix PATH for .NET SDK
$env:DOTNET_ROOT = "C:\Program Files\dotnet"
$env:PATH = "C:\Program Files\dotnet;$env:PATH"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Running All Tests" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$backendTestsPassed = $false
$frontendTestsPassed = $false

# Run Backend Tests
Write-Host "1. Running Backend Tests (.NET)..." -ForegroundColor Yellow
Write-Host ""

try {
    Push-Location "src\TaskManagement.Tests"
    dotnet test --verbosity normal
    if ($LASTEXITCODE -eq 0) {
        $backendTestsPassed = $true
        Write-Host ""
        Write-Host "✓ Backend tests passed!" -ForegroundColor Green
    } else {
        Write-Host ""
        Write-Host "✗ Backend tests failed!" -ForegroundColor Red
    }
} catch {
    Write-Host "✗ Error running backend tests: $_" -ForegroundColor Red
} finally {
    Pop-Location
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Run Frontend Tests
Write-Host "2. Running Frontend Tests (Vitest)..." -ForegroundColor Yellow
Write-Host ""

try {
    Push-Location "src\TaskManagement.Web"
    
    # Check if node_modules exists
    if (-not (Test-Path "node_modules")) {
        Write-Host "Installing frontend dependencies..." -ForegroundColor Yellow
        npm install
    }
    
    npm run test -- --run
    if ($LASTEXITCODE -eq 0) {
        $frontendTestsPassed = $true
        Write-Host ""
        Write-Host "✓ Frontend tests passed!" -ForegroundColor Green
    } else {
        Write-Host ""
        Write-Host "✗ Frontend tests failed!" -ForegroundColor Red
    }
} catch {
    Write-Host "✗ Error running frontend tests: $_" -ForegroundColor Red
} finally {
    Pop-Location
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Test Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if ($backendTestsPassed) {
    Write-Host "✓ Backend Tests: PASSED" -ForegroundColor Green
} else {
    Write-Host "✗ Backend Tests: FAILED" -ForegroundColor Red
}

if ($frontendTestsPassed) {
    Write-Host "✓ Frontend Tests: PASSED" -ForegroundColor Green
} else {
    Write-Host "✗ Frontend Tests: FAILED" -ForegroundColor Red
}

Write-Host ""

if ($backendTestsPassed -and $frontendTestsPassed) {
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "All tests passed! ✓" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    exit 0
} else {
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "Some tests failed! ✗" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    exit 1
}
