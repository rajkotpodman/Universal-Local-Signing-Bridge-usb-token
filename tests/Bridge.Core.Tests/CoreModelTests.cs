using System;
using Bridge.Core.Models;
using Bridge.Crypto;
using Xunit;

namespace Bridge.Core.Tests;

public class CoreModelTests
{
    [Fact]
    public void Certificate_ExpirationFlags_WorkCorrectly()
    {
        var expiredCert = new Certificate(
            Id: "1",
            Subject: "CN=Test",
            Issuer: "CN=Issuer",
            SerialNumber: "123",
            Thumbprint: "ABC",
            ValidFrom: DateTimeOffset.UtcNow.AddYears(-2),
            ValidTo: DateTimeOffset.UtcNow.AddDays(-1),
            SignatureAlgorithm: "SHA256withRSA",
            PublicKeyAlgorithm: "RSA",
            KeySize: 2048,
            ProviderId: "mock",
            SmartCardBacked: true,
            HasPrivateKey: true,
            Status: CertificateStatus.EXPIRED
        );

        Assert.True(expiredCert.IsExpired);

        var validCert = new Certificate(
            Id: "2",
            Subject: "CN=Test2",
            Issuer: "CN=Issuer",
            SerialNumber: "124",
            Thumbprint: "DEF",
            ValidFrom: DateTimeOffset.UtcNow.AddDays(-10),
            ValidTo: DateTimeOffset.UtcNow.AddYears(1),
            SignatureAlgorithm: "SHA256withRSA",
            PublicKeyAlgorithm: "RSA",
            KeySize: 2048,
            ProviderId: "mock",
            SmartCardBacked: true,
            HasPrivateKey: true,
            Status: CertificateStatus.VALID
        );

        Assert.False(validCert.IsExpired);
    }

    [Fact]
    public void HashUtility_ComputesDeterministicSha256Hex()
    {
        var text = "Hello Universal Signing Bridge";
        var hash1 = HashUtility.ComputeSha256Hex(text);
        var hash2 = HashUtility.ComputeSha256Hex(text);

        Assert.Equal(hash1, hash2);
        Assert.Equal(64, hash1.Length);
    }

    [Fact]
    public void Session_Expiration_CalculatesAccurately()
    {
        var activeSession = new Session("s1", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1), "http://localhost", "app");
        Assert.False(activeSession.IsExpired);

        var expiredSession = new Session("s2", DateTimeOffset.UtcNow.AddHours(-2), DateTimeOffset.UtcNow.AddHours(-1), "http://localhost", "app");
        Assert.True(expiredSession.IsExpired);
    }
}
