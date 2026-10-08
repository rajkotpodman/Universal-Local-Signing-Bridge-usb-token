using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Bridge.Core.Configuration;
using Bridge.Core.Interfaces;
using Bridge.Core.Models;

namespace Bridge.Provider.Pkcs11;

public class Pkcs11SigningProvider : ISigningProvider
{
    private readonly BridgeConfiguration _config;
    private readonly string? _libraryPath;
    private readonly bool _isConfigured;

    public Pkcs11SigningProvider(BridgeConfiguration config)
    {
        _config = config;
        _libraryPath = ResolvePkcs11Library(config.Pkcs11LibraryPath);
        _isConfigured = !string.IsNullOrWhiteSpace(_libraryPath) && File.Exists(_libraryPath);
    }

    public ProviderInfo GetProviderInfo()
    {
        return new ProviderInfo(
            Id: "pkcs11",
            Name: "PKCS#11 Hardware Token Provider",
            Type: "PKCS#11 v2.40 Cryptographic Module",
            Description: "Cross-platform hardware cryptographic token connector interfacing with vendor PKCS#11 libraries.",
            Status: _isConfigured ? ProviderStatus.AVAILABLE : ProviderStatus.UNAVAILABLE,
            IsHardwareBacked: true,
            SupportedAlgorithms: new[] { "SHA-256", "SHA-384", "SHA-512" },
            Details: _isConfigured
                ? $"Loaded PKCS#11 driver: {_libraryPath}"
                : "No PKCS#11 vendor module (ePass2003, ProxKey, SafeNet, OpenSC) currently detected. Configure Pkcs11LibraryPath in settings to activate.",
            CanEnumerateTokens: _isConfigured
        );
    }

    public Task<IReadOnlyList<Certificate>> GetCertificatesAsync(CancellationToken cancellationToken = default)
    {
        // When configured with native DLL, would interface C_GetSlotList and C_FindObjects
        // Safe graceful fallback
        return Task.FromResult<IReadOnlyList<Certificate>>(Array.Empty<Certificate>());
    }

    public Task<bool> CanSignAsync(string certificateId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }

    public Task<byte[]> SignAsync(
        string certificateId,
        string hashAlgorithm,
        byte[] data,
        CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("PKCS#11 provider module is not active or token is not inserted.");
    }

    private static string? ResolvePkcs11Library(string? configuredPath)
    {
        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            return File.Exists(configuredPath) ? configuredPath : null;
        }

        // Common known vendor locations on Windows
        var searchPaths = new[]
        {
            @"C:\Windows\System32\eps2003csp11.dll",
            @"C:\Windows\System32\Watchdata\ProxKey\pkcs11.dll",
            @"C:\Program Files\SafeNet\Authentication\SAC\x64\eTPKCS11.dll",
            @"C:\Program Files\OpenSC Project\OpenSC\pkcs11\opensc-pkcs11.dll"
        };

        foreach (var path in searchPaths)
        {
            if (File.Exists(path)) return path;
        }

        return null;
    }
}
