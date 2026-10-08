using System.Collections.Generic;

namespace Bridge.Core.Models;

public enum ProviderStatus
{
    AVAILABLE,
    UNAVAILABLE,
    ERROR,
    MOCK
}

public record ProviderInfo(
    string Id,
    string Name,
    string Type,
    string Description,
    ProviderStatus Status,
    bool IsHardwareBacked,
    IReadOnlyList<string> SupportedAlgorithms,
    string? Details = null,
    bool CanEnumerateTokens = true
);
