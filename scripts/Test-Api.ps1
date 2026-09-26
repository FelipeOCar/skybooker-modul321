$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Net.Http
$root = Split-Path $PSScriptRoot -Parent
$collection = Get-Content (Join-Path $root 'postman/SkyBooker.postman_collection.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$vars = @{}
foreach ($v in $collection.variable) { $vars[$v.key] = [string]$v.value }
$run = [Guid]::NewGuid().ToString('N').Substring(0,10)
$vars.username = 'felipe_' + $run
$vars.email = $vars.username + '@example.com'
$vars.flightNo = 'SK' + $run.ToUpperInvariant()
$vars.departure = [DateTime]::UtcNow.AddDays(7).ToString('o')
$vars.arrival = [DateTime]::UtcNow.AddDays(7).AddHours(2).ToString('o')
function Expand-Variables([string]$Value) {
    foreach ($key in $vars.Keys) { $Value = $Value.Replace('{{' + $key + '}}', [string]$vars[$key]) }
    return $Value
}
$client = [System.Net.Http.HttpClient]::new()
$client.Timeout = [TimeSpan]::FromSeconds(15)
try {
    foreach ($item in $collection.item) {
        $request = [System.Net.Http.HttpRequestMessage]::new([System.Net.Http.HttpMethod]::new($item.request.method), (Expand-Variables $item.request.url))
        $response = $null
        try {
            if ($item.request.auth.type -eq 'bearer') { $request.Headers.Authorization = [System.Net.Http.Headers.AuthenticationHeaderValue]::new('Bearer', $vars.token) }
            if ($item.request.auth.type -eq 'basic') {
                $basic = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($vars.username + ':' + $vars.password))
                $request.Headers.Authorization = [System.Net.Http.Headers.AuthenticationHeaderValue]::new('Basic', $basic)
            }
            if ($item.request.body) { $request.Content = [System.Net.Http.StringContent]::new((Expand-Variables $item.request.body.raw), [Text.Encoding]::UTF8, 'application/json') }
            $response = $client.SendAsync($request).GetAwaiter().GetResult()
            $raw = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
            $lines = $item.event[0].script.exec
            $expected = [int]([regex]::Match($lines[0], 'HTTP (\d+)').Groups[1].Value)
            $status = [int]$response.StatusCode
            if ($status -ne $expected) { throw "$($item.name): erwartet $expected, erhalten $status. $raw" }
            foreach ($line in $lines | Select-Object -Skip 1) {
                $match = [regex]::Match($line, 'pm.collectionVariables.set\("(\w+)", pm.response.json\(\).(\w+)\)')
                if ($match.Success) { $data = $raw | ConvertFrom-Json; $vars[$match.Groups[1].Value] = [string]$data.($match.Groups[2].Value) }
            }
            Write-Host "PASS $($item.name): HTTP $status"
        } finally { if ($null -ne $response) { $response.Dispose() }; $request.Dispose() }
    }
    Write-Host '20 API-Prüfungen bestanden. Testdaten bleiben für die Live-Demo erhalten.'
} catch { Write-Host "FEHLER: $($_.Exception.Message)" -ForegroundColor Red; exit 1 }
finally { $client.Dispose() }
