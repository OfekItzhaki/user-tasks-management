@echo off
REM First Setup - Double-click to run
REM This batch file runs the PowerShell setup script

echo ========================================
echo Task Management System - First Setup
echo ========================================
echo.
echo Starting automated setup...
echo.

REM Get the directory where this batch file is located
set "SCRIPT_DIR=%~dp0"

REM Run the PowerShell script
powershell.exe -ExecutionPolicy Bypass -NoProfile -File "%SCRIPT_DIR%First setup.ps1"

REM Pause to see any errors
if errorlevel 1 (
    echo.
    echo Setup failed. Check the error messages above.
    pause
    exit /b 1
)

exit /b 0
