import React from 'react';
import { 
  LayoutDashboard, 
  KeyRound, 
  Layers, 
  PenTool, 
  Stethoscope, 
  ShieldCheck, 
  FileText, 
  Settings, 
  Info,
  Shield
} from 'lucide-react';

interface SidebarProps {
  activeTab: string;
  onSelectTab: (tab: string) => void;
  mockMode: boolean;
}

export const Sidebar: React.FC<SidebarProps> = ({ activeTab, onSelectTab, mockMode }) => {
  const menuItems = [
    { id: 'dashboard', label: 'Dashboard', icon: LayoutDashboard },
    { id: 'certificates', label: 'Certificates', icon: KeyRound },
    { id: 'providers', label: 'Providers', icon: Layers },
    { id: 'sign', label: 'Signing Test', icon: PenTool },
    { id: 'diagnostics', label: 'Browser Diagnostics', icon: Stethoscope },
    { id: 'security', label: 'Security', icon: ShieldCheck },
    { id: 'audit', label: 'Audit Logs', icon: FileText },
    { id: 'settings', label: 'Settings', icon: Settings },
    { id: 'about', label: 'About', icon: Info },
  ];

  return (
    <aside style={{
      width: 260,
      background: 'var(--bg-sidebar)',
      borderRight: '1px solid var(--border-card)',
      display: 'flex',
      flexDirection: 'column',
      padding: '24px 16px',
      gap: 20
    }}>
      {/* Brand Header */}
      <div style={{ display: 'flex', alignItems: 'center', gap: 12, padding: '0 8px' }}>
        <div style={{
          width: 38,
          height: 38,
          borderRadius: 10,
          background: 'linear-gradient(135deg, #2563eb, #6366f1)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          boxShadow: '0 0 15px rgba(59, 130, 246, 0.35)'
        }}>
          <Shield size={20} color="#fff" />
        </div>
        <div>
          <h2 style={{ fontSize: 15, fontWeight: 800, letterSpacing: '-0.02em', margin: 0 }}>
            Universal Bridge
          </h2>
          <span style={{ fontSize: 11, color: 'var(--text-dim)' }}>Localhost PKI Engine</span>
        </div>
      </div>

      {/* Mode Tag */}
      {mockMode ? (
        <div style={{
          background: 'rgba(245, 158, 11, 0.1)',
          border: '1px solid rgba(245, 158, 11, 0.25)',
          borderRadius: 8,
          padding: '8px 12px',
          fontSize: 11,
          color: '#fbbf24',
          fontWeight: 700,
          display: 'flex',
          alignItems: 'center',
          gap: 6
        }}>
          <span className="pulse-dot" />
          <span>DEVELOPMENT MOCK PROVIDER</span>
        </div>
      ) : (
        <div style={{
          background: 'rgba(16, 185, 129, 0.1)',
          border: '1px solid rgba(16, 185, 129, 0.25)',
          borderRadius: 8,
          padding: '8px 12px',
          fontSize: 11,
          color: '#34d399',
          fontWeight: 700,
          display: 'flex',
          alignItems: 'center',
          gap: 6
        }}>
          <span className="pulse-dot" />
          <span>HARDWARE TOKEN READY</span>
        </div>
      )}

      {/* Navigation Links */}
      <nav style={{ display: 'flex', flexDirection: 'column', gap: 4, flex: 1 }}>
        {menuItems.map(item => {
          const Icon = item.icon;
          const active = activeTab === item.id;

          return (
            <button
              key={item.id}
              onClick={() => onSelectTab(item.id)}
              style={{
                display: 'flex',
                alignItems: 'center',
                gap: 12,
                padding: '10px 14px',
                borderRadius: 8,
                border: 'none',
                background: active ? 'rgba(59, 130, 246, 0.15)' : 'transparent',
                color: active ? '#60a5fa' : 'var(--text-muted)',
                fontWeight: active ? 700 : 500,
                fontSize: 13,
                cursor: 'pointer',
                textAlign: 'left',
                transition: 'all 0.15s ease'
              }}
            >
              <Icon size={17} color={active ? '#60a5fa' : 'var(--text-dim)'} />
              <span>{item.label}</span>
            </button>
          );
        })}
      </nav>

      <div style={{ fontSize: 11, color: 'var(--text-dim)', textAlign: 'center', paddingTop: 12, borderTop: '1px solid var(--border-card)' }}>
        v1.0.0 • 127.0.0.1:8080
      </div>
    </aside>
  );
};
