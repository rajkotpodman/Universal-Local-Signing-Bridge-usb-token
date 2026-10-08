#!/usr/bin/env bash
# ==============================================================================
# Universal Local Signing Bridge - macOS Launcher
# Supports Apple Silicon (M1/M2/M3/M4) & Intel Macs
# ==============================================================================

set -e

echo "==============================================================================="
echo "                UNIVERSAL LOCAL SIGNING BRIDGE v1.0.0 (macOS)"
echo "       Enterprise Smart Card, USB DSC, and PKI Localhost Middleware"
echo "==============================================================================="
echo ""

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

if ! command -v dotnet &> /dev/null && [ ! -f "./UniversalSigningBridge" ]; then
    echo "[!] Error: .NET 8 runtime is not installed."
    echo "[*] Install with Homebrew: brew install dotnet"
    exit 1
fi

export ASPNETCORE_URLS="http://127.0.0.1:8080"
export BRIDGE_MODE="PKCS11"
export PORT="8080"

echo "[*] Launching Universal Signing Bridge on http://127.0.0.1:8080 ..."

# Open browser in background after 2 seconds
(sleep 2 && open "http://127.0.0.1:8080" 2>/dev/null || true) &

if [ -f "./UniversalSigningBridge" ]; then
    ./UniversalSigningBridge
elif [ -f "src/Bridge.Api/Bridge.Api.csproj" ]; then
    dotnet run --project "src/Bridge.Api/Bridge.Api.csproj"
else
    echo "[!] Executable or project not found."
    exit 1
fi
