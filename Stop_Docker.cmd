@echo off
cd /d "%~dp0"
docker compose down
set "result=%errorlevel%"
pause
exit /b %result%
