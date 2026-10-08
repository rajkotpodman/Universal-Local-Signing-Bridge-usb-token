# Cryptographic Providers Guide

The Universal Local Signing Bridge supports a multi-provider pluggable architecture via `ISigningProvider`.

## Available Providers

### 1. Mock Signing Provider (`mock-provider`)
- **Status**: `MOCK`
- **Use Case**: Local development, CI/CD automated test runs, environments without physical USB tokens.
- **Capabilities**: Generates in-memory RSA 2048, RSA 4096, and ECDSA P-256 test keypairs.
- **Activation**: Set `$env:MOCK_MODE = "true"` or `$env:BRIDGE_MODE = "MOCK"`.

### 2. Windows Certificate Provider (`windows-store`)
- **Status**: `AVAILABLE` on Windows 10/11
- **Use Case**: Physical USB tokens (ePass2003, Watchdata ProxKey, SafeNet, mToken) and Windows software CSPs.
- **Capabilities**: Directly queries `CurrentUser\My`. Prompts for token PIN through native OS dialog.
- **Activation**: Set `$env:MOCK_MODE = "false"` and `$env:BRIDGE_MODE = "WINDOWS"`.

### 3. PKCS#11 Hardware Provider (`pkcs11`)
- **Status**: `AVAILABLE` when driver DLL is detected; `UNAVAILABLE` otherwise.
- **Use Case**: Direct interfacing with vendor PKCS#11 dynamic libraries (`eps2003csp11.dll`, `opensc-pkcs11.dll`, etc.).
- **Activation**: Set `Pkcs11LibraryPath` in `appsettings.json` or `$env:BRIDGE_MODE = "PKCS11"`.
