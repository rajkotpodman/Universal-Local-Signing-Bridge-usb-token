using System;
using System.Collections.Generic;
using System.Linq;

namespace Bridge.Security;

public class OriginValidator
{
    private readonly HashSet<string> _allowedOrigins;

    public OriginValidator(IEnumerable<string> allowedOrigins)
    {
        _allowedOrigins = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var origin in allowedOrigins)
        {
            _allowedOrigins.Add(Normalize(origin));
        }
    }

    public bool IsAllowed(string? origin)
    {
        if (string.IsNullOrWhiteSpace(origin))
        {
            // Direct localhost tool or same-origin request
            return true;
        }

        var normalized = Normalize(origin);
        return _allowedOrigins.Contains(normalized);
    }

    public IReadOnlyCollection<string> GetAllowedOrigins() => _allowedOrigins.ToList();

    private static string Normalize(string url)
    {
        var trimmed = url.Trim().TrimEnd('/');
        return trimmed;
    }
}
