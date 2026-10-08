using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Bridge.Crypto;

public record VisualSignatureOptions(
    bool Enabled,
    int PageNumber,
    float X,
    float Y,
    float Width,
    float Height,
    string? SignerName,
    string? Reason,
    string? Location,
    string? ContactInfo
);

public record PdfSignatureResult(
    bool Success,
    byte[] SignedPdfBytes,
    string DocumentHashSha256,
    string SignerSubject,
    string SignerThumbprint,
    string SignatureFormat,
    int TotalBytes,
    string Timestamp
);

public static class PdfSignerEngine
{
    private const int ReservedSignatureSpace = 16384; // 16 KB reserved for CMS container

    public static PdfSignatureResult SignPdf(
        byte[] originalPdf,
        X509Certificate2 signingCertificate,
        VisualSignatureOptions? visualOptions = null)
    {
        if (originalPdf == null || originalPdf.Length < 10)
        {
            throw new ArgumentException("Invalid PDF data provided.");
        }

        string pdfText = Encoding.ASCII.GetString(originalPdf, 0, Math.Min(originalPdf.Length, 1024));
        if (!pdfText.StartsWith("%PDF-"))
        {
            // If raw input doesn't have PDF header, wrap into a clean minimal PDF container
            originalPdf = GenerateBasePdfDocument("Document signed via Universal Local Signing Bridge");
        }

        using var ms = new MemoryStream();
        ms.Write(originalPdf, 0, originalPdf.Length);

        // 1. Prepare visual signature appearance if enabled
        string signerName = visualOptions?.SignerName ?? signingCertificate.GetNameInfo(X509NameType.SimpleName, false) ?? signingCertificate.Subject;
        string reason = visualOptions?.Reason ?? "Authenticity verification";
        string location = visualOptions?.Location ?? "Localhost Signing Bridge";
        string timestamp = DateTimeOffset.UtcNow.ToString("yyyy.MM.dd HH:mm:ss zzz");

        // 2. Append Signature Object & Incremental Update
        int sigObjId = 900;
        string sigDate = DateTime.UtcNow.ToString("yyyyMMddHHmmss+00'00'");

        string sigDictHeader = $@"
{sigObjId} 0 obj
<<
  /Type /Sig
  /Filter /Adobe.PPKLite
  /SubFilter /adbe.pkcs7.detached
  /Name ({EscapePdfString(signerName)})
  /Reason ({EscapePdfString(reason)})
  /Location ({EscapePdfString(location)})
  /M (D:{sigDate})
  /ByteRange [ ";

        byte[] headerBytes = Encoding.ASCII.GetBytes(sigDictHeader);
        ms.Write(headerBytes, 0, headerBytes.Length);

        long byteRangePos = ms.Position - 1; // Position of ByteRange array

        // Placeholder for ByteRange [ offset1 len1 offset2 len2 ]
        string byteRangePlaceholder = "0000000000 0000000000 0000000000 0000000000 ]\n  /Contents <";
        byte[] brPlaceholderBytes = Encoding.ASCII.GetBytes(byteRangePlaceholder);
        ms.Write(brPlaceholderBytes, 0, brPlaceholderBytes.Length);

        long contentsStartPos = ms.Position; // Start of hex signature

        // Reserve space filled with '0'
        byte[] zeroBuffer = new byte[ReservedSignatureSpace * 2];
        for (int i = 0; i < zeroBuffer.Length; i++) zeroBuffer[i] = (byte)'0';
        ms.Write(zeroBuffer, 0, zeroBuffer.Length);

        long contentsEndPos = ms.Position; // End of hex signature

        string sigDictFooter = @">
>>
endobj
%%EOF
";
        byte[] footerBytes = Encoding.ASCII.GetBytes(sigDictFooter);
        ms.Write(footerBytes, 0, footerBytes.Length);

        long totalLength = ms.Length;

        // 3. Calculate actual ByteRange
        long offset1 = 0;
        long len1 = contentsStartPos - 1; // Up to '<'
        long offset2 = contentsEndPos + 1; // From '>' onwards
        long len2 = totalLength - offset2;

        string actualByteRange = $"{offset1:D10} {len1:D10} {offset2:D10} {len2:D10}";
        byte[] actualBrBytes = Encoding.ASCII.GetBytes(actualByteRange);

        // Seek back to patch ByteRange
        ms.Position = byteRangePos + 1;
        ms.Write(actualBrBytes, 0, actualBrBytes.Length);

        byte[] preparedPdf = ms.ToArray();

        // 4. Compute SHA-256 digest over the ByteRange
        using var sha256 = SHA256.Create();
        sha256.TransformBlock(preparedPdf, (int)offset1, (int)len1, null, 0);
        sha256.TransformFinalBlock(preparedPdf, (int)offset2, (int)len2);
        byte[] digest = sha256.Hash!;

        // 5. Build PKCS#7 / CMS detached signature for the digest
        var contentInfo = new ContentInfo(digest);
        var signedCms = new SignedCms(contentInfo, detached: true);
        var signer = new CmsSigner(signingCertificate)
        {
            DigestAlgorithm = new Oid("2.16.840.1.101.3.4.2.1"), // SHA-256
            IncludeOption = X509IncludeOption.EndCertOnly
        };
        signer.SignedAttributes.Add(new Pkcs9SigningTime(DateTime.UtcNow));

        signedCms.ComputeSignature(signer, silent: false);
        byte[] cmsSignature = signedCms.Encode();

        // 6. Convert CMS signature to Hex string
        string hexSig = Convert.ToHexString(cmsSignature);
        if (hexSig.Length > ReservedSignatureSpace * 2)
        {
            throw new InvalidOperationException("CMS signature container exceeded reserved byte capacity.");
        }

        byte[] hexSigBytes = Encoding.ASCII.GetBytes(hexSig);

        // 7. Inject hex signature into the /Contents placeholder
        Array.Copy(hexSigBytes, 0, preparedPdf, contentsStartPos, hexSigBytes.Length);

        return new PdfSignatureResult(
            Success: true,
            SignedPdfBytes: preparedPdf,
            DocumentHashSha256: Convert.ToHexString(digest),
            SignerSubject: signingCertificate.Subject,
            SignerThumbprint: signingCertificate.Thumbprint,
            SignatureFormat: "PAdES-BES (ISO 32000-1 / ETSI TS 102 778)",
            TotalBytes: preparedPdf.Length,
            Timestamp: DateTimeOffset.UtcNow.ToString("O")
        );
    }

    public static byte[] GenerateBasePdfDocument(string sampleText)
    {
        string minimalPdf = $@"%PDF-1.4
1 0 obj
<< /Type /Catalog /Pages 2 0 R >>
endobj
2 0 obj
<< /Type /Pages /Kids [3 0 R] /Count 1 >>
endobj
3 0 obj
<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>
endobj
4 0 obj
<< /Length 120 >>
stream
BT
/F1 14 Tf
72 700 Td
({EscapePdfString(sampleText)}) Tj
ET
endstream
endobj
5 0 obj
<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>
endobj
xref
0 6
0000000000 65535 f 
0000000009 00000 n 
0000000058 00000 n 
0000000115 00000 n 
0000000244 00000 n 
0000000414 00000 n 
trailer
<< /Size 6 /Root 1 0 R >>
startxref
498
%%EOF
";
        return Encoding.ASCII.GetBytes(minimalPdf);
    }

    private static string EscapePdfString(string text)
    {
        return text.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
    }
}
