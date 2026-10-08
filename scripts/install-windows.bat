@echo off
title Install Universal Local Signing Bridge
color 0A
chcp 65001 >nul

echo ===============================================================================
echo          INSTALLING UNIVERSAL LOCAL SIGNING BRIDGE (Windows)
echo ===============================================================================
echo.

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0register-native-host.ps1"

echo.
echo [✓] Native messaging host registration complete for Chrome, Edge, and Brave!
echo [*] The bridge can now communicate directly with browser extensions.
echo.
pause
