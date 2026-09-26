@echo off
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\Start.ps1" -InfrastructureOnly
set "result=%errorlevel%"
pause
exit /b %result%
