import React, { useState } from 'react';
import { PenTool, KeyRound, ShieldCheck, CheckCircle2, AlertCircle, Copy, Download, Check } from 'lucide-react';
import { Certificate, SignResponse } from '../types';
import { executeSign } from '../api';

interface SigningTestViewProps {
  certificates: Certificate[];
  selectedCertId?: string;
}

export const SigningTestView: React.FC<SigningTestViewProps> = ({
  certificates,
  selectedCertId: initialCertId
}) => {
  const [certId, setCertId] = useState<string>(initialCertId || (certificates[0]?.id || ''));
  const [hashAlgo, setHashAlgo] = useState<string>('SHA-256');
  const [inputText, setInputText] = useState<string>('Universal Local Signing Bridge Test Document @ ' + new Date().toISOString());
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState<SignResponse | null>(null);
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
    setResult(null);

    try {
      const base64Data = btoa(unescape(encodeURIComponent(inputText)));
      const res = await executeSign({
        certificateId: certId || selectedCert.id,
        hashAlgorithm: hashAlgo,
        data: base64Data
      });

      setResult(res);
    } catch (err: any) {
      setError(err.message || 'Signing operation failed.');
    } finally {
      setLoading(false);
    }
  };

  const copySignature = () => {
    if (!result?.signature) return;
    navigator.clipboard.writeText(result.signature);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  const downloadResult = () => {
    if (!result) return;
    const blob = new Blob([JSON.stringify(result, null, 2)], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `signature-${result.algorithm}-${Date.now()}.json`;
    a.click();
    URL.revokeObjectURL(url);
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 20 }}>
      {/* Header */}
      <div>
        <h2 style={{ fontSize: 20, fontWeight: 700, margin: '0 0 4px 0', display: 'flex', alignItems: 'center', gap: 10 }}>
          <PenTool size={20} color="#3b82f6" />
          Interactive Cryptographic Signing Test
        </h2>
        <p style={{ margin: 0, fontSize: 13, color: 'var(--text-muted)' }}>
          Execute and inspect standard digital signature operations using local USB tokens and PKI providers.
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
          <strong style={{ color: '#f8fafc' }}>Protected Execution Boundary:</strong> When using physical USB smart cards, token PIN entry is handled via native Windows OS dialogs. The bridge never captures or logs PINs.
        </div>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(380px, 1fr))', gap: 20 }}>
        {/* Input Configuration Panel */}
        <div className="glass-card" style={{ padding: 22, display: 'flex', flexDirection: 'column', gap: 16 }}>
          <h3 style={{ fontSize: 15, fontWeight: 700, margin: 0 }}>Signing Configuration</h3>

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

          {/* Hash Algorithm Selector */}
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

          {/* Payload to Sign */}
          <div>
            <label style={{ display: 'block', fontSize: 12, fontWeight: 600, color: 'var(--text-muted)', marginBottom: 6 }}>
              Document / Data to Authenticate
            </label>
            <textarea
              className="form-input"
              rows={4}
              value={inputText}
              onChange={e => setInputText(e.target.value)}
              placeholder="Enter text payload to sign..."
              style={{ resize: 'vertical' }}
            />
          </div>

          <button
            onClick={handleSign}
            className="btn btn-primary"
            disabled={loading || certificates.length === 0}
            style={{ width: '100%', padding: '11px 16px' }}
          >
            {loading ? (
              <>
                <span className="pulse-dot" />
                <span>Signing via Cryptographic Provider...</span>
              </>
            ) : (
              <>
                <PenTool size={15} />
                <span>Sign Data</span>
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
            {result && (
              <span className="badge badge-success">
                <CheckCircle2 size={12} /> Valid ({result.durationMs}ms)
              </span>
            )}
          </div>

          {!result ? (
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
              <KeyRound size={36} strokeWidth={1.5} />
              <p style={{ margin: 0, fontSize: 13 }}>Click "Sign Data" to perform digital signing with the selected key.</p>
            </div>
          ) : (
            <div style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
              <div>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 6 }}>
                  <span style={{ fontSize: 12, color: 'var(--text-muted)' }}>Cryptographic Signature (Base64)</span>
                  <div style={{ display: 'flex', gap: 6 }}>
                    <button onClick={copySignature} className="btn btn-secondary btn-sm" style={{ padding: '3px 8px', fontSize: 11 }}>
                      {copied ? <Check size={12} color="#10b981" /> : <Copy size={12} />}
                      <span>{copied ? 'Copied' : 'Copy'}</span>
                    </button>
                    <button onClick={downloadResult} className="btn btn-secondary btn-sm" style={{ padding: '3px 8px', fontSize: 11 }}>
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
                  maxHeight: 120,
                  overflowY: 'auto',
                  fontSize: 12,
                  color: '#34d399',
                  wordBreak: 'break-all'
                }}>
                  {result.signature}
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
                  <div style={{ color: '#f8fafc', marginTop: 2, fontWeight: 600 }}>{result.certificate.subject}</div>
                </div>
                <div>
                  <span style={{ color: 'var(--text-dim)' }}>Algorithm:</span>
                  <div style={{ color: '#f8fafc', marginTop: 2, fontWeight: 600 }}>{result.algorithm}</div>
                </div>
                <div>
                  <span style={{ color: 'var(--text-dim)' }}>Provider:</span>
                  <div style={{ color: '#93c5fd', marginTop: 2 }}>{result.providerId}</div>
                </div>
                <div>
                  <span style={{ color: 'var(--text-dim)' }}>Duration:</span>
                  <div style={{ color: '#34d399', marginTop: 2 }}>{result.durationMs} ms</div>
                </div>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
