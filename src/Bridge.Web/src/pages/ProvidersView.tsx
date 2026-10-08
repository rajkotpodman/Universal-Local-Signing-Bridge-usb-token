import React from 'react';
import { Layers, CheckCircle2, XCircle, AlertCircle, HardDrive, Cpu, Terminal } from 'lucide-react';
import { ProviderInfo } from '../types';

interface ProvidersViewProps {
  providers: ProviderInfo[];
}

export const ProvidersView: React.FC<ProvidersViewProps> = ({ providers }) => {
  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 20 }}>
      {/* Header */}
      <div>
        <h2 style={{ fontSize: 20, fontWeight: 700, margin: '0 0 4px 0', display: 'flex', alignItems: 'center', gap: 10 }}>
          <Layers size={20} color="#3b82f6" />
          Cryptographic Provider Architecture
        </h2>
        <p style={{ margin: 0, fontSize: 13, color: 'var(--text-muted)' }}>
          Modular signing engines interfacing with operating system stores and hardware security devices.
        </p>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))', gap: 18 }}>
        {providers.map(prov => {
          const isAvailable = prov.status === 'AVAILABLE' || prov.status === 'MOCK';
          const isMock = prov.status === 'MOCK';

          return (
            <div key={prov.id} className="glass-card" style={{ padding: 22, display: 'flex', flexDirection: 'column', gap: 14 }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                  <div style={{
                    padding: 8,
                    borderRadius: 8,
                    background: isMock ? 'rgba(245, 158, 11, 0.15)' : 'rgba(59, 130, 246, 0.15)',
                    color: isMock ? '#fbbf24' : '#60a5fa'
                  }}>
                    {isMock ? <Cpu size={20} /> : <HardDrive size={20} />}
                  </div>
                  <div>
                    <h3 style={{ fontSize: 16, fontWeight: 700, margin: 0, color: '#f8fafc' }}>
                      {prov.name}
                    </h3>
                    <span style={{ fontSize: 12, color: 'var(--text-dim)' }}>
                      {prov.type}
                    </span>
                  </div>
                </div>

                <span className={`badge ${
                  prov.status === 'AVAILABLE' ? 'badge-success' :
                  prov.status === 'MOCK' ? 'badge-warning' :
                  prov.status === 'ERROR' ? 'badge-danger' : 'badge-secondary'
                }`}>
                  {prov.status === 'AVAILABLE' && <CheckCircle2 size={12} />}
                  {prov.status === 'UNAVAILABLE' && <XCircle size={12} />}
                  {prov.status === 'ERROR' && <AlertCircle size={12} />}
                  <span>{prov.status}</span>
                </span>
              </div>

              <p style={{ margin: 0, fontSize: 13, color: 'var(--text-muted)', lineHeight: 1.5 }}>
                {prov.description}
              </p>

              {prov.details && (
                <div style={{
                  background: 'rgba(255, 255, 255, 0.02)',
                  border: '1px solid rgba(255, 255, 255, 0.06)',
                  borderRadius: 8,
                  padding: 10,
                  fontSize: 12,
                  color: '#93c5fd'
                }}>
                  {prov.details}
                </div>
              )}

              <div style={{ marginTop: 'auto', paddingTop: 10, borderTop: '1px solid rgba(255, 255, 255, 0.05)', fontSize: 12 }}>
                <span style={{ color: 'var(--text-dim)' }}>Supported Algorithms: </span>
                <span className="mono-text" style={{ color: '#e2e8f0' }}>
                  {prov.supportedAlgorithms.join(', ')}
                </span>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
};
