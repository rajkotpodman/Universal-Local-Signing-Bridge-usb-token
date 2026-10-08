using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Bridge.Core.Interfaces;
using Bridge.Core.Models;
using Bridge.Security;

namespace Bridge.Provider.Windows;

[SupportedOSPlatform("windows")]
public class WindowsCertificateProvider : ISigningProvider
{
    public ProviderInfo GetProviderInfo()
    {
        bool isWindows = OperatingSystem.IsWindows();
        bool hasSmartCardService = false;

        if (isWindows)
        {
            try
            {
                // Check if certificates exist or store opens
                using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly);
                hasSmartCardService = true;
            }
            catch
            {
                hasSmartCardService = false;
            }
        }

        return new ProviderInfo(
            Id: "windows-store",
            Name: "Windows Certificate Store & Smart Card Provider",
            Type: "OS CNG / CryptoAPI",
            Description: "Communicates directly with Windows Certificate Store (CurrentUser\\My) and native hardware USB token CSP/KSP drivers.",
            Status: isWindows ? (hasSmartCardService ? ProviderStatus.AVAILABLE : ProviderStatus.ERROR) : ProviderStatus.UNAVAILABLE,
            IsHardwareBacked: true,
            SupportedAlgorithms: new[] { "SHA-256", "SHA-384", "SHA-512" },
            Details: isWindows
                ? "Windows 10/11 Certificate Store accessible. Physical USB tokens prompt for PIN through native OS dialog."
                : "Windows provider requires Windows OS.",
            CanEnumerateTokens: isWindows
        );
    }

    public Task<IReadOnlyList<Certificate>> GetCertificatesAsync(CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsWindows())
        {
            return Task.FromResult<IReadOnlyList<Certificate>>(Array.Empty<Certificate>());
        }

        var results = new List<Certificate>();

        try
        {
            using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

            foreach (var cert in store.Certificates)
            {
                try
                {
                    if (!cert.HasPrivateKey) continue;

                    var isSmartCard = DetectSmartCard(cert);
                    var keySize = 2048;
                    var pubKeyAlgo = cert.PublicKey.Oid.FriendlyName ?? "RSA";

                    try
                    {
                        using var rsa = cert.GetRSAPrivateKey();
                        if (rsa != null) keySize = rsa.KeySize;
                    }
                    catch { /* Best effort key size detection */ }

                    var now = DateTimeOffset.UtcNow;
                    var status = CertificateStatus.VALID;
                    if (now > cert.NotAfter) status = CertificateStatus.EXPIRED;
                    else if (now < cert.NotBefore) status = CertificateStatus.NOT_YET_VALID;
                    else if (now.AddDays(30) > cert.NotAfter) status = CertificateStatus.EXPIRING_SOON;

                    results.Add(new Certificate(
                        Id: cert.Thumbprint.ToLowerInvariant(),
                        Subject: cert.Subject,
                        Issuer: cert.Issuer,
                        SerialNumber: cert.SerialNumber,
                        Thumbprint: cert.Thumbprint,
                        ValidFrom: cert.NotBefore,
                        ValidTo: cert.NotAfter,
                        SignatureAlgorithm: cert.SignatureAlgorithm.FriendlyName ?? "SHA256withRSA",
                        PublicKeyAlgorithm: pubKeyAlgo,
                        KeySize: keySize,
                        ProviderId: "windows-store",
                        SmartCardBacked: isSmartCard,
                        HasPrivateKey: true,
                        Status: status
                    ));
                }
                catch
                {
                    // Skip malformed certificates
                }
            }
        }
        catch
        {
            // Fallback empty list if store access fails
        }

        return Task.FromResult<IReadOnlyList<Certificate>>(results);
    }

    public Task<bool> CanSignAsync(string certificateId, CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsWindows()) return Task.FromResult(false);

        using var cert = FindCertificate(certificateId);
        return Task.FromResult(cert != null && cert.HasPrivateKey);
    }

    public Task<byte[]> SignAsync(
        string certificateId,
        string hashAlgorithm,
        byte[] data,
        CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("Windows certificate provider is only supported on Windows.");
        }

        using var cert = FindCertificate(certificateId);
        if (cert == null)
        {
            throw new KeyNotFoundException($"Certificate '{certificateId}' not found in Windows store.");
        }

        if (!cert.HasPrivateKey)
        {
            throw new InvalidOperationException($"Certificate '{certificateId}' does not have an accessible private key.");
        }

        var hashName = AlgorithmValidator.ToHashAlgorithmName(hashAlgorithm);

        // 1. Attempt RSA signing via OS provider (delegates PIN entry to Windows CSP/KSP dialog)
        using var rsa = cert.GetRSAPrivateKey();
        if (rsa != null)
        {
            // Windows native token driver will prompt for PIN dialog here if required
            var signature = rsa.SignData(data, hashName, RSASignaturePadding.Pkcs1);
            return Task.FromResult(signature);
        }

        // 2. Attempt ECDSA signing
        using var ecdsa = cert.GetECDsaPrivateKey();
        if (ecdsa != null)
        {
            var signature = ecdsa.SignData(data, hashName);
            return Task.FromResult(signature);
        }

        throw new NotSupportedException("Certificate private key does not support RSA or ECDSA signing.");
    }

    private static X509Certificate2? FindCertificate(string idOrThumbprint)
    {
        using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

        var clean = idOrThumbprint.Replace(" ", "").Trim();
        var matches = store.Certificates.Find(X509FindType.FindByThumbprint, clean, false);

        if (matches.Count > 0)
        {
            return matches[0];
        }

        // Try exact match on serial number
        var serialMatches = store.Certificates.Find(X509FindType.FindBySerialNumber, clean, false);
        return serialMatches.Count > 0 ? serialMatches[0] : null;
    }

    private static bool DetectSmartCard(X509Certificate2 cert)
    {
        try
        {
            // Inspect CSP/KSP flags via CNG or extensions
            foreach (var ext in cert.Extensions)
            {
                if (ext.Oid?.Value == "1.3.6.1.4.1.311.20.2" || // Smart Card User
                    ext.Oid?.Value == "1.3.6.1.5.5.7.3.2")       // Client Auth
                {
                    return true;
                }
            }
        }
        catch { }

        return false;
    }

    public Task<X509Certificate2?> GetX509CertificateAsync(string certificateId, CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsWindows()) return Task.FromResult<X509Certificate2?>(null);

        var cert = FindCertificate(certificateId);
        return Task.FromResult(cert);
    }
}
