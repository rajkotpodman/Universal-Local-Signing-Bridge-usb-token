using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Bridge.Core.Interfaces;
using Bridge.Core.Models;
using Bridge.Security;

namespace Bridge.Provider.Mock;

public class MockSigningProvider : ISigningProvider, IDisposable
{
    private readonly RSA _rsa2048;
    private readonly RSA _rsa4096;
    private readonly ECDsa _ecdsa256;
    private readonly List<Certificate> _certificates = new();
    private readonly Dictionary<string, object> _keyMap = new(StringComparer.OrdinalIgnoreCase);

    public MockSigningProvider()
    {
        _rsa2048 = RSA.Create(2048);
        _rsa4096 = RSA.Create(4096);
        _ecdsa256 = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        InitializeMockCertificates();
    }

    private void InitializeMockCertificates()
    {
        // Cert 1: RSA 2048 Class 3 DSC
        var req1 = new CertificateRequest(
            "CN=JOHN DOE (Mock Class 3 DSC), O=Enterprise Demo CA, C=US",
            _rsa2048,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1
        );
        var cert1 = req1.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-30), DateTimeOffset.UtcNow.AddYears(1));
        var id1 = cert1.Thumbprint.ToLowerInvariant();
        _certificates.Add(new Certificate(
            Id: id1,
            Subject: cert1.Subject,
            Issuer: cert1.Issuer,
            SerialNumber: cert1.SerialNumber,
            Thumbprint: cert1.Thumbprint,
            ValidFrom: cert1.NotBefore,
            ValidTo: cert1.NotAfter,
            SignatureAlgorithm: "SHA256withRSA",
            PublicKeyAlgorithm: "RSA",
            KeySize: 2048,
            ProviderId: "mock-provider",
            SmartCardBacked: true,
            HasPrivateKey: true,
            Status: CertificateStatus.VALID
        ));
        _keyMap[id1] = _rsa2048;

        // Cert 2: RSA 4096 Corporate Signer
        var req2 = new CertificateRequest(
            "CN=ACME CORP (Mock Corporate DSC), O=Acme Trust Services, C=US",
            _rsa4096,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1
        );
        var cert2 = req2.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-60), DateTimeOffset.UtcNow.AddYears(2));
        var id2 = cert2.Thumbprint.ToLowerInvariant();
        _certificates.Add(new Certificate(
            Id: id2,
            Subject: cert2.Subject,
            Issuer: cert2.Issuer,
            SerialNumber: cert2.SerialNumber,
            Thumbprint: cert2.Thumbprint,
            ValidFrom: cert2.NotBefore,
            ValidTo: cert2.NotAfter,
            SignatureAlgorithm: "SHA256withRSA",
            PublicKeyAlgorithm: "RSA",
            KeySize: 4096,
            ProviderId: "mock-provider",
            SmartCardBacked: true,
            HasPrivateKey: true,
            Status: CertificateStatus.VALID
        ));
        _keyMap[id2] = _rsa4096;

        // Cert 3: ECDSA P-256 Citizen ID
        var req3 = new CertificateRequest(
            "CN=ALICE SMITH (Mock Citizen ECDSA), O=National eID Authority, C=US",
            _ecdsa256,
            HashAlgorithmName.SHA256
        );
        var cert3 = req3.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-10), DateTimeOffset.UtcNow.AddYears(1));
        var id3 = cert3.Thumbprint.ToLowerInvariant();
        _certificates.Add(new Certificate(
            Id: id3,
            Subject: cert3.Subject,
            Issuer: cert3.Issuer,
            SerialNumber: cert3.SerialNumber,
            Thumbprint: cert3.Thumbprint,
            ValidFrom: cert3.NotBefore,
            ValidTo: cert3.NotAfter,
            SignatureAlgorithm: "SHA256withECDSA",
            PublicKeyAlgorithm: "ECDSA",
            KeySize: 256,
            ProviderId: "mock-provider",
            SmartCardBacked: true,
            HasPrivateKey: true,
            Status: CertificateStatus.VALID
        ));
        _keyMap[id3] = _ecdsa256;
    }

    public ProviderInfo GetProviderInfo()
    {
        return new ProviderInfo(
            Id: "mock-provider",
            Name: "Development Mock Provider",
            Type: "Simulated PKI",
            Description: "In-memory software simulation for local development and CI testing. Mock signatures are NOT legally binding.",
            Status: ProviderStatus.MOCK,
            IsHardwareBacked: false,
            SupportedAlgorithms: new[] { "SHA-256", "SHA-384", "SHA-512" },
            Details: "Active in-memory RSA 2048/4096 and ECDSA P-256 keys. No physical token required.",
            CanEnumerateTokens: true
        );
    }

    public Task<IReadOnlyList<Certificate>> GetCertificatesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<Certificate>>(_certificates);
    }

    public Task<bool> CanSignAsync(string certificateId, CancellationToken cancellationToken = default)
    {
        var can = _keyMap.ContainsKey(certificateId);
        return Task.FromResult(can);
    }

    public Task<byte[]> SignAsync(
        string certificateId,
        string hashAlgorithm,
        byte[] data,
        CancellationToken cancellationToken = default)
    {
        if (!_keyMap.TryGetValue(certificateId, out var keyObj))
        {
            throw new KeyNotFoundException($"Certificate '{certificateId}' not found in mock provider.");
        }

        var hashName = AlgorithmValidator.ToHashAlgorithmName(hashAlgorithm);

        if (keyObj is RSA rsa)
        {
            var sig = rsa.SignData(data, hashName, RSASignaturePadding.Pkcs1);
            return Task.FromResult(sig);
        }
        else if (keyObj is ECDsa ecdsa)
        {
            var sig = ecdsa.SignData(data, hashName);
            return Task.FromResult(sig);
        }

        throw new InvalidOperationException("Unsupported cryptographic key type.");
    }

    public void Dispose()
    {
        _rsa2048.Dispose();
        _rsa4096.Dispose();
        _ecdsa256.Dispose();
    }
}
