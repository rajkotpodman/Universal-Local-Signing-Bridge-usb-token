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
