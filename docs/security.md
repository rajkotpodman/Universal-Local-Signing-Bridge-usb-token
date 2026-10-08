# Security Model & Policy

## 1. Zero Private Key Extraction Policy
1. The bridge will NEVER extract, export, serialize, or log private keys.
2. Digital signatures are generated either directly inside the physical USB cryptographic token or via Windows Cryptography Next Generation (CNG) APIs.
3. Private keys are marked as non-exportable within the key storage container.

## 2. Zero PIN Interception Policy
1. The bridge will NEVER collect, prompt for, or log token PINs.
2. When a physical USB smart-card token requires a PIN, the request is delegated to Windows CSP/KSP, which displays the manufacturer's authentic native PIN dialog.
3. The bridge application memory never contains the user's PIN string.

## 3. Strict Loopback Binding
1. The bridge HTTP listener binds strictly to `127.0.0.1`.
2. It will never bind to `0.0.0.0` or public network adapters.
3. Requests arriving from non-loopback network addresses are blocked at the socket and middleware layers.

## 4. Origin Allowlisting & Anti-DNS Rebinding
1. Every incoming HTTP and WebSocket request must pass `SecurityMiddleware`.
2. The `Origin` header must match one of the allowed origins in `appsettings.json`.
3. Wildcard `*` origins are strictly forbidden.

## 5. Denial of Service & Brute-Force Defenses
1. **Request Payload Size**: Capped at 5 MB (`5242880` bytes). Payloads exceeding this receive HTTP `413 Payload Too Large`.
2. **Rate Limiting**: Signing requests are rate-limited to 60 requests/minute per client IP to safeguard hardware tokens against automated PIN brute-force attempts.
3. **Session Verification**: Sensitive signing calls require a valid `X-Session-Token` issued by `/api/v1/session`.
