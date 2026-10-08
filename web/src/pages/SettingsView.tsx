import React from 'react';
import { Settings, Cpu, HardDrive, Usb, Layers, Terminal } from 'lucide-react';
import { BridgeStatus } from '../types';

interface SettingsViewProps {
  status: BridgeStatus | null;
}

export const SettingsView: React.FC<SettingsViewProps> = ({ status }) => {
  const isMock = status?.mockMode ?? true;

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 20 }}>
      {/* Header */}
      <div>
        <h2 style={{ fontSize: 20, fontWeight: 700, margin: '0 0 4px 0', display: 'flex', alignItems: 'center', gap: 10 }}>
          <Settings size={20} color="#3b82f6" />
          Bridge Configuration & Provider Mode
        </h2>
        <p style={{ margin: 0, fontSize: 13, color: 'var(--text-muted)' }}>
          Environment operational mode, USB token hardware guidelines, and Windows startup services.
        </p>
      </div>

      {/* Mode Card */}
      <div className="glass-card" style={{ padding: 22 }}>
        <h3 style={{ fontSize: 15, fontWeight: 700, margin: '0 0 14px 0', display: 'flex', alignItems: 'center', gap: 8 }}>
          {isMock ? <Cpu size={18} color="#f59e0b" /> : <HardDrive size={18} color="#10b981" />}
          Operating Mode Configuration
        </h3>

        <div style={{
          padding: 16,
          borderRadius: 10,
          background: isMock ? 'rgba(245, 158, 11, 0.08)' : 'rgba(16, 185, 129, 0.08)',
          border: `1px solid ${isMock ? 'rgba(245, 158, 11, 0.25)' : 'rgba(16, 185, 129, 0.25)'}`,
          marginBottom: 16
        }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginBottom: 6 }}>
            <span style={{ fontSize: 15, fontWeight: 800, color: isMock ? '#fbbf24' : '#34d399' }}>
              {isMock ? 'MOCK MODE (MOCK_MODE=true)' : 'WINDOWS HARDWARE TOKEN MODE (MOCK_MODE=false)'}
            </span>
          </div>
          <p style={{ margin: 0, fontSize: 13, color: 'var(--text-muted)', lineHeight: 1.5 }}>
            {isMock
              ? 'Software simulation using in-memory RSA 2048/4096 and ECDSA keys. Ideal for local web development, headless CI pipelines, and environments without physical USB hardware.'
              : 'Directly interfacing with Windows Certificate Store (CurrentUser\\My) and native USB token CSP/KSP drivers.'}
          </p>
        </div>

        <div style={{ fontSize: 13, color: 'var(--text-muted)' }}>
          To switch modes, set the environment variable prior to launching the bridge:
          <pre style={{
            background: '#060911',
            border: '1px solid rgba(255, 255, 255, 0.08)',
            borderRadius: 8,
            padding: 12,
            marginTop: 8,
            fontSize: 12,
            color: '#cbd5e1'
          }}>
$env:MOCK_MODE = "{isMock ? 'false' : 'true'}"
$env:BRIDGE_MODE = "{isMock ? 'WINDOWS' : 'MOCK'}"
dotnet run --project src/Bridge.Api
          </pre>
        </div>
      </div>

      {/* USB Token Hardware Setup */}
      <div className="glass-card" style={{ padding: 22 }}>
        <h3 style={{ fontSize: 15, fontWeight: 700, margin: '0 0 14px 0', display: 'flex', alignItems: 'center', gap: 8 }}>
          <Usb size={18} color="#60a5fa" />
          USB DSC / Smart Card Hardware Integration Steps
        </h3>

        <div style={{ display: 'flex', flexDirection: 'column', gap: 12, fontSize: 13, color: 'var(--text-muted)' }}>
          <div>1. <strong>Install Manufacturer PKI Middleware</strong> (e.g. ePass2003 PKI Client, Watchdata ProxKey, SafeNet SAC).</div>
          <div>2. <strong>Plug in USB Token</strong> into physical port on Windows 10/11.</div>
          <div>3. <strong>Verify in Windows Store</strong>: Run <code style={{ color: '#93c5fd' }}>certmgr.msc</code> & inspect Personal &gt; Certificates.</div>
          <div>4. <strong>Launch Bridge in Windows Mode</strong> (<code style={{ color: '#93c5fd' }}>$env:MOCK_MODE="false"</code>).</div>
          <div>5. <strong>Sign Request</strong>: When browser requests a signature, Windows displays the native PIN dialog.</div>
        </div>
      </div>
    </div>
  );
};
