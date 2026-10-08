import React, { useState, useEffect } from 'react';
import {
  Layers,
  CheckCircle2,
  XCircle,
  AlertCircle,
  HardDrive,
  Cpu,
  RefreshCw,
  ShieldCheck,
  Key,
  Globe2,
  Zap,
  Info,
  Usb
} from 'lucide-react';
import { ProviderInfo, WorldCatalogResponse, WorldProviderStatus } from '../types';
import { fetchWorldCatalog, scanProviders } from '../api';

interface ProvidersViewProps {
  providers: ProviderInfo[];
}

export const ProvidersView: React.FC<ProvidersViewProps> = ({ providers }) => {
  const [worldCatalog, setWorldCatalog] = useState<WorldCatalogResponse | null>(null);
  const [isScanning, setIsScanning] = useState(false);
  const [scanMessage, setScanMessage] = useState<string | null>(null);

  const loadCatalog = async () => {
    try {
      const data = await fetchWorldCatalog();
      setWorldCatalog(data);
    } catch (err) {
      console.error('Failed to load world catalog:', err);
    }
  };

  useEffect(() => {
    loadCatalog();
  }, []);

  const handleScan = async () => {
    setIsScanning(true);
    setScanMessage(null);
    try {
      const res = await scanProviders();
      setWorldCatalog({
        totalProviders: res.totalProviders,
        aladdinFamilyDetected: res.providers.some(p => p.isAladdinFamily && p.readiness === 'READY'),
        aladdinEtokenReady: res.providers.find(p => p.id === 'aladdin-etoken')?.readiness === 'READY',
        hasAttachedToken: res.hasAttachedToken,
        attachedTokenCount: res.attachedTokenCount,
        attachedTokens: res.attachedTokens,
        providers: res.providers
      });
      setScanMessage(`Scan complete: ${res.attachedTokenCount} attached hardware token(s) detected, ${res.readyProviders} provider(s) active, ${res.certificatesFound} DSC certificate(s) accessible.`);
    } catch (err: any) {
      setScanMessage(`Scan error: ${err.message}`);
    } finally {
      setIsScanning(false);
    }
  };

  const aladdinProvider = worldCatalog?.providers.find(p => p.id === 'aladdin-etoken');

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 24 }}>
      {/* Header & Scan Action */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: 16 }}>
        <div>
          <h2 style={{ fontSize: 22, fontWeight: 700, margin: '0 0 6px 0', display: 'flex', alignItems: 'center', gap: 10 }}>
            <Globe2 size={24} color="#38bdf8" />
            Global Smart Card & Aladdin Provider Ecosystem
          </h2>
          <p style={{ margin: 0, fontSize: 13, color: 'var(--text-muted)' }}>
            Full hardware compatibility layer for Aladdin eToken, SafeNet, Thales, Gemalto, Feitian, ProxKey, mToken, StarKey & YubiKey.
          </p>
        </div>

        <button
          onClick={handleScan}
          disabled={isScanning}
          className="btn-primary"
          style={{ display: 'flex', alignItems: 'center', gap: 8, padding: '10px 18px', cursor: 'pointer' }}
        >
          <RefreshCw size={16} className={isScanning ? 'spin' : ''} />
          {isScanning ? 'Scanning Hardware...' : 'Scan USB & Cryptography Bus'}
        </button>
      </div>

      {/* Currently Attached Hardware Token Status Card */}
      <div className="glass-card" style={{
        padding: 20,
        background: worldCatalog?.hasAttachedToken
          ? 'linear-gradient(135deg, rgba(6, 78, 59, 0.45) 0%, rgba(15, 23, 42, 0.6) 100%)'
          : 'rgba(255, 255, 255, 0.02)',
        border: worldCatalog?.hasAttachedToken
          ? '1px solid rgba(52, 211, 153, 0.4)'
          : '1px solid rgba(255, 255, 255, 0.08)'
      }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 12 }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
            <div style={{
              width: 44,
              height: 44,
              borderRadius: 10,
              background: worldCatalog?.hasAttachedToken ? 'rgba(16, 185, 129, 0.2)' : 'rgba(148, 163, 184, 0.1)',
              border: worldCatalog?.hasAttachedToken ? '1px solid rgba(52, 211, 153, 0.4)' : '1px solid rgba(148, 163, 184, 0.2)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              color: worldCatalog?.hasAttachedToken ? '#34d399' : '#94a3b8'
            }}>
              <Usb size={22} />
            </div>
            <div>
              <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                <h3 style={{ margin: 0, fontSize: 16, fontWeight: 700, color: '#f8fafc' }}>
                  {worldCatalog?.hasAttachedToken
                    ? 'PHYSICAL HARDWARE TOKEN ATTACHED'
                    : 'NO HARDWARE TOKEN ATTACHED'}
                </h3>
                <span className={`badge ${worldCatalog?.hasAttachedToken ? 'badge-success' : 'badge-secondary'}`}>
                  {worldCatalog?.hasAttachedToken ? '● CONNECTED & ACTIVE' : 'NO DEVICE'}
                </span>
              </div>
              <div style={{ fontSize: 13, color: '#94a3b8', marginTop: 2 }}>
                {worldCatalog?.hasAttachedToken
                  ? `${worldCatalog.attachedTokenCount} security token device(s) physically connected to local USB bus.`
                  : 'Insert your USB DSC / Aladdin token into any USB port to sign documents.'}
              </div>
            </div>
          </div>
        </div>

        {worldCatalog?.hasAttachedToken && worldCatalog.attachedTokens && worldCatalog.attachedTokens.length > 0 && (
          <div style={{ display: 'flex', flexDirection: 'column', gap: 10, marginTop: 16 }}>
            {worldCatalog.attachedTokens.map((tok, idx) => (
              <div key={idx} style={{
                background: 'rgba(0, 0, 0, 0.3)',
                border: '1px solid rgba(52, 211, 153, 0.25)',
                borderRadius: 8,
                padding: 14,
                display: 'grid',
                gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))',
                gap: 12
              }}>
                <div>
                  <div style={{ fontSize: 11, color: '#94a3b8', textTransform: 'uppercase' }}>Attached Device</div>
                  <div style={{ fontSize: 14, fontWeight: 700, color: '#f8fafc', marginTop: 2, display: 'flex', alignItems: 'center', gap: 6 }}>
                    <Key size={14} color="#34d399" />
                    {tok.deviceName}
                  </div>
                </div>

                <div>
                  <div style={{ fontSize: 11, color: '#94a3b8', textTransform: 'uppercase' }}>Manufacturer</div>
                  <div style={{ fontSize: 13, fontWeight: 600, color: '#60a5fa', marginTop: 2 }}>
                    {tok.manufacturer}
                  </div>
                </div>

                <div>
                  <div style={{ fontSize: 11, color: '#94a3b8', textTransform: 'uppercase' }}>Hardware ID</div>
                  <div className="mono-text" style={{ fontSize: 12, color: '#cbd5e1', marginTop: 2 }}>
                    {tok.hardwareId}
                  </div>
                </div>

                <div>
                  <div style={{ fontSize: 11, color: '#94a3b8', textTransform: 'uppercase' }}>Connection Details</div>
                  <div style={{ fontSize: 12, color: '#86efac', marginTop: 2 }}>
                    {tok.details}
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {scanMessage && (
        <div style={{
          background: 'rgba(56, 189, 248, 0.1)',
          border: '1px solid rgba(56, 189, 248, 0.3)',
          borderRadius: 8,
          padding: '10px 14px',
          fontSize: 13,
          color: '#bae6fd',
          display: 'flex',
          alignItems: 'center',
          gap: 8
        }}>
          <Info size={16} color="#38bdf8" />
          {scanMessage}
        </div>
      )}

      {/* Featured Spotlight: Aladdin eToken Ecosystem */}
      {aladdinProvider && (
        <div className="glass-card" style={{
          padding: 24,
          background: 'linear-gradient(135deg, rgba(30, 58, 138, 0.45) 0%, rgba(15, 23, 42, 0.6) 100%)',
          border: '1px solid rgba(96, 165, 250, 0.4)',
          position: 'relative',
          overflow: 'hidden'
        }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: 16 }}>
            <div style={{ display: 'flex', gap: 14 }}>
              <div style={{
                width: 48,
                height: 48,
                borderRadius: 12,
                background: 'rgba(59, 130, 246, 0.25)',
                border: '1px solid rgba(96, 165, 250, 0.5)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: '#60a5fa'
              }}>
                <Key size={26} />
              </div>
              <div>
                <div style={{ display: 'flex', alignItems: 'center', gap: 10, flexWrap: 'wrap' }}>
                  <h3 style={{ margin: 0, fontSize: 18, fontWeight: 700, color: '#f8fafc' }}>
                    {aladdinProvider.brandName}
                  </h3>
                  <span className="badge badge-success" style={{ display: 'flex', alignItems: 'center', gap: 5 }}>
                    <ShieldCheck size={13} />
                    Aladdin Certified
                  </span>
                  <span className="badge" style={{ background: 'rgba(56, 189, 248, 0.15)', color: '#38bdf8', border: '1px solid rgba(56, 189, 248, 0.3)' }}>
                    Windows CSP Active
                  </span>
                </div>
                <div style={{ fontSize: 13, color: '#94a3b8', marginTop: 4 }}>
                  Manufacturer: <span style={{ color: '#e2e8f0', fontWeight: 600 }}>{aladdinProvider.manufacturer}</span>
                </div>
              </div>
            </div>

            <div style={{ textAlign: 'right' }}>
              <span className={`badge ${aladdinProvider.readiness === 'READY' ? 'badge-success' : 'badge-warning'}`} style={{ fontSize: 13, padding: '6px 12px' }}>
                {aladdinProvider.readiness === 'READY' ? '● READY FOR SIGNING' : '● CONFIGURING'}
              </span>
            </div>
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))', gap: 16, marginTop: 18 }}>
            <div style={{ background: 'rgba(0, 0, 0, 0.25)', padding: 12, borderRadius: 8, border: '1px solid rgba(255, 255, 255, 0.05)' }}>
              <div style={{ fontSize: 11, color: '#94a3b8', textTransform: 'uppercase' }}>Supported Aladdin Models</div>
              <div style={{ fontSize: 13, color: '#f1f5f9', fontWeight: 500, marginTop: 4 }}>
                {aladdinProvider.modelSupport}
              </div>
            </div>

            <div style={{ background: 'rgba(0, 0, 0, 0.25)', padding: 12, borderRadius: 8, border: '1px solid rgba(255, 255, 255, 0.05)' }}>
              <div style={{ fontSize: 11, color: '#94a3b8', textTransform: 'uppercase' }}>Cryptographic Service Provider (CSP)</div>
              <div style={{ fontSize: 13, color: '#60a5fa', fontWeight: 600, marginTop: 4 }}>
                {aladdinProvider.cspName}
              </div>
            </div>

            <div style={{ background: 'rgba(0, 0, 0, 0.25)', padding: 12, borderRadius: 8, border: '1px solid rgba(255, 255, 255, 0.05)' }}>
              <div style={{ fontSize: 11, color: '#94a3b8', textTransform: 'uppercase' }}>Subsystem Readiness</div>
              <div style={{ fontSize: 12, color: '#86efac', marginTop: 4 }}>
                {aladdinProvider.statusDetails}
              </div>
            </div>
          </div>
        </div>
      )}

      {/* World Provider Catalog Grid */}
      <div>
        <h3 style={{ fontSize: 16, fontWeight: 700, margin: '0 0 14px 0', display: 'flex', alignItems: 'center', gap: 8, color: '#e2e8f0' }}>
          <Zap size={18} color="#eab308" />
          Global Smart Card Hardware Drivers & Detection Matrix
        </h3>

        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))', gap: 16 }}>
          {worldCatalog?.providers.map(prov => {
            const isReady = prov.readiness === 'READY';
            return (
              <div
                key={prov.id}
                className="glass-card"
                style={{
                  padding: 20,
                  display: 'flex',
                  flexDirection: 'column',
                  gap: 12,
                  border: prov.isAladdinFamily ? '1px solid rgba(96, 165, 250, 0.3)' : undefined
                }}
              >
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                  <div>
                    <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                      <h4 style={{ fontSize: 15, fontWeight: 700, margin: 0, color: '#f8fafc' }}>
                        {prov.brandName}
                      </h4>
                      {prov.isAladdinFamily && (
                        <span className="badge" style={{ background: 'rgba(59, 130, 246, 0.2)', color: '#93c5fd', fontSize: 10 }}>
                          Aladdin
                        </span>
                      )}
                    </div>
                    <span style={{ fontSize: 12, color: 'var(--text-dim)' }}>
                      {prov.manufacturer}
                    </span>
                  </div>

                  <span className={`badge ${
                    isReady ? 'badge-success' : 'badge-secondary'
                  }`}>
                    {isReady ? <CheckCircle2 size={12} /> : <AlertCircle size={12} />}
                    <span>{prov.readiness}</span>
                  </span>
                </div>

                <p style={{ margin: 0, fontSize: 12, color: 'var(--text-muted)', lineHeight: 1.4 }}>
                  {prov.description}
                </p>

                <div style={{ fontSize: 12, display: 'flex', flexDirection: 'column', gap: 4, background: 'rgba(0,0,0,0.15)', padding: 10, borderRadius: 6 }}>
                  <div>
                    <span style={{ color: 'var(--text-dim)' }}>CSP: </span>
                    <span style={{ color: prov.cspInstalled ? '#86efac' : '#cbd5e1' }}>
                      {prov.cspName} {prov.cspInstalled && '✓'}
                    </span>
                  </div>
                  <div>
                    <span style={{ color: 'var(--text-dim)' }}>Models: </span>
                    <span style={{ color: '#e2e8f0' }}>{prov.modelSupport}</span>
                  </div>
                  {prov.pkcs11Path && (
                    <div style={{ wordBreak: 'break-all' }}>
                      <span style={{ color: 'var(--text-dim)' }}>PKCS#11: </span>
                      <span className="mono-text" style={{ color: '#93c5fd', fontSize: 11 }}>{prov.pkcs11Path}</span>
                    </div>
                  )}
                </div>

                <div style={{ marginTop: 'auto', paddingTop: 8, fontSize: 11, color: isReady ? '#86efac' : '#94a3b8' }}>
                  {prov.statusDetails}
                </div>
              </div>
            );
          })}
        </div>
      </div>

      {/* Internal Bridge Signing Engines */}
      <div>
        <h3 style={{ fontSize: 16, fontWeight: 700, margin: '0 0 14px 0', display: 'flex', alignItems: 'center', gap: 8, color: '#e2e8f0' }}>
          <Layers size={18} color="#3b82f6" />
          Active Signing Engine Pipelines
        </h3>

        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))', gap: 16 }}>
          {providers.map(prov => {
            const isMock = prov.status === 'MOCK';
            return (
              <div key={prov.id} className="glass-card" style={{ padding: 20, display: 'flex', flexDirection: 'column', gap: 12 }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                    <div style={{
                      padding: 8,
                      borderRadius: 8,
                      background: isMock ? 'rgba(245, 158, 11, 0.15)' : 'rgba(59, 130, 246, 0.15)',
                      color: isMock ? '#fbbf24' : '#60a5fa'
                    }}>
                      {isMock ? <Cpu size={18} /> : <HardDrive size={18} />}
                    </div>
                    <div>
                      <h4 style={{ fontSize: 15, fontWeight: 700, margin: 0, color: '#f8fafc' }}>
                        {prov.name}
                      </h4>
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

                <p style={{ margin: 0, fontSize: 12, color: 'var(--text-muted)', lineHeight: 1.4 }}>
                  {prov.description}
                </p>

                {prov.details && (
                  <div style={{
                    background: 'rgba(255, 255, 255, 0.02)',
                    border: '1px solid rgba(255, 255, 255, 0.06)',
                    borderRadius: 6,
                    padding: 8,
                    fontSize: 11,
                    color: '#93c5fd'
                  }}>
                    {prov.details}
                  </div>
                )}

                <div style={{ marginTop: 'auto', paddingTop: 8, borderTop: '1px solid rgba(255, 255, 255, 0.05)', fontSize: 11 }}>
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
    </div>
  );
};

