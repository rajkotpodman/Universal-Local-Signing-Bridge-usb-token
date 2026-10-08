# Build and package script for Universal Local Signing Bridge
$ErrorActionPreference = "Stop"

$root = Resolve-Path "$PSScriptRoot\.."
$publishDir = "$root\publish"

Write-Host ">>> 1. Building Frontend UI..." -ForegroundColor Cyan
Set-Location "$root\web"
npm run build

Write-Host ">>> 2. Copying web dist to Bridge.Api wwwroot..." -ForegroundColor Cyan
if (Test-Path "$root\src\Bridge.Api\wwwroot") { Remove-Item -Recurse -Force "$root\src\Bridge.Api\wwwroot" }
Copy-Item -Recurse "$root\web\dist" "$root\src\Bridge.Api\wwwroot"

Write-Host ">>> 3. Publishing Desktop and Api to $publishDir..." -ForegroundColor Cyan
if (Test-Path $publishDir) { Remove-Item -Recurse -Force $publishDir }

$dotnet = "$env:LOCALAPPDATA\Microsoft\dotnet\dotnet.exe"
if (-not (Test-Path $dotnet)) { $dotnet = "dotnet" }

& $dotnet publish "$root\src\Bridge.Desktop\Bridge.Desktop.csproj" -c Release -r win-x64 --self-contained false -o $publishDir

Write-Host ">>> Published successfully to: $publishDir" -ForegroundColor Green
