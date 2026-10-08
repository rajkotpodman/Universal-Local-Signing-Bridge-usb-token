# API Specification

## Base URL
```
http://127.0.0.1:8080
```

---

## 1. Health Probe
### `GET /health`
Returns quick liveness indicator.

#### Response `200 OK`:
```json
{
  "status": "ok",
  "service": "Universal Local Signing Bridge",
  "version": "1.0.0",
  "timestamp": "2026-10-08T06:30:00Z"
}
```

---

## 2. Service Telemetry
### `GET /api/v1/status`
Returns complete runtime operational telemetry.

#### Response `200 OK`:
```json
{
  "service": "Universal Local Signing Bridge",
  "version": "1.0.0",
  "status": "Running",
  "host": "127.0.0.1",
  "port": 8080,
  "uptimeSeconds": 240.5,
  "mockMode": true,
  "activeProviderId": "mock-provider",
  "totalCertificates": 3,
  "smartCardCertificates": 3,
  "activeSessions": 2,
  "wsClients": 1,
  "totalSignOperations": 4,
  "tlsEnabled": false
}
```

---

## 3. Providers
### `GET /api/v1/providers`
Returns list of registered signing engines.

#### Response `200 OK`:
```json
[
  {
    "id": "mock-provider",
    "name": "Development Mock Provider",
    "type": "Simulated PKI",
    "description": "In-memory software simulation for local development.",
    "status": "MOCK",
    "isHardwareBacked": false,
    "supportedAlgorithms": ["SHA-256", "SHA-384", "SHA-512"],
    "canEnumerateTokens": true
  },
  {
    "id": "windows-store",
    "name": "Windows Certificate Store & Smart Card Provider",
    "type": "OS CNG / CryptoAPI",
    "description": "Communicates directly with Windows Certificate Store (CurrentUser\\My).",
    "status": "AVAILABLE",
    "isHardwareBacked": true,
    "supportedAlgorithms": ["SHA-256", "SHA-384", "SHA-512"],
    "canEnumerateTokens": true
  }
]
```

---

## 4. Certificates
### `GET /api/v1/certificates`
Enumerate certificates with private keys across providers.

#### Response `200 OK`:
```json
[
  {
    "id": "c1a2...99b",
    "subject": "CN=JOHN DOE (Mock Class 3 DSC), O=Enterprise Demo CA, C=US",
    "issuer": "CN=Enterprise Demo CA, C=US",
    "serialNumber": "7F0012A45C",
    "thumbprint": "C1A2B3C4D5E6F7A8B9C0D1E2F3A4B5C6D7E8F9A0",
    "validFrom": "2026-09-08T00:00:00Z",
    "validTo": "2027-10-08T23:59:59Z",
    "signatureAlgorithm": "SHA256withRSA",
    "publicKeyAlgorithm": "RSA",
    "keySize": 2048,
    "providerId": "mock-provider",
    "smartCardBacked": true,
    "hasPrivateKey": true,
    "status": "VALID"
  }
]
```

---

## 5. Session Establishment
### `POST /api/v1/session`
Initiates a secure origin-bound session.

#### Request:
```json
{
  "clientAppId": "MyPortal",
  "origin": "http://localhost:5173"
}
```

#### Response `200 OK`:
```json
{
  "success": true,
  "sessionId": "4fa838df48b64e5fae38bce6744fba3799d12a3b4c5d6e7f",
  "createdAt": "2026-10-08T06:30:00Z",
  "expiresAt": "2026-10-08T07:30:00Z",
  "allowedOrigin": "http://localhost:5173"
}
```

---

## 6. Digital Signing
### `POST /api/v1/sign`
Requests a cryptographic signature on data.

#### Headers:
- `Content-Type: application/json`
- `X-Session-Token: <sessionId>`
- `Origin: http://localhost:5173`

#### Request:
```json
{
  "certificateId": "c1a2...99b",
  "hashAlgorithm": "SHA-256",
  "data": "SGVsbG8gV29ybGQ="
}
```

#### Response `200 OK`:
```json
{
  "success": true,
  "signature": "MEYCIQC9x0...Base64Signature...",
  "certificate": {
    "subject": "CN=JOHN DOE (Mock Class 3 DSC), O=Enterprise Demo CA, C=US",
    "issuer": "CN=Enterprise Demo CA, C=US",
    "serialNumber": "7F0012A45C",
    "thumbprint": "C1A2B3C4D5E6F7A8B9C0D1E2F3A4B5C6D7E8F9A0"
  },
  "algorithm": "SHA-256",
  "timestamp": "2026-10-08T06:30:01Z",
  "durationMs": 14.5,
  "providerId": "mock-provider"
}
```

---

## 7. Audit Stream
### `GET /api/v1/audit?limit=50`
Fetches sanitized audit entries from SQLite.

---

## 8. WebSocket Live Stream
### `WS /ws/v1`
Events:
- `connected`
- `service_status`
- `provider_status`
- `certificate_changed`
- `sign_started`
- `sign_completed`
- `sign_failed`
