using System;

namespace Bridge.Core.Models;

public record SignRequest(
    string CertificateId,
    string HashAlgorithm, // SHA-256, SHA-384, SHA-512
    string Data           // Base64-encoded raw bytes or document hash
);

public record SignCertificateSummary(
    string Subject,
    string Issuer,
    string SerialNumber,
    string Thumbprint
);

public record SignResponse(
    bool Success,
    string Signature, // Base64
    SignCertificateSummary Certificate,
    string Algorithm,
    string Timestamp,
    double DurationMs,
    string ProviderId,
    string? ErrorMessage = null
);

public record PdfSignRequest(
    string CertificateId,
    string? PdfBase64,
    bool CreateSampleIfEmpty = true,
    bool EnableVisualSignature = true,
    int PageNumber = 1,
    float X = 350f,
    float Y = 50f,
    float Width = 200f,
    float Height = 80f,
    string? Reason = "Digitally signed via Universal Signing Bridge",
    string? Location = "Secure Localhost Cryptographic Bridge",
    string? ContactInfo = null,
    string? TimestampUrl = null
);

public record PdfSignResponse(
    bool Success,
    string SignedPdfBase64,
    string DocumentHashSha256,
    string SignerSubject,
    string SignerThumbprint,
    string SignatureFormat,
    int TotalBytes,
    string Timestamp,
    double DurationMs,
    string ProviderId,
    string? TimestampTokenBase64 = null
);

public record CadesSignRequest(
    string CertificateId,
    string Data, // Base64
    string HashAlgorithm = "SHA-256",
    bool Detached = true
);

public record XadesSignRequest(
    string CertificateId,
    string XmlContent,
    string? SignatureType = "Enveloped"
);

public record TimestampRequest(
    string DataDigestBase64,
    string? TsaUrl = "http://timestamp.digicert.com"
);

public record BatchSignItem(
    string ItemId,
    string DataBase64,
    string Type = "raw" // "raw", "cades", "xml"
);

public record BatchSignItemResult(
    string ItemId,
    bool Success,
    string? SignatureBase64,
    string? ErrorMessage
);

public record BatchSignRequest(
    string CertificateId,
    string HashAlgorithm,
    System.Collections.Generic.List<BatchSignItem> Items
);

public record BatchSignResponse(
    bool Success,
    System.Collections.Generic.List<BatchSignItemResult> Results,
    int TotalProcessed,
    int SuccessCount,
    double DurationMs
);
