@echo off
REM First Setup - Double-click to run
REM Run from repo root after a clean clone for Option 1 (Docker) or Option 2 (Local).

REM Ensure we run from the folder containing this batch file (repo root)
cd /d "%~dp0"

echo ========================================
echo Task Management System - First Setup
echo ========================================
echo.

set "SCRIPT_DIR=%~dp0"
powershell.exe -ExecutionPolicy Bypass -NoProfile -File "%SCRIPT_DIR%First setup.ps1"

REM Pause to see any errors
if errorlevel 1 (
    echo.
    echo Setup failed. Check the error messages above.
    pause
    exit /b 1
)

exit /b 0
