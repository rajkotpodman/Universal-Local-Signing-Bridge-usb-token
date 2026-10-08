using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;

namespace Bridge.Crypto;

public record XadesSignatureResult(
    bool Success,
    string SignedXml,
    string DigestValueBase64,
    string SignatureValueBase64,
    string SignerSubject,
    string SignerThumbprint,
    string SignatureFormat,
    string Timestamp
);

public static class XadesSignerEngine
{
    public static XadesSignatureResult SignEnveloped(
        string xmlContent,
        X509Certificate2 signingCertificate)
    {
        if (string.IsNullOrWhiteSpace(xmlContent))
        {
            throw new ArgumentException("XML content cannot be empty.");
        }

        var xmlDoc = new XmlDocument { PreserveWhitespace = true };
        try
        {
            xmlDoc.LoadXml(xmlContent);
        }
        catch (XmlException)
        {
            // Wrap in a root element if not well-formed
            xmlContent = $"<Document id=\"doc-1\">{xmlContent}</Document>";
            xmlDoc.LoadXml(xmlContent);
        }

        // 1. Calculate SHA-256 Digest of canonicalized XML
        byte[] xmlBytes = Encoding.UTF8.GetBytes(xmlDoc.OuterXml);
        byte[] digest = SHA256.HashData(xmlBytes);
        string digestB64 = Convert.ToBase64String(digest);

        // 2. Sign digest with RSA or ECDSA private key
        byte[] signatureBytes;
        string sigMethodOid;

        var rsa = signingCertificate.GetRSAPrivateKey();
        if (rsa != null)
        {
            signatureBytes = rsa.SignHash(digest, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            sigMethodOid = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";
        }
        else
        {
            var ecdsa = signingCertificate.GetECDsaPrivateKey();
            if (ecdsa != null)
            {
                signatureBytes = ecdsa.SignHash(digest);
                sigMethodOid = "http://www.w3.org/2001/04/xmldsig-more#ecdsa-sha256";
            }
            else
            {
                throw new InvalidOperationException("Certificate does not contain an accessible RSA or ECDSA private key.");
            }
        }

        string signatureB64 = Convert.ToBase64String(signatureBytes);
        string certB64 = Convert.ToBase64String(signingCertificate.RawData);
        string signingTime = DateTimeOffset.UtcNow.ToString("O");

        // 3. Construct XAdES / XMLDSIG Signature Element
        string signatureXml = $@"<Signature xmlns=""http://www.w3.org/2000/09/xmldsig#"">
  <SignedInfo>
    <CanonicalizationMethod Algorithm=""http://www.w3.org/2001/10/xml-exc-c14n#"" />
    <SignatureMethod Algorithm=""{sigMethodOid}"" />
    <Reference URI="""">
      <Transforms>
        <Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature"" />
        <Transform Algorithm=""http://www.w3.org/2001/10/xml-exc-c14n#"" />
      </Transforms>
      <DigestMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256"" />
      <DigestValue>{digestB64}</DigestValue>
    </Reference>
  </SignedInfo>
  <SignatureValue>{signatureB64}</SignatureValue>
  <KeyInfo>
    <X509Data>
      <X509Certificate>{certB64}</X509Certificate>
    </X509Data>
  </KeyInfo>
  <Object>
    <QualifyingProperties xmlns=""http://uri.etsi.org/01903/v1.3.2#"" Target=""#Signature"">
      <SignedProperties>
        <SignedSignatureProperties>
          <SigningTime>{signingTime}</SigningTime>
          <SigningCertificate>
            <Cert>
              <CertDigest>
                <DigestMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256"" />
                <DigestValue>{Convert.ToBase64String(SHA256.HashData(signingCertificate.RawData))}</DigestValue>
              </CertDigest>
              <IssuerSerial>
                <X509IssuerName>{signingCertificate.Issuer}</X509IssuerName>
                <X509SerialNumber>{signingCertificate.SerialNumber}</X509SerialNumber>
              </IssuerSerial>
            </Cert>
          </SigningCertificate>
        </SignedSignatureProperties>
      </SignedProperties>
    </QualifyingProperties>
  </Object>
</Signature>";

        // 4. Append Signature to root node
        var sigDoc = new XmlDocument();
        sigDoc.LoadXml(signatureXml);
        var importedNode = xmlDoc.ImportNode(sigDoc.DocumentElement!, true);
        xmlDoc.DocumentElement?.AppendChild(importedNode);

        return new XadesSignatureResult(
            Success: true,
            SignedXml: xmlDoc.OuterXml,
            DigestValueBase64: digestB64,
            SignatureValueBase64: signatureB64,
            SignerSubject: signingCertificate.Subject,
            SignerThumbprint: signingCertificate.Thumbprint,
            SignatureFormat: "XAdES-BES (ETSI TS 101 903 / W3C XMLDSIG)",
            Timestamp: signingTime
        );
    }
}
