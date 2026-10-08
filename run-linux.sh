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

# Check .NET runtime
if ! command -v dotnet &> /dev/null && [ ! -f "./UniversalSigningBridge" ]; then
    echo "[!] Error: .NET 8 runtime is not installed."
    echo "[*] Install with: sudo apt-get install -y dotnet-runtime-8.0"
    exit 1
fi

export ASPNETCORE_URLS="http://127.0.0.1:8080"
export BRIDGE_MODE="PKCS11"
export PORT="8080"

echo "[*] Launching Universal Signing Bridge on http://127.0.0.1:8080 ..."

# Open browser in background after 2 seconds
(sleep 2 && (xdg-open "http://127.0.0.1:8080" 2>/dev/null || sensible-browser "http://127.0.0.1:8080" 2>/dev/null || true)) &

if [ -f "./UniversalSigningBridge" ]; then
    ./UniversalSigningBridge
elif [ -f "src/Bridge.Api/Bridge.Api.csproj" ]; then
    dotnet run --project "src/Bridge.Api/Bridge.Api.csproj"
else
    echo "[!] Executable or project not found."
    exit 1
fi
