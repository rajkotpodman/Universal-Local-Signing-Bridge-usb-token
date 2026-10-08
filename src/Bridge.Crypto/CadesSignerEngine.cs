using System;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using Bridge.Core.Models;

namespace Bridge.Crypto;

public record CadesSignatureResult(
    bool Success,
    string SignatureBase64,
    string SignatureFormat, // "CAdES-BES", "PKCS7-Detached"
    string DigestAlgorithm,
    int SignedContentBytes,
    string SignerSubject,
    string SignerThumbprint,
    string Timestamp
);

public static class CadesSignerEngine
{
    public static CadesSignatureResult SignDetached(
        byte[] contentToSign,
        X509Certificate2 signingCertificate,
        HashAlgorithmName hashAlgorithm,
        bool detached = true)
    {
        var contentInfo = new ContentInfo(contentToSign);
        var signedCms = new SignedCms(contentInfo, detached);

        var signer = new CmsSigner(signingCertificate)
        {
            DigestAlgorithm = new Oid(GetOidForHash(hashAlgorithm)),
            IncludeOption = X509IncludeOption.EndCertOnly
        };

        // Add signing time attribute
        signer.SignedAttributes.Add(new Pkcs9SigningTime(DateTime.UtcNow));

        signedCms.ComputeSignature(signer, silent: false);
        byte[] encoded = signedCms.Encode();

        return new CadesSignatureResult(
            Success: true,
            SignatureBase64: Convert.ToBase64String(encoded),
            SignatureFormat: detached ? "PKCS7-Detached (CAdES-BES)" : "PKCS7-Enveloped",
            DigestAlgorithm: hashAlgorithm.Name ?? "SHA256",
            SignedContentBytes: contentToSign.Length,
            SignerSubject: signingCertificate.Subject,
            SignerThumbprint: signingCertificate.Thumbprint,
            Timestamp: DateTimeOffset.UtcNow.ToString("O")
        );
    }

    private static string GetOidForHash(HashAlgorithmName name)
    {
        if (name == HashAlgorithmName.SHA256) return "2.16.840.1.101.3.4.2.1";
        if (name == HashAlgorithmName.SHA384) return "2.16.840.1.101.3.4.2.2";
        if (name == HashAlgorithmName.SHA512) return "2.16.840.1.101.3.4.2.3";
        return "2.16.840.1.101.3.4.2.1";
    }
}
