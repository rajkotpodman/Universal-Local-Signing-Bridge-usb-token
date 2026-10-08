# 🚀 Universal Local Signing Bridge v1.0.0 (Heavy Standalone & Portable Edition)

## 📦 Zero Prerequisites / Complete Standalone Portable Packages
કોઈપણ વધારાના સોફ્ટવેર કે .NET SDK ઇન્સ્ટોલ કરવાની જરૂર નથી! આ પેકેજ સંપૂર્ણપણે સ્વનિર્ભર (Self-Contained) છે, જેમાં સંપૂર્ણ .NET 8 રનટાઇમ, ક્રિપ્ટોગ્રાફિક એન્જિન અને વેબ ડેશબોર્ડ પહેલેથી જ સમાવિષ્ટ છે. ફક્ત ડાઉનલોડ કરો અને ડબલ-ક્લિક કરીને ચાલુ કરો!

### 📥 Standalone Downloads (સંપૂર્ણ પોર્ટેબલ એપ્લિકેશન ડાઉનલોડ્સ):
- **🪟 Windows (x64)**: [`UniversalLocalSigningBridge-Windows-x64.zip`](https://github.com/rajkotpodman/Universal-Local-Signing-Bridge-usb-token/releases/download/v1.0.0/UniversalLocalSigningBridge-Windows-x64.zip) (~145.5 MB)
  - *સમાવેશ*: `UniversalSigningBridge.exe`, `Bridge.Api.exe`, `run.bat` (1-ક્લિક લોન્ચર), `installer/` (Inno Setup), `scripts/` (Automated Service Setup), `src/` (સંપૂર્ણ સોર્સ કોડ), `wwwroot/` (Bilingual Web Dashboard).
  - *વાપરવા માટે*: ઝિપ ફાઇલ અનઝિપ કરો અને `run.bat` અથવા `UniversalSigningBridge.exe` પર ડબલ-ક્લિક કરો! બ્રાઉઝર આપમેળે ખૂલશે (`http://127.0.0.1:8080`).

- **🐧 Linux (Ubuntu / Debian / Fedora / RHEL / Arch - x64)**: [`UniversalLocalSigningBridge-Linux-x64.tar.gz`](https://github.com/rajkotpodman/Universal-Local-Signing-Bridge-usb-token/releases/download/v1.0.0/UniversalLocalSigningBridge-Linux-x64.tar.gz) (~137.0 MB)
  - *સમાવેશ*: `UniversalSigningBridge`, `Bridge.Api`, `run-linux.sh` (Auto-execute launcher), `installer/universal-signing-bridge.service` (systemd user daemon), `src/` (સંપૂર્ણ સોર્સ કોડ).
  - *વાપરવા માટે*: `tar -xzf UniversalLocalSigningBridge-Linux-x64.tar.gz && ./run-linux.sh`

- **🍏 macOS Apple Silicon (M1 / M2 / M3 / M4 - arm64)**: [`UniversalLocalSigningBridge-macOS-arm64.tar.gz`](https://github.com/rajkotpodman/Universal-Local-Signing-Bridge-usb-token/releases/download/v1.0.0/UniversalLocalSigningBridge-macOS-arm64.tar.gz) (~131.7 MB)
  - *સમાવેશ*: Native Apple Silicon binary `UniversalSigningBridge`, `Bridge.Api`, `run-macos.sh`, `installer/com.universal.signing.bridge.plist` (launchd agent), `src/` (સંપૂર્ણ સોર્સ કોડ).
  - *વાપરવા માટે*: `tar -xzf UniversalLocalSigningBridge-macOS-arm64.tar.gz && ./run-macos.sh`

- **📁 Full Source Code Archive**: [`UniversalLocalSigningBridge-v1.0.0-SourceCode.zip`](https://github.com/rajkotpodman/Universal-Local-Signing-Bridge-usb-token/releases/download/v1.0.0/UniversalLocalSigningBridge-v1.0.0-SourceCode.zip)

---

### 🛡️ Enterprise Hardware Token Compatibility
- **Aladdin / SafeNet eToken**: 5110, 5100, Pro 72K (PKCS#11 `eTPKCS11.dll` / `libeTPkcs11.so`)
- **Watchdata ProxKey / TrustKey**: CAPI & PKCS#11
- **Feitian ePass2003**: CSP & PKCS#11 (`eps2003csp11.dll`)
- **mToken CryptoID**: Full PKCS#11 support
- **YubiKey 5 Series**: PIV & PKCS#11
- **Windows CAPI / CNG Store**: MY / Root / Personal certificate discovery
- **WinSCard PC/SC**: Real-time physical card insertion/removal hardware listener

### ✨ Integrated Advanced Features
1. **Bilingual GUI**: English & ગુજરાતી toggle dynamically in the top bar and sidebar.
2. **1-Click Quick PDF Signer Widget**: Drag-and-drop any PDF file to instantly sign and download the signed PAdES-BES file with visual timestamp seal.
3. **Enterprise Standards**: PAdES-BES (ISO 32000-1), XAdES-BES (W3C/ETSI TS 101 903), CAdES-BES (RFC 5652 / ETSI TS 101 733), RFC 3161 TSA Timestamping.
4. **Zero-Key-Theft Guarantee**: Private keys NEVER leave the physical token hardware. PINs are never persisted or logged.
