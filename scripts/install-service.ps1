# Install Windows Service for Universal Local Signing Bridge
param(
    [string]$ServiceName = "UniversalSigningBridge",
    [string]$DisplayName = "Universal Local Signing Bridge Service",
    [string]$BinaryPath = ""
)

if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Error "Please run this script as Administrator."
    exit 1
}

$root = Resolve-Path "$PSScriptRoot\.."
if ([string]::IsNullOrWhiteSpace($BinaryPath)) {
    $BinaryPath = "$root\publish\Bridge.Api.exe"
}

Write-Host "Creating Windows Service: $ServiceName..." -ForegroundColor Cyan
New-Service -Name $ServiceName -BinaryPathName $BinaryPath -DisplayName $DisplayName -StartupType Automatic -Description "Localhost PKI and smart card signing middleware"
Write-Host "Service $ServiceName successfully registered!" -ForegroundColor Green
