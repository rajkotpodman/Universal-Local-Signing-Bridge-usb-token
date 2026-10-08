import React, { useState, useEffect } from 'react';
import { FileText, RefreshCw, CheckCircle2, XCircle, ShieldCheck } from 'lucide-react';
import { AuditEntry } from '../types';
import { fetchAuditLogs } from '../api';

export const AuditLogsView: React.FC = () => {
  const [logs, setLogs] = useState<AuditEntry[]>([]);
  const [loading, setLoading] = useState(true);

  const loadLogs = () => {
    setLoading(true);
    fetchAuditLogs(50)
      .then(setLogs)
      .catch(() => {})
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    loadLogs();
    const interval = setInterval(loadLogs, 5000);
    return () => clearInterval(interval);
  }, []);

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 20 }}>
      {/* Header */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 14 }}>
        <div>
          <h2 style={{ fontSize: 20, fontWeight: 700, margin: '0 0 4px 0', display: 'flex', alignItems: 'center', gap: 10 }}>
            <FileText size={20} color="#3b82f6" />
            Tamper-Evident Audit System
          </h2>
          <p style={{ margin: 0, fontSize: 13, color: 'var(--text-muted)' }}>
            SQLite-backed cryptographic activity logs. Stores SHA-256 payload digests without raw text or credentials.
          </p>
        </div>

        <button onClick={loadLogs} className="btn btn-secondary btn-sm" disabled={loading}>
          <RefreshCw size={14} className={loading ? 'spin' : ''} />
          <span>Refresh</span>
        </button>
      </div>

      <div className="glass-card" style={{ padding: 0, overflow: 'hidden' }}>
        {logs.length === 0 ? (
          <div style={{ padding: 48, textAlign: 'center', color: 'var(--text-dim)' }}>
            No audit log entries recorded yet.
          </div>
        ) : (
          <div style={{ overflowX: 'auto' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: 12 }}>
              <thead>
                <tr style={{ background: 'rgba(255, 255, 255, 0.03)', borderBottom: '1px solid rgba(255, 255, 255, 0.08)' }}>
                  <th style={{ padding: '12px 16px', color: 'var(--text-muted)' }}>Timestamp</th>
                  <th style={{ padding: '12px 16px', color: 'var(--text-muted)' }}>Operation</th>
                  <th style={{ padding: '12px 16px', color: 'var(--text-muted)' }}>Status</th>
                  <th style={{ padding: '12px 16px', color: 'var(--text-muted)' }}>Provider</th>
                  <th style={{ padding: '12px 16px', color: 'var(--text-muted)' }}>Duration</th>
                  <th style={{ padding: '12px 16px', color: 'var(--text-muted)' }}>SHA-256 Digest</th>
                </tr>
              </thead>
              <tbody>
                {logs.map(log => (
                  <tr key={log.id} style={{ borderBottom: '1px solid rgba(255, 255, 255, 0.04)' }}>
                    <td className="mono-text" style={{ padding: '10px 16px', color: 'var(--text-muted)', whiteSpace: 'nowrap' }}>
                      {new Date(log.timestamp).toLocaleTimeString()}
                    </td>
                    <td style={{ padding: '10px 16px', fontWeight: 600 }}>
                      <span className="mono-text" style={{ background: 'rgba(59, 130, 246, 0.1)', color: '#60a5fa', padding: '2px 6px', borderRadius: 4 }}>
                        {log.operation}
                      </span>
                    </td>
                    <td style={{ padding: '10px 16px' }}>
                      <span className={`badge ${log.success ? 'badge-success' : 'badge-danger'}`}>
                        {log.success ? <CheckCircle2 size={11} /> : <XCircle size={11} />}
                        <span>{log.success ? 'SUCCESS' : 'FAILED'}</span>
                      </span>
                    </td>
                    <td style={{ padding: '10px 16px', color: '#e2e8f0' }}>
                      {log.provider}
                    </td>
                    <td className="mono-text" style={{ padding: '10px 16px', color: '#34d399' }}>
                      {Math.round(log.durationMs)} ms
                    </td>
                    <td className="mono-text" style={{ padding: '10px 16px', color: '#a78bfa' }}>
                      {log.payloadDigestSha256 ? `${log.payloadDigestSha256.slice(0, 16)}...` : '—'}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
};
