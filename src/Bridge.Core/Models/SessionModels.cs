using System;

namespace Bridge.Core.Models;

public record Session(
    string SessionId,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    string AllowedOrigin,
    string ClientAppId
)
{
    public bool IsExpired => DateTimeOffset.UtcNow > ExpiresAt;
}

public record SessionRequest(
    string? ClientAppId = "BrowserPortal",
    string? Origin = null
);

public record SessionResponse(
    bool Success,
    string SessionId,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    string AllowedOrigin
);
