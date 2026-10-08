using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using Bridge.Crypto;
using Bridge.Providers;
using Xunit;

namespace Bridge.Core.Tests;

public class AdvancedSigningTests : IDisposable
{
    private readonly RSA _rsa;
    private readonly X509Certificate2 _testCert;

    public AdvancedSigningTests()
    {
        _rsa = RSA.Create(2048);
        var req = new CertificateRequest(
            "CN=Test Bridge Signer, O=Universal Local Bridge, C=IN",
            _rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1
        );
        _testCert = req.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));
    }

    [Fact]
    public void PdfSignerEngine_SignsDocument_Successfully()
    {
        var samplePdf = PdfSignerEngine.GenerateBasePdfDocument("Automated Test PDF for PAdES-BES Signing");
        Assert.NotNull(samplePdf);
        Assert.True(samplePdf.Length > 0);

        var visual = new VisualSignatureOptions(
            Enabled: true,
            PageNumber: 1,
            X: 300,
            Y: 100,
            Width: 200,
            Height: 80,
            SignerName: "Test Signer",
            Reason: "Automated Unit Test",
            Location: "Localhost",
            ContactInfo: "test@bridge.local"
        );

        var result = PdfSignerEngine.SignPdf(samplePdf, _testCert, visual);

        Assert.True(result.Success);
        Assert.NotNull(result.SignedPdfBytes);
        Assert.True(result.SignedPdfBytes.Length > samplePdf.Length);
        Assert.Equal("PAdES-BES (ISO 32000-1 / ETSI TS 102 778)", result.SignatureFormat);
        Assert.NotEmpty(result.DocumentHashSha256);
        Assert.Contains("Test Bridge Signer", result.SignerSubject);

        // Verify PDF signature structure
        string pdfStr = Encoding.ASCII.GetString(result.SignedPdfBytes);
        Assert.Contains("/ByteRange", pdfStr);
        Assert.Contains("/SubFilter /adbe.pkcs7.detached", pdfStr);
        Assert.Contains("/Type /Sig", pdfStr);
    }

    [Fact]
    public void CadesSignerEngine_SignsDetached_ProducesValidCms()
    {
        byte[] payload = Encoding.UTF8.GetBytes("Critical Enterprise Contract Data 2026");

        var result = CadesSignerEngine.SignDetached(payload, _testCert, HashAlgorithmName.SHA256, detached: true);

        Assert.True(result.Success);
        Assert.NotEmpty(result.SignatureBase64);
        Assert.Equal("PKCS7-Detached (CAdES-BES)", result.SignatureFormat);
        Assert.Equal("SHA256", result.DigestAlgorithm);
        Assert.Equal(payload.Length, result.SignedContentBytes);

        // Decode CMS
        byte[] cmsBytes = Convert.FromBase64String(result.SignatureBase64);
        Assert.True(cmsBytes.Length > 0);
    }

    [Fact]
    public void XadesSignerEngine_SignsEnvelopedXml_ProducesValidSignatureXml()
    {
        string xml = "<Invoice id=\"INV-2026-001\"><Amount currency=\"INR\">150000.00</Amount><Buyer>Acme India Corp</Buyer></Invoice>";

        var result = XadesSignerEngine.SignEnveloped(xml, _testCert);

        Assert.True(result.Success);
        Assert.NotEmpty(result.SignedXml);
        Assert.NotEmpty(result.DigestValueBase64);
        Assert.NotEmpty(result.SignatureValueBase64);
        Assert.Equal("XAdES-BES (ETSI TS 101 903 / W3C XMLDSIG)", result.SignatureFormat);

        // Validate XML document syntax
        var doc = new XmlDocument();
        doc.LoadXml(result.SignedXml);

        var sigElem = doc.GetElementsByTagName("Signature");
        Assert.True(sigElem.Count > 0, "XML must contain <Signature> element");

        var digestElem = doc.GetElementsByTagName("DigestValue");
        Assert.True(digestElem.Count > 0, "XML must contain <DigestValue> element");

        var sigValueElem = doc.GetElementsByTagName("SignatureValue");
        Assert.True(sigValueElem.Count > 0, "XML must contain <SignatureValue> element");
    }

    [Fact]
    public void PcscHardwareMonitor_ScanReaders_ExecutesWithoutCrashing()
    {
        var readers = PcscHardwareMonitor.ScanReaders();
        Assert.NotNull(readers);
        // Even if 0 readers are attached on VM/headless host, the call must return cleanly
    }

    public void Dispose()
    {
        _testCert.Dispose();
        _rsa.Dispose();
    }
}
