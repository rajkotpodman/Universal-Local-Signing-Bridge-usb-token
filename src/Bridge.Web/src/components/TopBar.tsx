import React from 'react';
import { RefreshCw, Activity, CheckCircle2 } from 'lucide-react';
import { BridgeStatus } from '../types';

interface TopBarProps {
  status: BridgeStatus | null;
  loading: boolean;
  onRefresh: () => void;
  wsConnected: boolean;
}

export const TopBar: React.FC<TopBarProps> = ({ status, loading, onRefresh, wsConnected }) => {
  return (
    <header style={{
      height: 64,
      borderBottom: '1px solid var(--border-card)',
      background: 'rgba(8, 12, 20, 0.8)',
      backdropFilter: 'blur(16px)',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'space-between',
      padding: '0 32px'
    }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: 14 }}>
        <h1 style={{ fontSize: 17, fontWeight: 700, margin: 0 }}>
          Universal Local Signing Bridge
        </h1>
        <span style={{ fontSize: 12, color: 'var(--text-dim)', fontFamily: 'var(--font-mono)' }}>
          http://127.0.0.1:{status?.port || 8080}
        </span>
      </div>

      <div style={{ display: 'flex', alignItems: 'center', gap: 14 }}>
        {/* Metric Badges */}
        <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
          <div className="badge badge-success">
            <span className="pulse-dot" />
            <span>SERVICE: Running</span>
          </div>

          <div className="badge badge-info">
            <CheckCircle2 size={12} />
            <span>API: Available</span>
          </div>

          <div className={`badge ${wsConnected ? 'badge-success' : 'badge-warning'}`}>
            <Activity size={12} />
            <span>WS: {wsConnected ? 'Connected' : 'Connecting'}</span>
          </div>
        </div>

        <button
          onClick={onRefresh}
          className="btn btn-secondary btn-sm"
          disabled={loading}
          title="Refresh All"
        >
          <RefreshCw size={13} className={loading ? 'spin' : ''} />
          <span>Sync</span>
        </button>
      </div>
    </header>
  );
};
