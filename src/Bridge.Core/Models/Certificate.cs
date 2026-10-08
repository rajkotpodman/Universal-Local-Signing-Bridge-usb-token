using System;

namespace Bridge.Core.Models;

public enum CertificateStatus
{
    VALID,
    EXPIRING_SOON,
    EXPIRED,
    NOT_YET_VALID,
    NO_PRIVATE_KEY,
    REVOKED
}

public record Certificate(
    string Id,
    string Subject,
    string Issuer,
    string SerialNumber,
    string Thumbprint,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidTo,
    string SignatureAlgorithm,
    string PublicKeyAlgorithm,
    int KeySize,
    string ProviderId,
    bool SmartCardBacked,
    bool HasPrivateKey,
    CertificateStatus Status
)
{
    public bool IsExpired => DateTimeOffset.UtcNow > ValidTo;
    public bool IsExpiringSoon => !IsExpired && DateTimeOffset.UtcNow.AddDays(30) > ValidTo;
}
