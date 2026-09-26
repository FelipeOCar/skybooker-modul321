$ErrorActionPreference = 'Stop'
Set-Location (Split-Path $PSScriptRoot -Parent)
$root = Get-Location
$localPath = Join-Path $root 'skybooker.local.json'
$envPath = Join-Path $root '.env'
function Random-Secret([int]$Length) {
    $bytes = New-Object byte[] $Length
    $rng = [Security.Cryptography.RandomNumberGenerator]::Create()
    try { $rng.GetBytes($bytes) } finally { $rng.Dispose() }
    return [Convert]::ToBase64String($bytes)
}
# Preserve database passwords on every subsequent run, including VS/VS Code switches.
if (Test-Path $envPath) {
    $values = @{}
    foreach ($line in Get-Content $envPath) {
        if ($line -match '^([A-Z_]+)=(.*)$') { $values[$Matches[1]] = $Matches[2] }
    }
    foreach ($name in 'JWT_KEY','MSSQL_SA_PASSWORD','MONGO_ROOT_PASSWORD') {
        if (-not $values[$name]) { throw "$name fehlt in .env. Bestehende Datenbank-Passwörter nicht neu erzeugen." }
    }
} else {
    if (Test-Path $localPath) { throw '.env fehlt, aber die lokale Konfiguration existiert. Die zugehörige .env wiederherstellen.' }
    $values = @{
        JWT_KEY = (Random-Secret 64)
        MSSQL_SA_PASSWORD = ('Sky!9' + (Random-Secret 24))
        MONGO_ROOT_PASSWORD = ('Sky!9' + (Random-Secret 24))
    }
    $text = "JWT_KEY=$($values.JWT_KEY)`nMSSQL_SA_PASSWORD=$($values.MSSQL_SA_PASSWORD)`nMONGO_ROOT_PASSWORD=$($values.MONGO_ROOT_PASSWORD)`n"
    [IO.File]::WriteAllText($envPath, $text, (New-Object Text.UTF8Encoding($false)))
}
$mongoPass = [Uri]::EscapeDataString($values.MONGO_ROOT_PASSWORD)
$configuration = @{
    Jwt = @{ Key = $values.JWT_KEY }
    ConnectionStrings = @{
        FlightDb = "mongodb://root:${mongoPass}@127.0.0.1:32517/?authSource=admin"
        BookingDb = "Server=127.0.0.1,32533;Database=SkyBookerBookings;User Id=sa;Password=$($values.MSSQL_SA_PASSWORD);Encrypt=True;TrustServerCertificate=True"
    }
    Services = @{ FlightUrl = 'http://127.0.0.1:32511/' }
}
$configuration | ConvertTo-Json -Depth 5 | ForEach-Object { [IO.File]::WriteAllText($localPath, $_, (New-Object Text.UTF8Encoding($false))) }
Write-Host 'Konfiguration bereit. Vorhandene Schlüssel und Datenbank-Passwörter bleiben erhalten.'
