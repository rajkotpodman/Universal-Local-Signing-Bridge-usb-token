# Test script for Universal Local Signing Bridge
$ErrorActionPreference = "Stop"

$root = Resolve-Path "$PSScriptRoot\.."
$dotnet = "$env:LOCALAPPDATA\Microsoft\dotnet\dotnet.exe"
if (-not (Test-Path $dotnet)) { $dotnet = "dotnet" }

Write-Host ">>> Running Unit, Security, and Integration Test Suites..." -ForegroundColor Cyan
& $dotnet test "$root\UniversalSigningBridge.sln" --logger "console;verbosity=normal"
