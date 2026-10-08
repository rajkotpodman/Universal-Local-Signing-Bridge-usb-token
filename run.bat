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

:: Check for pre-built standalone binaries first (Zero prerequisites needed!)
if exist "UniversalSigningBridge.exe" (
    echo [*] Launching portable standalone UniversalSigningBridge.exe ...
    start "" cmd /c "timeout /t 2 /nobreak >nul & start http://127.0.0.1:8080"
    UniversalSigningBridge.exe
    exit /b 0
)
if exist "Bridge.Api.exe" (
    echo [*] Launching portable standalone Bridge.Api.exe ...
    start "" cmd /c "timeout /t 2 /nobreak >nul & start http://127.0.0.1:8080"
    Bridge.Api.exe
    exit /b 0
)

:: Check if dotnet or pre-built binary exists in bin
where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    if not exist "bin\Release\net8.0-windows\Bridge.Api.exe" (
        color 0C
        echo [!] Error: Neither standalone executable nor .NET 8 runtime was found.
        echo [*] Download the pre-packaged standalone release or install .NET 8 from: https://dotnet.microsoft.com/download/dotnet/8.0
        echo.
        pause
        exit /b 1
    )
)

echo [*] Starting Universal Signing Bridge on http://127.0.0.1:8080 ...
echo [*] Supported Tokens: Aladdin eToken, SafeNet, ProxKey, ePass2003, mToken, YubiKey
echo.

:: Automatically launch browser after 2 seconds in background
start "" cmd /c "timeout /t 2 /nobreak >nul & start http://127.0.0.1:8080"

:: Start via dotnet or bin
if exist "src\Bridge.Api\Bridge.Api.csproj" (
    set MOCK_MODE=false
    set BRIDGE_MODE=WINDOWS
    set PORT=8080
    dotnet run --project "src\Bridge.Api\Bridge.Api.csproj"
) else if exist "bin\Release\net8.0-windows\Bridge.Api.exe" (
    bin\Release\net8.0-windows\Bridge.Api.exe
) else (
    echo [!] Could not locate project or executable.
    pause
)
