@echo off
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\Test-Api.ps1"
set "result=%errorlevel%"
pause
exit /b %result%
