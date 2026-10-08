# Architecture Overview

The **Universal Local Signing Bridge** is an enterprise-grade localhost middleware service engineered to replace legacy browser plugins, Java applets, and NPAPI extensions for digital signatures and PKI authentication.

```
Browser (Chrome / Edge / Firefox)
   │
   │ HTTPS/HTTP localhost (127.0.0.1:8080)
   ▼
Local Signing Bridge
   │
   ├── REST API v1
   ├── WebSocket API (/ws/v1)
   ├── Browser Diagnostics (6-step automated wizard)
   ├── Certificate Manager
   ├── Audit System (SQLite, SHA-256 digests)
   └── Provider Manager (Multi-provider abstraction)
          │
          ├── Mock Provider (In-memory RSA 2048/4096 & ECDSA)
          ├── Windows Certificate Store Provider (CurrentUser\My, CNG/CAPI)
          ├── PKCS#11 Provider (Hardware module connector)
          └── Future Providers (Linux PKCS#11 / macOS Keychain)
                    │
                    ▼
              USB DSC / Smart Card
                    │
                    ▼
              Protected Private Key (Hardware Bound)
```

## Core Subsystems

### 1. Provider Manager
- Manages dynamic registration and lifecycle of `ISigningProvider` implementations.
- Resolves certificates by ID or thumbprint across active providers.
- Detects provider health: `AVAILABLE`, `UNAVAILABLE`, `ERROR`, `MOCK`.

### 2. Security Middleware
- **Loopback Enforcement**: Only allows connections from loopback IPv4 `127.0.0.1` and `::1`.
- **Origin Validation**: Rejects untrusted origins.
- **Request Size Limiter**: Enforces 5 MB payload ceiling.
- **Rate Limiter**: Sliding window 60 requests/minute per IP on `/api/v1/sign`.

### 3. Session Store & Anti-CSRF
- Generates cryptographically random 192-bit session tokens.
- Binds sessions to caller origin and expires after configurable duration (default 1 hour).

### 4. Audit Trail
- Stores execution records in SQLite (`audit.db`).
- Records SHA-256 digests of document payloads without storing confidential contents.
- Strict policy: Zero private keys, zero PINs, zero raw document storage.
