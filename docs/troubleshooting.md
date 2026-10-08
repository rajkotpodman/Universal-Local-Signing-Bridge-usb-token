# Troubleshooting Guide

## 1. Diagnostics Step 5 Fails ("Zero certificates detected")
- **Cause**: Windows store has no certificate with an exportable/usable private key container.
- **Fix**: Plug in your physical USB token, install token vendor drivers, and verify certificate in `certmgr.msc` under `Personal > Certificates`. Alternatively, switch to mock mode (`$env:MOCK_MODE="true"`).

## 2. Diagnostics Step 6 Fails ("Signing operation failed")
- **Cause**: User cancelled token PIN entry or PIN is locked.
- **Fix**: Check Windows taskbar for the blinking Smart Card PIN entry prompt. Test PIN unlock in manufacturer PKI management utility.

## 3. Port Conflict (8080 already in use)
- **Cause**: Another service is using port 8080.
- **Fix**: The bridge automatically scans ports 8080-8130 and binds to the first available port. The selected port is displayed in the startup console, system tray, and health probe.

## 4. Origin Rejected (HTTP 403 Forbidden)
- **Cause**: Browser website domain is not in `AllowedOrigins`.
- **Fix**: Add the calling web application origin to `Bridge:AllowedOrigins` in `src/Bridge.Api/appsettings.json`.
