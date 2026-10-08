import React, { useState } from 'react';
import {
  PenTool,
  KeyRound,
  ShieldCheck,
  CheckCircle2,
  AlertCircle,
  Copy,
  Download,
  Check,
  FileText,
  Code2,
  Package,
  Clock,
  Sparkles
} from 'lucide-react';
import { Certificate, SignResponse, PdfSignResponse, XmlSignResponse, CadesSignResponse } from '../types';
import { executeSign, executePdfSign, executeXmlSign, executeCadesSign } from '../api';

interface SigningTestViewProps {
  certificates: Certificate[];
  selectedCertId?: string;
}

type StudioMode = 'RAW' | 'PDF' | 'XML' | 'CADES';

export const SigningTestView: React.FC<SigningTestViewProps> = ({
  certificates,
  selectedCertId: initialCertId
}) => {
  const [activeTab, setActiveTab] = useState<StudioMode>('PDF');
  const [certId, setCertId] = useState<string>(initialCertId || (certificates[0]?.id || ''));
  const [hashAlgo, setHashAlgo] = useState<string>('SHA-256');
  const [inputText, setInputText] = useState<string>('Universal Local Signing Bridge Test Document @ ' + new Date().toISOString());

  // PDF Studio states
  const [pdfReason, setPdfReason] = useState<string>('Digitally approved and verified via Smart Card');
  const [pdfLocation, setPdfLocation] = useState<string>('Localhost Enterprise Bridge');
  const [pdfPage, setPdfPage] = useState<number>(1);
  const [enableVisual, setEnableVisual] = useState<boolean>(true);
  const [pdfResult, setPdfResult] = useState<PdfSignResponse | null>(null);

  // XML Studio states
  const [xmlContent, setXmlContent] = useState<string>(
`<Invoice id="INV-2026-001">
  <Issuer>National PKI Portal</Issuer>
  <Recipient>Enterprise Partner Ltd</Recipient>
  <Amount currency="INR">250000.00</Amount>
  <Status>PendingSignature</Status>
</Invoice>`
  );
  const [xmlResult, setXmlResult] = useState<XmlSignResponse | null>(null);

  // CAdES Studio states
  const [cadesResult, setCadesResult] = useState<CadesSignResponse | null>(null);

  // Raw Studio states
  const [rawResult, setRawResult] = useState<SignResponse | null>(null);

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [copied, setCopied] = useState(false);

  const selectedCert = certificates.find(c => c.id === certId) || certificates[0];

  const handleSign = async () => {
    if (!certId && !selectedCert) {
      setError('Please select a certificate with an available private key.');
      return;
    }

    setLoading(true);
    setError(null);

    const targetCertId = certId || selectedCert.id;

    try {
      if (activeTab === 'PDF') {
        const res = await executePdfSign({
          certificateId: targetCertId,
          createSampleIfEmpty: true,
          enableVisualSignature: enableVisual,
          pageNumber: pdfPage,
          reason: pdfReason,
          location: pdfLocation
        });
        setPdfResult(res);
      } else if (activeTab === 'XML') {
        const res = await executeXmlSign({
          certificateId: targetCertId,
          xmlContent
        });
        setXmlResult(res);
      } else if (activeTab === 'CADES') {
        const base64Data = btoa(unescape(encodeURIComponent(inputText)));
        const res = await executeCadesSign({
          certificateId: targetCertId,
          data: base64Data,
          hashAlgorithm: hashAlgo,
          detached: true
        });
        setCadesResult(res);
      } else {
        const base64Data = btoa(unescape(encodeURIComponent(inputText)));
        const res = await executeSign({
          certificateId: targetCertId,
          hashAlgorithm: hashAlgo,
          data: base64Data
        });
        setRawResult(res);
      }
    } catch (err: any) {
      setError(err.message || 'Signing operation failed.');
    } finally {
      setLoading(false);
    }
  };

  const copyText = (text: string) => {
    navigator.clipboard.writeText(text);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  const downloadSignedPdf = () => {
    if (!pdfResult?.signedPdfBase64) return;
    const byteCharacters = atob(pdfResult.signedPdfBase64);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
      byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: 'application/pdf' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `Signed-Document-PAdES-${Date.now()}.pdf`;
    a.click();
    URL.revokeObjectURL(url);
  };

  const downloadSignedXml = () => {
    if (!xmlResult?.signedXml) return;
    const blob = new Blob([xmlResult.signedXml], { type: 'application/xml' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `Signed-Document-XAdES-${Date.now()}.xml`;
    a.click();
    URL.revokeObjectURL(url);
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 20 }}>
      {/* Header */}
      <div>
        <h2 style={{ fontSize: 20, fontWeight: 700, margin: '0 0 4px 0', display: 'flex', alignItems: 'center', gap: 10 }}>
          <PenTool size={20} color="#3b82f6" />
          Advanced Document & Digital Signing Studio
        </h2>
        <p style={{ margin: 0, fontSize: 13, color: 'var(--text-muted)' }}>
          Enterprise cryptographic signing supporting ISO 32000-1 PAdES-BES, ETSI TS 101 903 XAdES-BES, RFC 5652 CAdES-BES & raw PKI algorithms.
        </p>
      </div>

      {/* Safety Notice */}
      <div className="glass-card" style={{
        padding: '14px 18px',
        borderLeft: '4px solid #3b82f6',
        display: 'flex',
        alignItems: 'center',
        gap: 12
      }}>
        <ShieldCheck size={20} color="#3b82f6" />
        <div style={{ fontSize: 13, color: 'var(--text-muted)' }}>
          <strong style={{ color: '#f8fafc' }}>Protected Hardware Boundary:</strong> When physical USB tokens (Aladdin eToken, ProxKey, ePass2003) are attached, cryptographic operations occur directly on the chip without key extraction.
        </div>
      </div>

      {/* Format Selector Tabs */}
      <div style={{ display: 'flex', gap: 8, borderBottom: '1px solid rgba(255, 255, 255, 0.08)', paddingBottom: 12 }}>
        <button
          type="button"
          onClick={() => setActiveTab('PDF')}
          className={`btn ${activeTab === 'PDF' ? 'btn-primary' : 'btn-secondary'}`}
          style={{ display: 'flex', alignItems: 'center', gap: 8, padding: '8px 16px', fontSize: 13 }}
        >
          <FileText size={16} />
          <span>PDF Signing Studio (PAdES-BES)</span>
        </button>

        <button
          type="button"
          onClick={() => setActiveTab('XML')}
          className={`btn ${activeTab === 'XML' ? 'btn-primary' : 'btn-secondary'}`}
          style={{ display: 'flex', alignItems: 'center', gap: 8, padding: '8px 16px', fontSize: 13 }}
        >
          <Code2 size={16} />
          <span>XML Signer (XAdES-BES)</span>
        </button>

        <button
          type="button"
          onClick={() => setActiveTab('CADES')}
          className={`btn ${activeTab === 'CADES' ? 'btn-primary' : 'btn-secondary'}`}
          style={{ display: 'flex', alignItems: 'center', gap: 8, padding: '8px 16px', fontSize: 13 }}
        >
          <Package size={16} />
          <span>CAdES / PKCS#7 Detached</span>
        </button>

        <button
          type="button"
          onClick={() => setActiveTab('RAW')}
          className={`btn ${activeTab === 'RAW' ? 'btn-primary' : 'btn-secondary'}`}
          style={{ display: 'flex', alignItems: 'center', gap: 8, padding: '8px 16px', fontSize: 13 }}
        >
          <PenTool size={16} />
          <span>Raw PKI Sign (SHA-256/384/512)</span>
        </button>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(380px, 1fr))', gap: 20 }}>
        {/* Input Configuration Panel */}
        <div className="glass-card" style={{ padding: 22, display: 'flex', flexDirection: 'column', gap: 16 }}>
          <h3 style={{ fontSize: 15, fontWeight: 700, margin: 0, display: 'flex', alignItems: 'center', gap: 8 }}>
            <Sparkles size={16} color="#38bdf8" />
            {activeTab === 'PDF' && 'PDF Signature Parameters'}
            {activeTab === 'XML' && 'XML Enveloped Signature Setup'}
            {activeTab === 'CADES' && 'CAdES Detached Envelope Configuration'}
            {activeTab === 'RAW' && 'Raw Cryptographic Sign Parameters'}
          </h3>

          {/* Certificate Selector */}
          <div>
            <label style={{ display: 'block', fontSize: 12, fontWeight: 600, color: 'var(--text-muted)', marginBottom: 6 }}>
              Signing Certificate
            </label>
            <select
              className="form-input"
              value={certId || selectedCert?.id || ''}
              onChange={e => setCertId(e.target.value)}
            >
              {certificates.map(c => (
                <option key={c.id} value={c.id}>
                  {c.subject} ({c.publicKeyAlgorithm} {c.keySize}-bit) [{c.providerId}]
                </option>
              ))}
            </select>
          </div>

          {/* PDF-Specific Controls */}
          {activeTab === 'PDF' && (
            <>
              <div>
                <label style={{ display: 'block', fontSize: 12, fontWeight: 600, color: 'var(--text-muted)', marginBottom: 6 }}>
                  Signing Reason / Statement
                </label>
                <input
                  type="text"
                  className="form-input"
                  value={pdfReason}
                  onChange={e => setPdfReason(e.target.value)}
                  placeholder="e.g. Approved and authenticated"
                />
              </div>

              <div>
                <label style={{ display: 'block', fontSize: 12, fontWeight: 600, color: 'var(--text-muted)', marginBottom: 6 }}>
                  Signing Location
                </label>
                <input
                  type="text"
                  className="form-input"
                  value={pdfLocation}
                  onChange={e => setPdfLocation(e.target.value)}
                  placeholder="e.g. Gujarat, India"
                />
              </div>

              <div style={{ display: 'flex', gap: 12, alignItems: 'center' }}>
                <label style={{ display: 'flex', alignItems: 'center', gap: 8, fontSize: 13, cursor: 'pointer', color: '#f8fafc' }}>
                  <input
                    type="checkbox"
                    checked={enableVisual}
                    onChange={e => setEnableVisual(e.target.checked)}
                    style={{ accentColor: '#3b82f6', width: 16, height: 16 }}
                  />
                  Embed Visual Signature Stamp
                </label>

                <div style={{ marginLeft: 'auto', display: 'flex', alignItems: 'center', gap: 8 }}>
                  <span style={{ fontSize: 12, color: 'var(--text-muted)' }}>Page:</span>
                  <input
                    type="number"
                    min={1}
                    className="form-input"
                    value={pdfPage}
                    onChange={e => setPdfPage(parseInt(e.target.value) || 1)}
                    style={{ width: 60, padding: '4px 8px', fontSize: 12 }}
                  />
                </div>
              </div>
            </>
          )}

          {/* XML-Specific Controls */}
          {activeTab === 'XML' && (
            <div>
              <label style={{ display: 'block', fontSize: 12, fontWeight: 600, color: 'var(--text-muted)', marginBottom: 6 }}>
                XML Document to Sign (W3C Canonicalization + Enveloped XMLDSIG)
              </label>
              <textarea
                className="form-input mono-text"
                rows={8}
                value={xmlContent}
                onChange={e => setXmlContent(e.target.value)}
                style={{ fontSize: 12, resize: 'vertical' }}
              />
            </div>
          )}

          {/* CAdES / Raw Controls */}
          {(activeTab === 'CADES' || activeTab === 'RAW') && (
            <>
              <div>
                <label style={{ display: 'block', fontSize: 12, fontWeight: 600, color: 'var(--text-muted)', marginBottom: 6 }}>
                  Hash Algorithm
                </label>
                <div style={{ display: 'flex', gap: 8 }}>
                  {['SHA-256', 'SHA-384', 'SHA-512'].map(algo => (
                    <button
                      key={algo}
                      type="button"
                      onClick={() => setHashAlgo(algo)}
                      className={`btn btn-sm ${hashAlgo === algo ? 'btn-primary' : 'btn-secondary'}`}
                      style={{ flex: 1 }}
                    >
                      {algo}
                    </button>
                  ))}
                </div>
              </div>

              <div>
                <label style={{ display: 'block', fontSize: 12, fontWeight: 600, color: 'var(--text-muted)', marginBottom: 6 }}>
                  Payload Data to Authenticate
                </label>
                <textarea
                  className="form-input"
                  rows={4}
                  value={inputText}
                  onChange={e => setInputText(e.target.value)}
                  style={{ resize: 'vertical' }}
                />
              </div>
            </>
          )}

          <button
            onClick={handleSign}
            className="btn btn-primary"
            disabled={loading || certificates.length === 0}
            style={{ width: '100%', padding: '12px 16px', fontWeight: 600 }}
          >
            {loading ? (
              <>
                <span className="pulse-dot" />
                <span>Computing Hardware Signature...</span>
              </>
            ) : (
              <>
                <PenTool size={16} />
                <span>
                  {activeTab === 'PDF' && 'Sign & Generate PAdES PDF'}
                  {activeTab === 'XML' && 'Sign & Envelope XML'}
                  {activeTab === 'CADES' && 'Generate CAdES-BES PKCS#7'}
                  {activeTab === 'RAW' && 'Sign Data Payload'}
                </span>
              </>
            )}
          </button>

          {error && (
            <div style={{
              background: 'rgba(244, 63, 94, 0.1)',
              border: '1px solid rgba(244, 63, 94, 0.3)',
              borderRadius: 8,
              padding: 12,
              fontSize: 13,
              color: '#f43f5e',
              display: 'flex',
              gap: 8,
              alignItems: 'center'
            }}>
              <AlertCircle size={16} />
              <span>{error}</span>
            </div>
          )}
        </div>

        {/* Results Panel */}
        <div className="glass-card" style={{ padding: 22, display: 'flex', flexDirection: 'column', gap: 16 }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <h3 style={{ fontSize: 15, fontWeight: 700, margin: 0 }}>Digital Signature Output</h3>
            {((activeTab === 'PDF' && pdfResult) ||
              (activeTab === 'XML' && xmlResult) ||
              (activeTab === 'CADES' && cadesResult) ||
              (activeTab === 'RAW' && rawResult)) && (
              <span className="badge badge-success">
                <CheckCircle2 size={12} /> Valid Signature
              </span>
            )}
          </div>

          {/* PDF Result */}
          {activeTab === 'PDF' && (
            !pdfResult ? (
              <div style={{
                flex: 1,
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                justifyContent: 'center',
                color: 'var(--text-dim)',
                textAlign: 'center',
                padding: 40,
                gap: 10
              }}>
                <FileText size={40} strokeWidth={1.5} color="#38bdf8" />
                <p style={{ margin: 0, fontSize: 13 }}>Click "Sign & Generate PAdES PDF" to create an ETSI-compliant signed PDF with embedded visual seal.</p>
              </div>
            ) : (
              <div style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
                <div style={{
                  background: 'rgba(56, 189, 248, 0.08)',
                  border: '1px solid rgba(56, 189, 248, 0.25)',
                  borderRadius: 8,
                  padding: 14,
                  display: 'flex',
                  justifyContent: 'space-between',
                  alignItems: 'center'
                }}>
                  <div>
                    <div style={{ fontSize: 14, fontWeight: 700, color: '#f8fafc' }}>{pdfResult.signatureFormat}</div>
                    <div style={{ fontSize: 12, color: 'var(--text-muted)', marginTop: 2 }}>
                      File Size: {Math.round(pdfResult.totalBytes / 1024)} KB • Execution: {pdfResult.durationMs} ms
                    </div>
                  </div>
                  <button onClick={downloadSignedPdf} className="btn btn-primary btn-sm" style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
                    <Download size={14} />
                    <span>Download Signed PDF</span>
                  </button>
                </div>

                <div>
                  <span style={{ fontSize: 12, color: 'var(--text-muted)' }}>Document Hash (SHA-256 ByteRange):</span>
                  <div className="mono-text" style={{
                    background: '#060911',
                    border: '1px solid rgba(255, 255, 255, 0.08)',
                    borderRadius: 8,
                    padding: 10,
                    fontSize: 11,
                    color: '#38bdf8',
                    marginTop: 4,
                    wordBreak: 'break-all'
                  }}>
                    {pdfResult.documentHashSha256}
                  </div>
                </div>

                <div style={{
                  display: 'grid',
                  gridTemplateColumns: '1fr 1fr',
                  gap: 10,
                  fontSize: 12,
                  padding: 12,
                  background: 'rgba(255, 255, 255, 0.02)',
                  borderRadius: 8
                }}>
                  <div>
                    <span style={{ color: 'var(--text-dim)' }}>Signer:</span>
                    <div style={{ color: '#f8fafc', marginTop: 2, fontWeight: 600 }}>{pdfResult.signerSubject}</div>
                  </div>
                  <div>
                    <span style={{ color: 'var(--text-dim)' }}>Thumbprint:</span>
                    <div className="mono-text" style={{ color: '#94a3b8', marginTop: 2, fontSize: 11 }}>{pdfResult.signerThumbprint.slice(0, 16)}...</div>
                  </div>
                </div>
              </div>
            )
          )}

          {/* XML Result */}
          {activeTab === 'XML' && (
            !xmlResult ? (
              <div style={{
                flex: 1,
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                justifyContent: 'center',
                color: 'var(--text-dim)',
                textAlign: 'center',
                padding: 40,
                gap: 10
              }}>
                <Code2 size={40} strokeWidth={1.5} color="#34d399" />
                <p style={{ margin: 0, fontSize: 13 }}>Click "Sign & Envelope XML" to generate W3C XMLDSIG / XAdES-BES output.</p>
              </div>
            ) : (
              <div style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <span style={{ fontSize: 12, color: 'var(--text-muted)' }}>Signed XML with Enveloped Signature:</span>
                  <div style={{ display: 'flex', gap: 6 }}>
                    <button onClick={() => copyText(xmlResult.signedXml)} className="btn btn-secondary btn-sm" style={{ padding: '3px 8px', fontSize: 11 }}>
                      {copied ? <Check size={12} color="#10b981" /> : <Copy size={12} />}
                      <span>{copied ? 'Copied' : 'Copy'}</span>
                    </button>
                    <button onClick={downloadSignedXml} className="btn btn-secondary btn-sm" style={{ padding: '3px 8px', fontSize: 11 }}>
                      <Download size={12} />
                      <span>Download</span>
                    </button>
                  </div>
                </div>

                <div className="mono-text" style={{
                  background: '#060911',
                  border: '1px solid rgba(255, 255, 255, 0.08)',
                  borderRadius: 8,
                  padding: 12,
                  maxHeight: 180,
                  overflowY: 'auto',
                  fontSize: 11,
                  color: '#34d399',
                  whiteSpace: 'pre-wrap'
                }}>
                  {xmlResult.signedXml}
                </div>

                <div style={{
                  display: 'grid',
                  gridTemplateColumns: '1fr 1fr',
                  gap: 10,
                  fontSize: 12,
                  padding: 12,
                  background: 'rgba(255, 255, 255, 0.02)',
                  borderRadius: 8
                }}>
                  <div>
                    <span style={{ color: 'var(--text-dim)' }}>Format:</span>
                    <div style={{ color: '#f8fafc', marginTop: 2, fontWeight: 600 }}>{xmlResult.signatureFormat}</div>
                  </div>
                  <div>
                    <span style={{ color: 'var(--text-dim)' }}>Digest:</span>
                    <div className="mono-text" style={{ color: '#38bdf8', marginTop: 2, fontSize: 11 }}>{xmlResult.digestValueBase64.slice(0, 16)}...</div>
                  </div>
                </div>
              </div>
            )
          )}

          {/* CAdES Result */}
          {activeTab === 'CADES' && (
            !cadesResult ? (
              <div style={{
                flex: 1,
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                justifyContent: 'center',
                color: 'var(--text-dim)',
                textAlign: 'center',
                padding: 40,
                gap: 10
              }}>
                <Package size={40} strokeWidth={1.5} color="#a855f7" />
                <p style={{ margin: 0, fontSize: 13 }}>Click "Generate CAdES-BES PKCS#7" to produce an ETSI-compliant CMS cryptographic envelope.</p>
              </div>
            ) : (
              <div style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <span style={{ fontSize: 12, color: 'var(--text-muted)' }}>CAdES-BES Signature Container (Base64):</span>
                  <button onClick={() => copyText(cadesResult.signatureBase64)} className="btn btn-secondary btn-sm" style={{ padding: '3px 8px', fontSize: 11 }}>
                    {copied ? <Check size={12} color="#10b981" /> : <Copy size={12} />}
                    <span>{copied ? 'Copied' : 'Copy'}</span>
                  </button>
                </div>

                <div className="mono-text" style={{
                  background: '#060911',
                  border: '1px solid rgba(255, 255, 255, 0.08)',
                  borderRadius: 8,
                  padding: 12,
                  maxHeight: 140,
                  overflowY: 'auto',
                  fontSize: 11,
                  color: '#c084fc',
                  wordBreak: 'break-all'
                }}>
                  {cadesResult.signatureBase64}
                </div>

                <div style={{
                  display: 'grid',
                  gridTemplateColumns: '1fr 1fr',
                  gap: 10,
                  fontSize: 12,
                  padding: 12,
                  background: 'rgba(255, 255, 255, 0.02)',
                  borderRadius: 8
                }}>
                  <div>
                    <span style={{ color: 'var(--text-dim)' }}>Format:</span>
                    <div style={{ color: '#f8fafc', marginTop: 2, fontWeight: 600 }}>{cadesResult.signatureFormat}</div>
                  </div>
                  <div>
                    <span style={{ color: 'var(--text-dim)' }}>Payload Size:</span>
                    <div style={{ color: '#34d399', marginTop: 2, fontWeight: 600 }}>{cadesResult.signedContentBytes} bytes</div>
                  </div>
                </div>
              </div>
            )
          )}

          {/* Raw Result */}
          {activeTab === 'RAW' && (
            !rawResult ? (
              <div style={{
                flex: 1,
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                justifyContent: 'center',
                color: 'var(--text-dim)',
                textAlign: 'center',
                padding: 40,
                gap: 10
              }}>
                <KeyRound size={40} strokeWidth={1.5} color="#f59e0b" />
                <p style={{ margin: 0, fontSize: 13 }}>Click "Sign Data Payload" to perform raw digital signing with the selected key.</p>
              </div>
            ) : (
              <div style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <span style={{ fontSize: 12, color: 'var(--text-muted)' }}>Cryptographic Signature (Base64)</span>
                  <button onClick={() => copyText(rawResult.signature)} className="btn btn-secondary btn-sm" style={{ padding: '3px 8px', fontSize: 11 }}>
                    {copied ? <Check size={12} color="#10b981" /> : <Copy size={12} />}
                    <span>{copied ? 'Copied' : 'Copy'}</span>
                  </button>
                </div>
                <div className="mono-text" style={{
                  background: '#060911',
                  border: '1px solid rgba(255, 255, 255, 0.08)',
                  borderRadius: 8,
                  padding: 12,
                  maxHeight: 120,
                  overflowY: 'auto',
                  fontSize: 12,
                  color: '#34d399',
                  wordBreak: 'break-all'
                }}>
                  {rawResult.signature}
                </div>

                <div style={{
                  display: 'grid',
                  gridTemplateColumns: '1fr 1fr',
                  gap: 10,
                  fontSize: 12,
                  padding: 12,
                  background: 'rgba(255, 255, 255, 0.02)',
                  borderRadius: 8
                }}>
                  <div>
                    <span style={{ color: 'var(--text-dim)' }}>Signer:</span>
                    <div style={{ color: '#f8fafc', marginTop: 2, fontWeight: 600 }}>{rawResult.certificate.subject}</div>
                  </div>
                  <div>
                    <span style={{ color: 'var(--text-dim)' }}>Algorithm:</span>
                    <div style={{ color: '#f8fafc', marginTop: 2, fontWeight: 600 }}>{rawResult.algorithm}</div>
                  </div>
                </div>
              </div>
            )
          )}
        </div>
      </div>
    </div>
  );
};
