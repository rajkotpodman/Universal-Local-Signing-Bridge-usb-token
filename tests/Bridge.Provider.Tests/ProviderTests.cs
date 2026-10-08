using System;
using System.Text;
using System.Threading.Tasks;
using Bridge.Core.Configuration;
using Bridge.Core.Interfaces;
using Bridge.Core.Models;
using Bridge.Provider.Mock;
using Bridge.Provider.Pkcs11;
using Bridge.Provider.Windows;
using Bridge.Providers;
using Xunit;

namespace Bridge.Provider.Tests;

public class ProviderTests
{
    [Fact]
    public async Task MockProvider_GeneratesThreeCertificatesAndSignsData()
    {
        using var mockProvider = new MockSigningProvider();
        var certs = await mockProvider.GetCertificatesAsync();

        Assert.Equal(3, certs.Count);

        var firstCert = certs[0];
        Assert.True(firstCert.HasPrivateKey);
        Assert.True(firstCert.SmartCardBacked);

        var dataToSign = Encoding.UTF8.GetBytes("Data to authenticate");
        var signature = await mockProvider.SignAsync(firstCert.Id, "SHA-256", dataToSign);

        Assert.NotNull(signature);
        Assert.True(signature.Length > 0);
    }

    [Fact]
    public async Task ProviderManager_ResolvesCertificateAndActiveProvider()
    {
        var config = new BridgeConfiguration { MockMode = true, ProviderMode = "MOCK" };
        var manager = new ProviderManager(config);

        using var mockProvider = new MockSigningProvider();
        var winProvider = new WindowsCertificateProvider();
        var p11Provider = new Pkcs11SigningProvider(config);

        manager.RegisterProviders(new ISigningProvider[] { mockProvider, winProvider, p11Provider });

        var active = manager.GetActiveProvider();
        Assert.Equal("mock-provider", active.GetProviderInfo().Id);

        var allCerts = await manager.GetAllCertificatesAsync();
        Assert.True(allCerts.Count >= 3);

        var (prov, cert) = await manager.ResolveCertificateAsync(allCerts[0].Id);
        Assert.NotNull(prov);
        Assert.NotNull(cert);
    }

    [Fact]
    public async Task ProviderManager_ThrowsOnNonExistentCertificate()
    {
        var config = new BridgeConfiguration { MockMode = true };
        var manager = new ProviderManager(config);
        using var mockProvider = new MockSigningProvider();
        manager.RegisterProvider(mockProvider);

        await Assert.ThrowsAsync<System.Collections.Generic.KeyNotFoundException>(() =>
            manager.ResolveCertificateAsync("DOES_NOT_EXIST_ID_999")
        );
    }

    [Fact]
    public void Pkcs11Provider_ReportsCorrectStatusWithoutCrashing()
    {
        var config = new BridgeConfiguration { Pkcs11LibraryPath = "" };
        var p11 = new Pkcs11SigningProvider(config);
        var info = p11.GetProviderInfo();

        Assert.NotNull(info);
        Assert.Equal("pkcs11", info.Id);
        // On host with installed PKCS#11 driver (e.g. ePass2003) it is AVAILABLE; otherwise UNAVAILABLE
        Assert.True(info.Status == ProviderStatus.AVAILABLE || info.Status == ProviderStatus.UNAVAILABLE);
    }

    [Fact]
    public void Pkcs11Provider_WithInvalidPath_ReportsUnavailable()
    {
        var config = new BridgeConfiguration { Pkcs11LibraryPath = @"C:\non_existent_folder_xyz\dummy_p11.dll" };
        var p11 = new Pkcs11SigningProvider(config);
        var info = p11.GetProviderInfo();

        Assert.Equal(ProviderStatus.UNAVAILABLE, info.Status);
    }
}
