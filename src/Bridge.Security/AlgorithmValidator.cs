using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Bridge.Security;

public static class AlgorithmValidator
{
    private static readonly HashSet<string> AllowedAlgorithms = new(StringComparer.OrdinalIgnoreCase)
    {
        "SHA256",
        "SHA-256",
        "SHA384",
        "SHA-384",
        "SHA512",
        "SHA-512"
    };

    private static readonly HashSet<string> WeakAlgorithms = new(StringComparer.OrdinalIgnoreCase)
    {
        "MD5",
        "SHA1",
        "SHA-1",
        "MD2",
        "MD4"
    };

    public static bool IsAllowed(string algorithm)
    {
        if (string.IsNullOrWhiteSpace(algorithm)) return false;
        return AllowedAlgorithms.Contains(algorithm.Trim());
    }

    public static bool IsExplicitlyWeak(string algorithm)
    {
        if (string.IsNullOrWhiteSpace(algorithm)) return false;
        return WeakAlgorithms.Contains(algorithm.Trim());
    }

    public static HashAlgorithmName ToHashAlgorithmName(string algorithm)
    {
        var norm = algorithm.Replace("-", "").ToUpperInvariant();
        return norm switch
        {
            "SHA256" => HashAlgorithmName.SHA256,
            "SHA384" => HashAlgorithmName.SHA384,
            "SHA512" => HashAlgorithmName.SHA512,
            _ => throw new ArgumentException($"Unsupported algorithm: {algorithm}")
        };
    }
}
