#!/usr/bin/env bash
# ==============================================================================
# Universal Local Signing Bridge - Linux Launcher
# Supports Ubuntu, Debian, Fedora, RHEL, Arch Linux
# ==============================================================================

set -e

echo "==============================================================================="
echo "                UNIVERSAL LOCAL SIGNING BRIDGE v1.0.0 (Linux)"
echo "       Enterprise Smart Card, USB DSC, and PKI Localhost Middleware"
echo "==============================================================================="
echo ""

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

# Check for standalone binary first (Zero prerequisites needed!)
if [ -f "./UniversalSigningBridge" ]; then
    echo "[*] Launching portable standalone UniversalSigningBridge ..."
    chmod +x ./UniversalSigningBridge 2>/dev/null || true
    (sleep 2 && (xdg-open "http://127.0.0.1:8080" 2>/dev/null || sensible-browser "http://127.0.0.1:8080" 2>/dev/null || true)) &
    ./UniversalSigningBridge
    exit 0
elif [ -f "./Bridge.Api" ]; then
    echo "[*] Launching portable standalone Bridge.Api ..."
    chmod +x ./Bridge.Api 2>/dev/null || true
    (sleep 2 && (xdg-open "http://127.0.0.1:8080" 2>/dev/null || sensible-browser "http://127.0.0.1:8080" 2>/dev/null || true)) &
    ./Bridge.Api
    exit 0
fi

# Fallback: check dotnet
if ! command -v dotnet &> /dev/null; then
    echo "[!] Error: Neither standalone binary nor .NET 8 runtime was found."
    echo "[*] Download the standalone release or install .NET: sudo apt-get install -y dotnet-runtime-8.0"
    exit 1
fi

export ASPNETCORE_URLS="http://127.0.0.1:8080"
export BRIDGE_MODE="PKCS11"
export PORT="8080"

echo "[*] Launching Universal Signing Bridge on http://127.0.0.1:8080 ..."

# Open browser in background after 2 seconds
(sleep 2 && (xdg-open "http://127.0.0.1:8080" 2>/dev/null || sensible-browser "http://127.0.0.1:8080" 2>/dev/null || true)) &

if [ -f "src/Bridge.Api/Bridge.Api.csproj" ]; then
    dotnet run --project "src/Bridge.Api/Bridge.Api.csproj"
else
    echo "[!] Executable or project not found."
    exit 1
fi
