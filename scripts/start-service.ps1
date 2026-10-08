# Start Windows Service
param([string]$ServiceName = "UniversalSigningBridge")

Write-Host "Starting $ServiceName..." -ForegroundColor Cyan
Start-Service -Name $ServiceName
Get-Service -Name $ServiceName
