# Permanent PATH Fix Script
# This script fixes the .NET SDK PATH issue for all PowerShell sessions

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Permanent PATH Fix for .NET SDK" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check execution policy
Write-Host "Checking execution policy..." -ForegroundColor Yellow
$executionPolicy = Get-ExecutionPolicy -Scope CurrentUser
Write-Host "Current execution policy: $executionPolicy" -ForegroundColor Cyan

if ($executionPolicy -eq "Restricted") {
    Write-Host "Setting execution policy to RemoteSigned..." -ForegroundColor Yellow
    Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser -Force
    Write-Host "✓ Execution policy updated" -ForegroundColor Green
}

Write-Host ""

# Profile content
$profileContent = @'
# Fix .NET SDK PATH issue - Auto-loaded in all PowerShell sessions
$env:DOTNET_ROOT = "C:\Program Files\dotnet"
if ($env:PATH -notlike "*C:\Program Files\dotnet*") {
    $env:PATH = "C:\Program Files\dotnet;" + $env:PATH
}

# Also check for x86 version (if needed)
if (Test-Path "C:\Program Files (x86)\dotnet") {
    if ($env:PATH -notlike "*C:\Program Files (x86)\dotnet*") {
        $env:PATH = "C:\Program Files (x86)\dotnet;" + $env:PATH
    }
}
'@

# Update Windows PowerShell profile (PowerShell 5.1)
Write-Host "Updating Windows PowerShell profile (PowerShell 5.1)..." -ForegroundColor Yellow
$winPSProfile = [System.Environment]::GetFolderPath('MyDocuments') + '\WindowsPowerShell\Microsoft.PowerShell_profile.ps1'
$winPSProfileDir = Split-Path $winPSProfile -Parent

if (-not (Test-Path $winPSProfileDir)) {
    New-Item -Path $winPSProfileDir -ItemType Directory -Force | Out-Null
    Write-Host "  Created directory: $winPSProfileDir" -ForegroundColor Gray
}

# Check if PATH fix already exists
$needsUpdate = $true
if (Test-Path $winPSProfile) {
    $existingContent = Get-Content $winPSProfile -Raw
    if ($existingContent -like "*DOTNET_ROOT*") {
        Write-Host "  PATH fix already exists, updating..." -ForegroundColor Gray
        # Remove old PATH fix if exists
        $lines = Get-Content $winPSProfile
        $newLines = $lines | Where-Object { $_ -notlike "*DOTNET_ROOT*" -and $_ -notlike "*C:\Program Files\dotnet*" -and $_ -notlike "*# Fix .NET SDK PATH*" }
        Set-Content -Path $winPSProfile -Value $newLines -Encoding UTF8
    }
}

Set-Content -Path $winPSProfile -Value $profileContent -Encoding UTF8 -Append
Write-Host "  ✓ Updated: $winPSProfile" -ForegroundColor Green

# Update PowerShell Core profile (PowerShell 7+)
Write-Host "Updating PowerShell Core profile (PowerShell 7+)..." -ForegroundColor Yellow
$psCoreProfile = [System.Environment]::GetFolderPath('MyDocuments') + '\PowerShell\Microsoft.PowerShell_profile.ps1'
$psCoreProfileDir = Split-Path $psCoreProfile -Parent

if (-not (Test-Path $psCoreProfileDir)) {
    New-Item -Path $psCoreProfileDir -ItemType Directory -Force | Out-Null
    Write-Host "  Created directory: $psCoreProfileDir" -ForegroundColor Gray
}

# Check if PATH fix already exists
if (Test-Path $psCoreProfile) {
    $existingContent = Get-Content $psCoreProfile -Raw
    if ($existingContent -like "*DOTNET_ROOT*") {
        Write-Host "  PATH fix already exists, updating..." -ForegroundColor Gray
        # Remove old PATH fix if exists
        $lines = Get-Content $psCoreProfile
        $newLines = $lines | Where-Object { $_ -notlike "*DOTNET_ROOT*" -and $_ -notlike "*C:\Program Files\dotnet*" -and $_ -notlike "*# Fix .NET SDK PATH*" }
        Set-Content -Path $psCoreProfile -Value $newLines -Encoding UTF8
    }
}

Set-Content -Path $psCoreProfile -Value $profileContent -Encoding UTF8 -Append
Write-Host "  ✓ Updated: $psCoreProfile" -ForegroundColor Green

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Testing PATH fix..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Apply to current session
. $winPSProfile
if (Test-Path $psCoreProfile) {
    . $psCoreProfile
}

# Test
try {
    $version = dotnet --version
    Write-Host "✓ PATH fix works! .NET SDK version: $version" -ForegroundColor Green
} catch {
    Write-Host "✗ PATH fix not working in current session" -ForegroundColor Red
    Write-Host "  Try opening a NEW PowerShell window" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Next Steps" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Close this PowerShell window" -ForegroundColor Yellow
Write-Host "2. Open a NEW PowerShell window" -ForegroundColor Yellow
Write-Host "3. Test: dotnet --version" -ForegroundColor Yellow
Write-Host ""
Write-Host "The PATH fix will now work in all future PowerShell sessions!" -ForegroundColor Green
Write-Host ""
