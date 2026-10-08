#!/usr/bin/env bash
# ==============================================================================
# Universal Local Signing Bridge - macOS Installation Script
# Installs launchd background agent and browser native messaging manifests
# ==============================================================================

set -e

INSTALL_DIR="/Users/Shared/universal-signing-bridge"
mkdir -p "$INSTALL_DIR"
mkdir -p "$HOME/Library/LaunchAgents"

echo "[*] Installing Universal Local Signing Bridge to: $INSTALL_DIR"
cp -r * "$INSTALL_DIR/"

# Install launchd agent
cp installer/com.universal.signing.bridge.plist "$HOME/Library/LaunchAgents/"
launchctl unload "$HOME/Library/LaunchAgents/com.universal.signing.bridge.plist" 2>/dev/null || true
launchctl load "$HOME/Library/LaunchAgents/com.universal.signing.bridge.plist"

# Setup Chrome & Edge Native Messaging
for BROWSER_DIR in "$HOME/Library/Application Support/Google/Chrome/NativeMessagingHosts" \
                   "$HOME/Library/Application Support/Microsoft Edge/NativeMessagingHosts" \
                   "$HOME/Library/Application Support/BraveSoftware/Brave-Browser/NativeMessagingHosts"; do
    mkdir -p "$BROWSER_DIR"
    cp installer/com.universal.signing.bridge.json "$BROWSER_DIR/" 2>/dev/null || true
done

# Setup Mozilla Firefox Native Messaging
mkdir -p "$HOME/Library/Application Support/Mozilla/NativeMessagingHosts"
cp installer/com.universal.signing.bridge.json "$HOME/Library/Application Support/Mozilla/NativeMessagingHosts/" 2>/dev/null || true

echo "[✓] Installation complete! The bridge service is now running in background."
echo "[*] Open Dashboard at: http://127.0.0.1:8080"
