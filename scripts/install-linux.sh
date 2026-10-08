#!/usr/bin/env bash
# ==============================================================================
# Universal Local Signing Bridge - Linux Installation Script
# Installs user systemd service and browser native messaging manifests
# ==============================================================================

set -e

INSTALL_DIR="$HOME/.local/share/universal-signing-bridge"
mkdir -p "$INSTALL_DIR"
mkdir -p "$HOME/.config/systemd/user"

echo "[*] Installing Universal Local Signing Bridge to: $INSTALL_DIR"
cp -r * "$INSTALL_DIR/"

# Install systemd service
cp installer/universal-signing-bridge.service "$HOME/.config/systemd/user/"
systemctl --user daemon-reload
systemctl --user enable --now universal-signing-bridge.service

# Setup Chrome & Chromium Native Messaging
for BROWSER_DIR in "$HOME/.config/google-chrome/NativeMessagingHosts" \
                   "$HOME/.config/chromium/NativeMessagingHosts" \
                   "$HOME/.config/BraveSoftware/Brave-Browser/NativeMessagingHosts" \
                   "$HOME/.config/microsoft-edge/NativeMessagingHosts"; do
    mkdir -p "$BROWSER_DIR"
    cp installer/com.universal.signing.bridge.json "$BROWSER_DIR/" 2>/dev/null || true
done

# Setup Mozilla Firefox Native Messaging
mkdir -p "$HOME/.mozilla/native-messaging-hosts"
cp installer/com.universal.signing.bridge.json "$HOME/.mozilla/native-messaging-hosts/" 2>/dev/null || true

echo "[✓] Installation complete! The bridge service is now running in background."
echo "[*] Open Dashboard at: http://127.0.0.1:8080"
