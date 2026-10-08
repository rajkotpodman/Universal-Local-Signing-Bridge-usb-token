# Universal Local Signing Bridge

> **Secure localhost signing middleware connecting browser applications to cryptographic hardware tokens and operating system certificate stores on Windows 10/11.**

---

## 🏛️ Architecture Overview

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

---

## 🔒 Security Principles

1. **Zero Private Key Extraction**: Private keys are maintained strictly inside the physical token or Windows CSP/KSP container. They are never exported or serialized.
2. **Zero PIN Interception**: The bridge never prompts for or stores token PINs. Windows native CSP/KSP dialogs handle user PIN challenges directly.
3. **Strict Loopback Binding**: The service strictly binds to IPv4 `127.0.0.1`. Remote network interfaces are denied.
4. **Origin Allowlisting**: Cross-origin requests from arbitrary external websites are rejected by middleware.
5. **Request Size & Rate Limits**: 5 MB payload ceiling and 60 requests/minute rate limit defend against DoS and automated brute-force attacks.
6. **Audit Hash Integrity**: Audit logs persist only SHA-256 digests of signed data payloads, never raw documents or secrets.

---

## 🚀 Quick Start

### 1. Build Backend and Frontend
```powershell
.\scripts\build.ps1
```

### 2. Run All Automated Tests
```powershell
.\scripts\test.ps1
```

### 3. Start Local Bridge Service
```powershell
# Development Mock Mode
.\scripts\dev-start.ps1 -Mode MOCK -Port 8080

# Real USB Hardware Token Mode
.\scripts\dev-start.ps1 -Mode WINDOWS -Port 8080
```

### 4. Start Web Application (Development)
```powershell
cd web
npm run dev -- --host 127.0.0.1 --port 5173
```

---

## 🌐 Live URLs

- **Enterprise Dashboard**: [http://127.0.0.1:8080](http://127.0.0.1:8080)
- **Frontend Dev UI**: [http://127.0.0.1:5173](http://127.0.0.1:5173)
- **Interactive Swagger**: [http://127.0.0.1:8080/swagger](http://127.0.0.1:8080/swagger)
- **Browser Diagnostics**: [http://127.0.0.1:5173/diagnostics](http://127.0.0.1:5173/diagnostics)
- **Signing Test**: [http://127.0.0.1:5173/sign](http://127.0.0.1:5173/sign)
- **WebSocket Channel**: `ws://127.0.0.1:8080/ws/v1`

---

## 📁 Project Structure

```
├── src/
│   ├── Bridge.Api/                  # ASP.NET Core Minimal API (.NET 8)
│   ├── Bridge.Core/                 # Domain models, ISigningProvider, config
│   ├── Bridge.Security/             # OriginValidator, RateLimiter, LocalhostEnforcer
│   ├── Bridge.Crypto/               # PAdES-BES, XAdES-BES, CAdES-BES, TSA Client, SQLite stores
│   ├── Bridge.Providers/            # ProviderManager & PC/SC WinSCard Hardware Monitor
│   ├── Bridge.Provider.Mock/        # MockSigningProvider (RSA & ECDSA test keys)
│   ├── Bridge.Provider.Windows/     # WindowsCertificateProvider (CurrentUser\My, CNG/CAPI)
│   ├── Bridge.Provider.Pkcs11/      # Pkcs11SigningProvider (Vendor PKCS#11 DLLs)
│   └── Bridge.Desktop/              # Native Messaging Host for Chrome/Edge/Firefox
├── web/                             # React 18 + TypeScript + Vite Dashboard & Signing Studio
├── tests/                           # 41 unit & integration tests (100% passing)
├── installer/                       # Native messaging host registry manifest
└── scripts/                         # Build, test, and registration automation scripts
```

---

## 🏆 Top 10 World-Class PKI Repositories Integrated

| Repository | Capability Integrated | Implementation in Bridge |
|---|---|---|
| **Pkcs11Interop/Pkcs11Interop** | PKCS#11 multi-slot hardware token connector | `Bridge.Provider.Pkcs11` vendor DLL scanner |
| **web-eid/web-eid-app** | Native messaging browser host & origin security | `Bridge.Desktop.NativeMessagingHost` + manifest |
| **PeculiarVentures/fortify** | Localhost browser cryptographic bridge | REST `/api/v1` + WebSocket `/ws/v1` |
| **OpenSC/OpenSC** | ISO 7816 APDU ATR hardware profile matching | `PcscHardwareMonitor` ATR database & detection |
| **danm-de/pcsc-sharp** | Native WinSCard PC/SC reader insertion monitor | `winscard.dll` P/Invoke hardware reader polling |
| **esig/dss** | ETSI PAdES, CAdES, XAdES standard signatures | `PdfSignerEngine`, `CadesSignerEngine`, `XadesSignerEngine` |
| **bcgit/bc-csharp** | RFC 3161 TSA timestamping client & CMS encoding | `TsaClient` ASN.1 DER parser & `SignedCms` |
| **Yubico/Yubico.NET.SDK** | PIV smart card container recognition | YubiKey 5 ATR signature detection |
| **damianofalcioni/Websocket-Smart-Card-Signer** | Streaming WebSocket interactive signing | `WebSocketBridgeService` live broadcast |
| **itext/itext-dotnet** | Deferred PDF signature injection & visual seals | `PdfSignerEngine` ByteRange calculation |

---

## 📡 Advanced API Endpoints

### Hardware & Provider Discovery
- `GET /api/v1/hardware/readers`: Real-time PC/SC WinSCard reader enumeration and ATR smart card identification.
- `GET /api/v1/providers/world-catalog`: Complete catalog of Aladdin eToken, SafeNet, Feitian, ProxKey, and YubiKey providers.
- `POST /api/v1/providers/scan`: Scan USB bus and cryptographic subsystems.

### Enterprise Document Signing
- `POST /api/v1/sign/pdf`: ISO 32000-1 / ETSI TS 102 778 PAdES-BES PDF signing with visual appearance seals and RFC 3161 timestamps.
- `POST /api/v1/sign/cades`: RFC 5652 / ETSI TS 101 733 CAdES-BES detached CMS signature container.
- `POST /api/v1/sign/xml`: W3C XMLDSIG / ETSI TS 101 903 enveloped XML signature generator.
- `POST /api/v1/sign/batch`: Batch signing of multiple documents in a single transaction.
- `POST /api/v1/timestamp`: Request standard RFC 3161 cryptographic timestamp token from trusted TSA authorities.
- `POST /api/v1/sign`: Raw cryptographic signing (SHA-256, SHA-384, SHA-512) via USB token or Windows Certificate Store.
