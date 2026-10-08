using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge.Core.Configuration;
using Bridge.Core.Interfaces;
using Bridge.Core.Models;

namespace Bridge.Providers;

public class ProviderManager
{
    private readonly List<ISigningProvider> _providers = new();
    private readonly BridgeConfiguration _config;
    private ISigningProvider? _activeProvider;

    public ProviderManager(BridgeConfiguration config)
    {
        _config = config;
    }

    public void RegisterProvider(ISigningProvider provider)
    {
        _providers.Add(provider);
        UpdateActiveProvider();
    }

    public void RegisterProviders(IEnumerable<ISigningProvider> providers)
    {
        _providers.AddRange(providers);
        UpdateActiveProvider();
    }

    public IReadOnlyList<ProviderInfo> GetAllProvidersInfo()
    {
        return _providers.Select(p => p.GetProviderInfo()).ToList();
    }

    public ISigningProvider GetActiveProvider()
    {
        if (_activeProvider == null)
        {
            UpdateActiveProvider();
        }

        return _activeProvider ?? throw new InvalidOperationException("No suitable signing provider is available.");
    }

    public void SetActiveProvider(string providerId)
    {
        var target = _providers.FirstOrDefault(p =>
            string.Equals(p.GetProviderInfo().Id, providerId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(p.GetProviderInfo().Name, providerId, StringComparison.OrdinalIgnoreCase)
        );

        if (target != null)
        {
            _activeProvider = target;
        }
        else
        {
            throw new ArgumentException($"Provider with ID '{providerId}' not found.");
        }
    }

    public async Task<IReadOnlyList<Certificate>> GetAllCertificatesAsync(CancellationToken ct = default)
    {
        var allCerts = new List<Certificate>();
        foreach (var provider in _providers)
        {
            try
            {
                var info = provider.GetProviderInfo();
                if (info.Status == ProviderStatus.AVAILABLE || info.Status == ProviderStatus.MOCK)
                {
                    var certs = await provider.GetCertificatesAsync(ct);
                    allCerts.AddRange(certs);
                }
            }
            catch
            {
                // Soft skip failing providers during enumeration
            }
        }

        return allCerts;
    }

    public async Task<(ISigningProvider Provider, Certificate Certificate)> ResolveCertificateAsync(
        string certificateId,
        CancellationToken ct = default)
    {
        foreach (var provider in _providers)
        {
            try
            {
                var certs = await provider.GetCertificatesAsync(ct);
                var match = certs.FirstOrDefault(c =>
                    string.Equals(c.Id, certificateId, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c.Thumbprint, certificateId, StringComparison.OrdinalIgnoreCase)
                );

                if (match != null)
                {
                    return (provider, match);
                }
            }
            catch
            {
                // Continue searching other providers
            }
        }

        throw new KeyNotFoundException($"Certificate '{certificateId}' not found across any registered provider.");
    }

    private void UpdateActiveProvider()
    {
        // 1. If MockMode is forced or requested
        if (_config.MockMode || _config.ProviderMode.Equals("MOCK", StringComparison.OrdinalIgnoreCase))
        {
            var mock = _providers.FirstOrDefault(p => p.GetProviderInfo().Status == ProviderStatus.MOCK);
            if (mock != null)
            {
                _activeProvider = mock;
                return;
            }
        }

        // 2. If WINDOWS mode requested
        if (_config.ProviderMode.Equals("WINDOWS", StringComparison.OrdinalIgnoreCase))
        {
            var win = _providers.FirstOrDefault(p => p.GetProviderInfo().Id == "windows-store");
            if (win != null && win.GetProviderInfo().Status == ProviderStatus.AVAILABLE)
            {
                _activeProvider = win;
                return;
            }
        }

        // 3. If PKCS11 mode requested
        if (_config.ProviderMode.Equals("PKCS11", StringComparison.OrdinalIgnoreCase))
        {
            var p11 = _providers.FirstOrDefault(p => p.GetProviderInfo().Id == "pkcs11");
            if (p11 != null && p11.GetProviderInfo().Status == ProviderStatus.AVAILABLE)
            {
                _activeProvider = p11;
                return;
            }
        }

        // Fallback to first available provider
        _activeProvider = _providers.FirstOrDefault(p => p.GetProviderInfo().Status == ProviderStatus.AVAILABLE)
                          ?? _providers.FirstOrDefault();
    }
}
