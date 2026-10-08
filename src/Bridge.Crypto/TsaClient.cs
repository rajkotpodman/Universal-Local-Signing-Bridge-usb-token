using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Bridge.Crypto;

public record TsaResponseResult(
    bool Success,
    string? TimestampTokenBase64,
    string ServerUrl,
    string Status,
    string? ErrorDetails
);

public static class TsaClient
{
    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(8) };

    public static async Task<TsaResponseResult> RequestTimestampAsync(
        byte[] dataDigest,
        string tsaUrl = "http://timestamp.digicert.com",
        CancellationToken ct = default)
    {
        try
        {
            // Build RFC 3161 TimeStampReq
            byte[] tsReq = BuildTimeStampRequest(dataDigest);

            var content = new ByteArrayContent(tsReq);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/timestamp-query");

            var response = await HttpClient.PostAsync(tsaUrl, content, ct);
            if (!response.IsSuccessStatusCode)
            {
                return new TsaResponseResult(
                    Success: false,
                    TimestampTokenBase64: null,
                    ServerUrl: tsaUrl,
                    Status: "HTTP_ERROR",
                    ErrorDetails: $"TSA server returned HTTP {(int)response.StatusCode}"
                );
            }

            byte[] tsResp = await response.Content.ReadAsByteArrayAsync(ct);
            return new TsaResponseResult(
                Success: true,
                TimestampTokenBase64: Convert.ToBase64String(tsResp),
                ServerUrl: tsaUrl,
                Status: "TIMESTAMPED",
                ErrorDetails: null
            );
        }
        catch (Exception ex)
        {
            return new TsaResponseResult(
                Success: false,
                TimestampTokenBase64: null,
                ServerUrl: tsaUrl,
                Status: "FAILED",
                ErrorDetails: ex.Message
            );
        }
    }

    private static byte[] BuildTimeStampRequest(byte[] hash)
    {
        // Minimal ASN.1 DER TimeStampReq for SHA-256
        // SEQUENCE { version INTEGER 1, messageImprint { algorithm { 2.16.840.1.101.3.4.2.1 }, hashedMessage hash }, certReq BOOLEAN true }
        byte[] oidSha256 = new byte[] { 0x06, 0x09, 0x60, 0x86, 0x48, 0x01, 0x65, 0x03, 0x04, 0x02, 0x01 };

        using var ms = new MemoryStream();
        ms.WriteByte(0x30); // SEQUENCE (AlgorithmIdentifier)
        ms.WriteByte((byte)oidSha256.Length);
        ms.Write(oidSha256);

        byte[] algId = ms.ToArray();

        using var mi = new MemoryStream();
        mi.Write(algId);
        mi.WriteByte(0x04); // OCTET STRING (hash)
        mi.WriteByte((byte)hash.Length);
        mi.Write(hash);

        byte[] messageImprint = mi.ToArray();

        using var req = new MemoryStream();
        // Version 1
        req.Write(new byte[] { 0x02, 0x01, 0x01 });

        // MessageImprint
        req.WriteByte(0x30);
        req.WriteByte((byte)messageImprint.Length);
        req.Write(messageImprint);

        // certReq TRUE
        req.Write(new byte[] { 0x01, 0x01, 0xFF });

        byte[] reqBody = req.ToArray();

        using var finalDer = new MemoryStream();
        finalDer.WriteByte(0x30); // SEQUENCE
        finalDer.WriteByte((byte)reqBody.Length);
        finalDer.Write(reqBody);

        return finalDer.ToArray();
    }
}
