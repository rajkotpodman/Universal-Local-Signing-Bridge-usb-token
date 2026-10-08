@echo off
title Universal Local Signing Bridge
color 0B
chcp 65001 >nul

echo ===============================================================================
echo                UNIVERSAL LOCAL SIGNING BRIDGE v1.0.0
echo       Enterprise Smart Card, USB DSC, and PKI Localhost Middleware
echo ===============================================================================
echo.
echo [*] Checking runtime environment...

:: Check if dotnet or pre-built binary exists
where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    if not exist "UniversalSigningBridge.exe" (
        if not exist "bin\Release\net8.0-windows\Bridge.Api.exe" (
            color 0C
            echo [!] Error: .NET 8 runtime is not installed and pre-built binary was not found.
            echo [*] Please install .NET 8 Runtime from: https://dotnet.microsoft.com/download/dotnet/8.0
            echo.
            pause
            exit /b 1
        )
    )
)

echo [*] Starting Universal Signing Bridge on http://127.0.0.1:8080 ...
echo [*] Supported Tokens: Aladdin eToken, SafeNet, ProxKey, ePass2003, mToken, YubiKey
echo.

:: Automatically launch browser after 2 seconds in background
start "" cmd /c "timeout /t 2 /nobreak >nul & start http://127.0.0.1:8080"

:: Start the API service
if exist "UniversalSigningBridge.exe" (
    UniversalSigningBridge.exe
) else if exist "src\Bridge.Api\Bridge.Api.csproj" (
    set MOCK_MODE=false
    set BRIDGE_MODE=WINDOWS
    set PORT=8080
    dotnet run --project "src\Bridge.Api\Bridge.Api.csproj"
) else (
    echo [!] Could not locate project or executable.
    pause
)
