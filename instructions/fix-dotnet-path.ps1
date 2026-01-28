# Quick Fix for .NET SDK PATH Issue
# This script fixes the PATH for the current PowerShell session

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Fixing .NET SDK PATH" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if .NET SDK exists
$dotnetPath = "C:\Program Files\dotnet"
if (-not (Test-Path $dotnetPath)) {
    Write-Host ".NET SDK not found at: $dotnetPath" -ForegroundColor Red
    Write-Host "Please install .NET SDK from: https://aka.ms/dotnet/download" -ForegroundColor Yellow
    exit 1
}

Write-Host ".NET SDK found at: $dotnetPath" -ForegroundColor Green
Write-Host ""

# Add to User PATH permanently
$userPath = [System.Environment]::GetEnvironmentVariable("PATH", "User")
if ($userPath -notlike "*$dotnetPath*") {
    Write-Host "Adding to User PATH permanently..." -ForegroundColor Yellow
    $newPath = "$dotnetPath;" + $userPath
    [System.Environment]::SetEnvironmentVariable("PATH", $newPath, "User")
    Write-Host "Added to User PATH" -ForegroundColor Green
} else {
    Write-Host "Already in User PATH" -ForegroundColor Green
}

# Also check x86 version
$x86Path = "C:\Program Files (x86)\dotnet"
if (Test-Path $x86Path) {
    $userPath = [System.Environment]::GetEnvironmentVariable("PATH", "User")
    if ($userPath -notlike "*$x86Path*") {
        $newPath = "$x86Path;" + $userPath
        [System.Environment]::SetEnvironmentVariable("PATH", $newPath, "User")
        Write-Host "Added x86 .NET SDK to PATH" -ForegroundColor Green
    }
}

# Fix current session PATH
$env:DOTNET_ROOT = $dotnetPath
$env:PATH = [System.Environment]::GetEnvironmentVariable("PATH", "User") + ";" + [System.Environment]::GetEnvironmentVariable("PATH", "Machine")

Write-Host ""
Write-Host "Testing .NET SDK..." -ForegroundColor Yellow

try {
    $version = dotnet --version
    Write-Host ".NET SDK is working! Version: $version" -ForegroundColor Green
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "PATH Fixed Successfully!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Note: This session is now fixed." -ForegroundColor Yellow
    Write-Host "For new PowerShell windows, the PATH is already configured." -ForegroundColor Yellow
    Write-Host ""
} catch {
    Write-Host "Still not working. Please close and reopen PowerShell." -ForegroundColor Red
    Write-Host ""
    Write-Host "If the issue persists:" -ForegroundColor Yellow
    Write-Host "1. Close ALL PowerShell windows" -ForegroundColor Yellow
    Write-Host "2. Open a NEW PowerShell window" -ForegroundColor Yellow
    Write-Host "3. Run: dotnet --version" -ForegroundColor Yellow
    Write-Host ""
    exit 1
}
