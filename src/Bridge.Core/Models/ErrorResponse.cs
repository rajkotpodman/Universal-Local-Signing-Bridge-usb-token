namespace Bridge.Core.Models;

public record ErrorDetail(
    string Code,
    string Message,
    string? RequestId = null,
    string? Details = null
);

public record ApiErrorResponse(
    ErrorDetail Error
);

public static class ErrorCodes
{
    public const string CertificateNotFound = "CERTIFICATE_NOT_FOUND";
    public const string InvalidSession = "INVALID_SESSION";
    public const string SessionExpired = "SESSION_EXPIRED";
    public const string InvalidOrigin = "INVALID_ORIGIN";
    public const string RemoteIpForbidden = "REMOTE_IP_FORBIDDEN";
    public const string UnsupportedAlgorithm = "UNSUPPORTED_ALGORITHM";
    public const string WeakAlgorithmRejected = "WEAK_ALGORITHM_REJECTED";
    public const string OversizedPayload = "OVERSIZED_PAYLOAD";
    public const string RateLimitExceeded = "RATE_LIMIT_EXCEEDED";
    public const string ProviderUnavailable = "PROVIDER_UNAVAILABLE";
    public const string SigningFailed = "SIGNING_FAILED";
    public const string NoPrivateKey = "NO_PRIVATE_KEY";
    public const string MalformedRequest = "MALFORMED_REQUEST";
}
