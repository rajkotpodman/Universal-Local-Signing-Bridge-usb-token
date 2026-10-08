# Stop Windows Service
param([string]$ServiceName = "UniversalSigningBridge")

Write-Host "Stopping $ServiceName..." -ForegroundColor Cyan
Stop-Service -Name $ServiceName
Get-Service -Name $ServiceName
