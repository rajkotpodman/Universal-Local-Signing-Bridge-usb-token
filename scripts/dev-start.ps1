param(
    [string]$Mode = "MOCK",
    [int]$Port = 8080
)

$dotnet = "$env:LOCALAPPDATA\Microsoft\dotnet\dotnet.exe"
if (-not (Test-Path $dotnet)) { $dotnet = "dotnet" }

$root = Resolve-Path "$PSScriptRoot\.."

$env:MOCK_MODE = if ($Mode -eq "MOCK") { "true" } else { "false" }
$env:BRIDGE_MODE = $Mode
$env:PORT = $Port

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " Starting Universal Local Signing Bridge ($Mode Mode)..." -ForegroundColor Cyan
Write-Host " Host: http://127.0.0.1:$Port" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

& $dotnet run --project "$root\src\Bridge.Api\Bridge.Api.csproj"
