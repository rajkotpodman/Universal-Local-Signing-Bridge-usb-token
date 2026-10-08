using System;
using System.Security.Cryptography;
using System.Text;

namespace Bridge.Crypto;

public static class HashUtility
{
    public static string ComputeSha256Hex(byte[] data)
    {
        var hash = SHA256.HashData(data);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public static string ComputeSha256Hex(string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        return ComputeSha256Hex(bytes);
    }

    public static byte[] HashData(byte[] data, HashAlgorithmName algorithm)
    {
        if (algorithm == HashAlgorithmName.SHA256)
            return SHA256.HashData(data);
        if (algorithm == HashAlgorithmName.SHA384)
            return SHA384.HashData(data);
        if (algorithm == HashAlgorithmName.SHA512)
            return SHA512.HashData(data);

        throw new NotSupportedException($"Hash algorithm {algorithm.Name} is not supported.");
    }
}
