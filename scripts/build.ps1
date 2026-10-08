# Complete build script for Universal Local Signing Bridge
$ErrorActionPreference = "Stop"

$root = Resolve-Path "$PSScriptRoot\.."
$dotnet = "$env:LOCALAPPDATA\Microsoft\dotnet\dotnet.exe"
if (-not (Test-Path $dotnet)) { $dotnet = "dotnet" }

Write-Host ">>> 1. Building .NET Solution..." -ForegroundColor Cyan
& $dotnet build "$root\UniversalSigningBridge.sln"

Write-Host ">>> 2. Building Frontend Application..." -ForegroundColor Cyan
Set-Location "$root\web"
npm run build

Write-Host ">>> 3. Copying dist to Bridge.Api wwwroot..." -ForegroundColor Cyan
if (Test-Path "$root\src\Bridge.Api\wwwroot") { Remove-Item -Recurse -Force "$root\src\Bridge.Api\wwwroot" }
Copy-Item -Recurse "$root\web\dist" "$root\src\Bridge.Api\wwwroot"

Write-Host ">>> Build completed successfully!" -ForegroundColor Green
