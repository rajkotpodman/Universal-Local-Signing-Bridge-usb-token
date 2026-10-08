# Installation & Deployment Guide

## System Requirements
- Windows 10 (1809+) or Windows 11 (64-bit)
- .NET 8.0 SDK or Runtime
- Node.js 18+ (for frontend development)

---

## Startup Methods

### 1. Manual CLI Startup
```powershell
# In Mock Mode
.\scripts\dev-start.ps1 -Mode MOCK -Port 8080

# In Real Hardware Token Mode
.\scripts\dev-start.ps1 -Mode WINDOWS -Port 8080
```

### 2. Windows System Tray Application
```powershell
dotnet run --project src/Bridge.Desktop
```
- Green Tray Icon: Service Ready & Connected
- Yellow Tray Icon: Provider Degraded
- Red Tray Icon: Service Stopped

### 3. Windows Background Service
Open PowerShell as Administrator:
```powershell
.\scripts\install-service.ps1
.\scripts\start-service.ps1
```
