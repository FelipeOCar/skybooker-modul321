param([switch]$InfrastructureOnly)
$ErrorActionPreference = 'Stop'
& "$PSScriptRoot/Setup.ps1"
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) { throw 'Docker Desktop installieren und starten.' }
$os = & docker info --format '{{.OSType}}'
if ($LASTEXITCODE -ne 0 -or $os -ne 'linux') { throw 'Docker Desktop mit Linux-Containern starten.' }
if ($InfrastructureOnly) {
    & docker compose stop auth-service flight-service booking-service
    if ($LASTEXITCODE -ne 0) { throw 'API-Container konnten nicht gestoppt werden.' }
    & docker compose up -d --wait --wait-timeout 240 mongodb sqlserver
    if ($LASTEXITCODE -ne 0) { throw 'Datenbanken nicht bereit. docker compose logs mongodb sqlserver prüfen.' }
    Write-Host 'Datenbanken bereit. Jetzt alle drei Webprojekte in Visual Studio oder VS Code starten.'
    exit 0
}
& docker compose up --build -d --wait --wait-timeout 300
if ($LASTEXITCODE -ne 0) { throw 'Start fehlgeschlagen. docker compose logs --tail=80 prüfen.' }
foreach ($port in 32510,32511,32512) { Start-Process "http://127.0.0.1:$port/swagger" }
Write-Host 'SkyBooker läuft. Postman-Sammlung importieren oder Test_API.cmd ausführen.'
