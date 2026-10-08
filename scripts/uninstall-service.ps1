# Uninstall Windows Service for Universal Local Signing Bridge
param([string]$ServiceName = "UniversalSigningBridge")

if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Error "Please run this script as Administrator."
    exit 1
}

Stop-Service -Name $ServiceName -ErrorAction SilentlyContinue
sc.exe delete $ServiceName
Write-Host "Windows Service $ServiceName deleted." -ForegroundColor Green
