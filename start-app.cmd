@echo off
rem Double-click to start the whole app in Docker (see scripts\start-app.ps1). Needs only Docker Desktop.
rem Applies migrations, activates the local plant calendar if not yet active, and opens the browser.
cd /d "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\start-app.ps1" -ActivateCalendar -Open
if errorlevel 1 (
    echo.
    echo Start failed. Read the messages above.
)
echo.
pause
