import React, { useState } from 'react';
import { KeyRound, Search, AlertTriangle, ShieldCheck, Cpu, PenTool, RefreshCw } from 'lucide-react';
import { Certificate } from '../types';

interface CertificatesViewProps {
  certificates: Certificate[];
  onRefresh: () => void;
  loading: boolean;
  onSelectSign: (certId: string) => void;
}

export const CertificatesView: React.FC<CertificatesViewProps> = ({
  certificates,
  onRefresh,
  loading,
  onSelectSign
}) => {
  const [searchTerm, setSearchTerm] = useState('');
  const [filterExpiry, setFilterExpiry] = useState<'all' | 'valid' | 'expiring' | 'expired'>('all');
  const [filterProvider, setFilterProvider] = useState<string>('all');

  const filtered = certificates.filter(c => {
    const matchesSearch = c.subject.toLowerCase().includes(searchTerm.toLowerCase()) ||
                          c.issuer.toLowerCase().includes(searchTerm.toLowerCase()) ||
                          c.thumbprint.toLowerCase().includes(searchTerm.toLowerCase());
    if (!matchesSearch) return false;

    if (filterProvider !== 'all' && c.providerId !== filterProvider) return false;

    if (filterExpiry === 'expired' && c.status !== 'EXPIRED') return false;
    if (filterExpiry === 'expiring' && c.status !== 'EXPIRING_SOON') return false;
    if (filterExpiry === 'valid' && c.status !== 'VALID') return false;

    return true;
  });

  const providersList = Array.from(new Set(certificates.map(c => c.providerId)));

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 20 }}>
      {/* Header */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 14 }}>
        <div>
          <h2 style={{ fontSize: 20, fontWeight: 700, margin: '0 0 4px 0', display: 'flex', alignItems: 'center', gap: 10 }}>
            <KeyRound size={20} color="#3b82f6" />
            Detected Certificates & Cryptographic Keys
          </h2>
          <p style={{ margin: 0, fontSize: 13, color: 'var(--text-muted)' }}>
            Certificates discovered across active signing providers and hardware USB tokens.
          </p>
        </div>

        <button onClick={onRefresh} className="btn btn-secondary btn-sm" disabled={loading}>
          <RefreshCw size={14} className={loading ? 'spin' : ''} />
          <span>Refresh</span>
        </button>
      </div>

      {/* Security Notice */}
      <div className="glass-card" style={{
        padding: '14px 18px',
        borderLeft: '4px solid #10b981',
        display: 'flex',
        alignItems: 'center',
        gap: 12
      }}>
        <ShieldCheck size={20} color="#10b981" />
        <div style={{ fontSize: 13, color: 'var(--text-muted)' }}>
          <strong style={{ color: '#f8fafc' }}>Non-Exportable Key Guarantee:</strong> Private keys are isolated in the hardware smart-card chip or secure OS provider and are never exposed, read, or transmitted to JavaScript.
        </div>
      </div>

      {/* Filter and Search Bar */}
      <div className="glass-card" style={{ padding: 16, display: 'flex', gap: 12, flexWrap: 'wrap', alignItems: 'center' }}>
        <div style={{ flex: 1, minWidth: 260, position: 'relative' }}>
          <Search size={16} color="#64748b" style={{ position: 'absolute', left: 12, top: 11 }} />
          <input
            type="text"
            className="form-input"
            style={{ paddingLeft: 36 }}
            placeholder="Search by subject, issuer, serial or thumbprint..."
            value={searchTerm}
            onChange={e => setSearchTerm(e.target.value)}
          />
        </div>

        {/* Filter by Expiry */}
        <div style={{ display: 'flex', gap: 6 }}>
          {(['all', 'valid', 'expiring', 'expired'] as const).map(tab => (
            <button
              key={tab}
              onClick={() => setFilterExpiry(tab)}
              className={`btn btn-sm ${filterExpiry === tab ? 'btn-primary' : 'btn-secondary'}`}
              style={{ textTransform: 'capitalize' }}
            >
              {tab === 'expiring' ? 'Expiring Soon' : tab}
            </button>
          ))}
        </div>

        {/* Filter by Provider */}
        <select
          className="form-input"
          style={{ width: 'auto', minWidth: 160 }}
          value={filterProvider}
          onChange={e => setFilterProvider(e.target.value)}
        >
          <option value="all">All Providers</option>
          {providersList.map(p => (
            <option key={p} value={p}>{p}</option>
          ))}
        </select>
      </div>

      {/* Certificates List */}
      {filtered.length === 0 ? (
        <div className="glass-card" style={{ padding: 48, textAlign: 'center', color: 'var(--text-dim)' }}>
          <AlertTriangle size={36} color="#f59e0b" style={{ margin: '0 auto 12px' }} />
          <p style={{ margin: 0, fontSize: 14 }}>No certificates matched your filter criteria.</p>
        </div>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
          {filtered.map(cert => {
            const isExpiringSoon = cert.status === 'EXPIRING_SOON';
            const isExpired = cert.status === 'EXPIRED';
            const noPrivateKey = !cert.hasPrivateKey;

            return (
              <div key={cert.id} className="glass-card" style={{ padding: 20 }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: 12 }}>
                  <div style={{ flex: 1, minWidth: 300 }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: 8, flexWrap: 'wrap', marginBottom: 6 }}>
                      <h3 style={{ fontSize: 15, fontWeight: 700, margin: 0, color: '#f8fafc' }}>
                        {cert.subject}
                      </h3>

                      {cert.smartCardBacked && (
                        <span className="badge badge-success">
                          <Cpu size={12} /> Smart Card
                        </span>
                      )}

                      {/* Warnings */}
                      {isExpiringSoon && (
                        <span className="badge badge-warning">
                          <AlertTriangle size={12} /> EXPIRING SOON
                        </span>
                      )}

                      {isExpired && (
                        <span className="badge badge-danger">
                          <AlertTriangle size={12} /> EXPIRED
                        </span>
                      )}

                      {noPrivateKey && (
                        <span className="badge badge-danger">
                          <AlertTriangle size={12} /> NO PRIVATE KEY
                        </span>
                      )}
                    </div>

                    <div style={{ fontSize: 13, color: 'var(--text-muted)' }}>
                      <strong>Issuer:</strong> {cert.issuer}
                    </div>
                  </div>

                  <button
                    onClick={() => onSelectSign(cert.id)}
                    className="btn btn-primary btn-sm"
                    disabled={noPrivateKey}
                  >
                    <PenTool size={14} />
                    <span>Test Sign</span>
                  </button>
                </div>

                {/* Metadata Grid */}
                <div style={{
                  display: 'grid',
                  gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))',
                  gap: 12,
                  marginTop: 14,
                  paddingTop: 14,
                  borderTop: '1px solid rgba(255, 255, 255, 0.05)',
                  fontSize: 12
                }}>
                  <div>
                    <span style={{ color: 'var(--text-dim)' }}>Algorithm & Key Size:</span>
                    <div className="mono-text" style={{ color: '#93c5fd', marginTop: 2, fontWeight: 600 }}>
                      {cert.publicKeyAlgorithm} {cert.keySize} bits ({cert.signatureAlgorithm})
                    </div>
                  </div>

                  <div>
                    <span style={{ color: 'var(--text-dim)' }}>Provider:</span>
                    <div style={{ color: '#e2e8f0', marginTop: 2 }}>
                      {cert.providerId}
                    </div>
                  </div>

                  <div>
                    <span style={{ color: 'var(--text-dim)' }}>Validity:</span>
                    <div style={{ color: '#e2e8f0', marginTop: 2 }}>
                      {new Date(cert.validFrom).toLocaleDateString()} — {new Date(cert.validTo).toLocaleDateString()}
                    </div>
                  </div>

                  <div>
                    <span style={{ color: 'var(--text-dim)' }}>Serial:</span>
                    <div className="mono-text" style={{ color: '#e2e8f0', marginTop: 2 }}>
                      {cert.serialNumber}
                    </div>
                  </div>

                  <div style={{ gridColumn: '1 / -1' }}>
                    <span style={{ color: 'var(--text-dim)' }}>Thumbprint:</span>
                    <div className="mono-text" style={{ color: '#a78bfa', marginTop: 2, wordBreak: 'break-all' }}>
                      {cert.thumbprint}
                    </div>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
};
