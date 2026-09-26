@echo off
cd /d "%~dp0"
dotnet test SkyBooker.sln --configuration Release --logger "trx;LogFileName=SkyBooker.trx"
set "result=%errorlevel%"
pause
exit /b %result%
