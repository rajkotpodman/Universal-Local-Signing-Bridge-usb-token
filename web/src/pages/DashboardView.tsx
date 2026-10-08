import React from 'react';
import { 
  Server, 
  Layers, 
  KeyRound, 
  PenTool, 
  Activity, 
  ArrowRight, 
  Usb, 
  ShieldCheck, 
  Globe, 
  Cpu
} from 'lucide-react';
import { BridgeStatus, ProviderInfo, Certificate } from '../types';

interface DashboardViewProps {
  status: BridgeStatus | null;
  providers: ProviderInfo[];
  certificates: Certificate[];
  onNavigate: (tab: string) => void;
  wsConnected: boolean;
}

export const DashboardView: React.FC<DashboardViewProps> = ({
  status,
  providers,
  certificates,
  onNavigate,
  wsConnected
}) => {
  const isMock = status?.mockMode ?? true;
  const availableProviders = providers.filter(p => p.status === 'AVAILABLE' || p.status === 'MOCK');

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 24 }}>
      {/* Banner */}
      <div className="glass-card" style={{
        padding: '24px 28px',
        background: 'linear-gradient(135deg, rgba(37, 99, 235, 0.15) 0%, rgba(99, 102, 241, 0.1) 100%)',
        border: '1px solid rgba(59, 130, 246, 0.25)',
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        flexWrap: 'wrap',
        gap: 20
      }}>
        <div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginBottom: 8 }}>
            <span className={isMock ? 'badge badge-warning' : 'badge badge-success'}>
              {isMock ? 'DEVELOPMENT MOCK PROVIDER ACTIVE' : 'WINDOWS HARDWARE TOKEN MODE'}
            </span>
            <span style={{ fontSize: 13, color: 'var(--text-dim)' }}>
              Bound strictly to 127.0.0.1:{status?.port || 8080}
            </span>
          </div>
          <h2 style={{ fontSize: 22, fontWeight: 800, margin: '0 0 6px 0' }}>
            Universal Local Signing Bridge
          </h2>
          <p style={{ margin: 0, color: 'var(--text-muted)', fontSize: 13, maxWidth: 650, lineHeight: 1.5 }}>
            High-performance localhost middleware connecting browser applications to cryptographic hardware tokens without browser extensions or NPAPI plugins.
          </p>
        </div>

        <div style={{ display: 'flex', gap: 10 }}>
          <button onClick={() => onNavigate('diagnostics')} className="btn btn-primary">
            <Activity size={15} />
            <span>Browser Diagnostics</span>
          </button>
          <button onClick={() => onNavigate('sign')} className="btn btn-secondary">
            <PenTool size={15} />
            <span>Signing Test</span>
          </button>
        </div>
      </div>

      {/* 6 Core Metric Cards */}
      <div style={{
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))',
        gap: 16
      }}>
        {/* Metric 1: Service */}
        <div className="glass-card" style={{ padding: 18, cursor: 'pointer' }} onClick={() => onNavigate('settings')}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 10 }}>
            <span style={{ fontSize: 12, color: 'var(--text-dim)', fontWeight: 600 }}>SERVICE</span>
            <Server size={16} color="#3b82f6" />
          </div>
          <div style={{ fontSize: 20, fontWeight: 800, color: '#10b981', display: 'flex', alignItems: 'center', gap: 6 }}>
            <span className="pulse-dot" />
            <span>Running</span>
          </div>
          <span style={{ fontSize: 11, color: 'var(--text-dim)', marginTop: 4, display: 'block' }}>
            Port {status?.port || 8080}
          </span>
        </div>

        {/* Metric 2: API */}
        <div className="glass-card" style={{ padding: 18, cursor: 'pointer' }} onClick={() => onNavigate('diagnostics')}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 10 }}>
            <span style={{ fontSize: 12, color: 'var(--text-dim)', fontWeight: 600 }}>API</span>
            <Activity size={16} color="#6366f1" />
          </div>
          <div style={{ fontSize: 20, fontWeight: 800, color: '#60a5fa', display: 'flex', alignItems: 'center', gap: 6 }}>
            <span>Available</span>
          </div>
          <span style={{ fontSize: 11, color: 'var(--text-dim)', marginTop: 4, display: 'block' }}>
            REST v1 Endpoints
          </span>
        </div>

        {/* Metric 3: WebSocket */}
        <div className="glass-card" style={{ padding: 18, cursor: 'pointer' }} onClick={() => onNavigate('diagnostics')}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 10 }}>
            <span style={{ fontSize: 12, color: 'var(--text-dim)', fontWeight: 600 }}>WEBSOCKET</span>
            <Globe size={16} color="#06b6d4" />
          </div>
          <div style={{ fontSize: 20, fontWeight: 800, color: wsConnected ? '#10b981' : '#f59e0b', display: 'flex', alignItems: 'center', gap: 6 }}>
            <span>{wsConnected ? 'Connected' : 'Connecting'}</span>
          </div>
          <span style={{ fontSize: 11, color: 'var(--text-dim)', marginTop: 4, display: 'block' }}>
            /ws/v1 Live Stream
          </span>
        </div>

        {/* Metric 4: Certificates */}
        <div className="glass-card" style={{ padding: 18, cursor: 'pointer' }} onClick={() => onNavigate('certificates')}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 10 }}>
            <span style={{ fontSize: 12, color: 'var(--text-dim)', fontWeight: 600 }}>CERTIFICATES</span>
            <KeyRound size={16} color="#10b981" />
          </div>
          <div style={{ fontSize: 20, fontWeight: 800, color: '#f8fafc' }}>
            {certificates.length} detected
          </div>
          <span style={{ fontSize: 11, color: 'var(--text-dim)', marginTop: 4, display: 'block' }}>
            {certificates.filter(c => c.smartCardBacked).length} Smart Card tokens
          </span>
        </div>

        {/* Metric 5: Providers */}
        <div className="glass-card" style={{ padding: 18, cursor: 'pointer' }} onClick={() => onNavigate('providers')}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 10 }}>
            <span style={{ fontSize: 12, color: 'var(--text-dim)', fontWeight: 600 }}>PROVIDERS</span>
            <Layers size={16} color="#8b5cf6" />
          </div>
          <div style={{ fontSize: 20, fontWeight: 800, color: '#f8fafc' }}>
            {availableProviders.length} available
          </div>
          <span style={{ fontSize: 11, color: 'var(--text-dim)', marginTop: 4, display: 'block' }}>
            {providers.length} total registered
          </span>
        </div>

        {/* Metric 6: Signing */}
        <div className="glass-card" style={{ padding: 18, cursor: 'pointer' }} onClick={() => onNavigate('sign')}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 10 }}>
            <span style={{ fontSize: 12, color: 'var(--text-dim)', fontWeight: 600 }}>SIGNING</span>
            <PenTool size={16} color="#f59e0b" />
          </div>
          <div style={{ fontSize: 20, fontWeight: 800, color: '#34d399' }}>
            Ready
          </div>
          <span style={{ fontSize: 11, color: 'var(--text-dim)', marginTop: 4, display: 'block' }}>
            {status?.totalSignOperations || 0} operations executed
          </span>
        </div>
      </div>

      {/* End-to-End Visual Architecture */}
      <div className="glass-card" style={{ padding: 24 }}>
        <h3 style={{ fontSize: 15, fontWeight: 700, marginBottom: 18, display: 'flex', alignItems: 'center', gap: 8 }}>
          <Cpu size={17} color="#60a5fa" />
          Cryptographic Hardware Pipeline
        </h3>

        <div style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))',
          gap: 12,
          alignItems: 'center'
        }}>
          <div style={{ background: '#070b14', border: '1px solid var(--border-card)', borderRadius: 10, padding: 14, textAlign: 'center' }}>
            <Usb size={24} color="#3b82f6" style={{ margin: '0 auto 6px' }} />
            <div style={{ fontSize: 13, fontWeight: 700 }}>USB Token / DSC</div>
            <div style={{ fontSize: 11, color: 'var(--text-dim)', marginTop: 2 }}>Non-exportable Key Container</div>
          </div>

          <div style={{ textAlign: 'center', color: '#475569' }}>
            <ArrowRight size={18} style={{ margin: '0 auto' }} />
          </div>

          <div style={{ background: '#070b14', border: '1px solid var(--border-card)', borderRadius: 10, padding: 14, textAlign: 'center' }}>
            <Layers size={24} color="#8b5cf6" style={{ margin: '0 auto 6px' }} />
            <div style={{ fontSize: 13, fontWeight: 700 }}>Crypto Provider</div>
            <div style={{ fontSize: 11, color: 'var(--text-dim)', marginTop: 2 }}>Windows CNG / PKCS#11</div>
          </div>

          <div style={{ textAlign: 'center', color: '#475569' }}>
            <ArrowRight size={18} style={{ margin: '0 auto' }} />
          </div>

          <div style={{ background: '#070b14', border: '1px solid rgba(59, 130, 246, 0.4)', borderRadius: 10, padding: 14, textAlign: 'center' }}>
            <ShieldCheck size={24} color="#10b981" style={{ margin: '0 auto 6px' }} />
            <div style={{ fontSize: 13, fontWeight: 700 }}>Local Signing Bridge</div>
            <div style={{ fontSize: 11, color: 'var(--text-dim)', marginTop: 2 }}>127.0.0.1:{status?.port || 8080}</div>
          </div>

          <div style={{ textAlign: 'center', color: '#475569' }}>
            <ArrowRight size={18} style={{ margin: '0 auto' }} />
          </div>

          <div style={{ background: '#070b14', border: '1px solid var(--border-card)', borderRadius: 10, padding: 14, textAlign: 'center' }}>
            <Globe size={24} color="#06b6d4" style={{ margin: '0 auto 6px' }} />
            <div style={{ fontSize: 13, fontWeight: 700 }}>Browser Portal</div>
            <div style={{ fontSize: 11, color: 'var(--text-dim)', marginTop: 2 }}>Web Apps & Forms</div>
          </div>
        </div>
      </div>
    </div>
  );
};
