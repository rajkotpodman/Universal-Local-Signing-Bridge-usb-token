using System;

namespace Bridge.Core.Models;

public record AuditEntry(
    string Id,
    DateTimeOffset Timestamp,
    string? SessionIdHash,
    string? CertificateId,
    string Provider,
    string Operation,
    bool Success,
    string? ErrorCode,
    double DurationMs,
    string? ClientIp,
    string? Origin,
    string? PayloadDigestSha256,
    string? Details = null
);
