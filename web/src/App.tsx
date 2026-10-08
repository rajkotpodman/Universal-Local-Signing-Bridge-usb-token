import React, { useState, useEffect } from 'react';
import { Sidebar } from './components/Sidebar';
import { TopBar } from './components/TopBar';
import { DashboardView } from './pages/DashboardView';
import { CertificatesView } from './pages/CertificatesView';
import { ProvidersView } from './pages/ProvidersView';
import { SigningTestView } from './pages/SigningTestView';
import { BrowserDiagnosticsView } from './pages/BrowserDiagnosticsView';
import { SecurityView } from './pages/SecurityView';
import { AuditLogsView } from './pages/AuditLogsView';
import { SettingsView } from './pages/SettingsView';
import { AboutView } from './pages/AboutView';
import { BridgeStatus, ProviderInfo, Certificate } from './types';
import { fetchStatus, fetchProviders, fetchCertificates } from './api';

export function App() {
  const [activeTab, setActiveTab] = useState<string>(() => {
    const path = window.location.pathname.toLowerCase();
    if (path.includes('diagnostic')) return 'diagnostics';
    if (path.includes('sign')) return 'sign';
    if (path.includes('cert')) return 'certificates';
    return 'dashboard';
  });

  const [status, setStatus] = useState<BridgeStatus | null>(null);
  const [providers, setProviders] = useState<ProviderInfo[]>([]);
  const [certificates, setCertificates] = useState<Certificate[]>([]);
  const [loading, setLoading] = useState(true);
  const [wsConnected, setWsConnected] = useState(false);
  const [selectedCertForSign, setSelectedCertForSign] = useState<string>('');

  const refreshAll = async () => {
    setLoading(true);
    try {
      const [st, provs, certs] = await Promise.allSettled([
        fetchStatus(),
        fetchProviders(),
        fetchCertificates()
      ]);

      if (st.status === 'fulfilled') setStatus(st.value);
      if (provs.status === 'fulfilled') setProviders(provs.value);
      if (certs.status === 'fulfilled') setCertificates(certs.value);
    } catch { }
    finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    refreshAll();
    const interval = setInterval(refreshAll, 10000);
    return () => clearInterval(interval);
  }, []);

  // WebSocket Live Listener
  useEffect(() => {
    const wsUrl = `ws://${window.location.hostname}:8080/ws/v1`;
    let ws: WebSocket;

    try {
      ws = new WebSocket(wsUrl);
      ws.onopen = () => setWsConnected(true);
      ws.onclose = () => setWsConnected(false);
      ws.onerror = () => setWsConnected(false);
      ws.onmessage = (event) => {
        try {
          const data = JSON.parse(event.data);
          if (data.event === 'certificate_changed' || data.event === 'sign_completed') {
            refreshAll();
          }
        } catch { }
      };
    } catch { }

    return () => {
      try { ws?.close(); } catch { }
    };
  }, []);

  const handleSelectCertSign = (certId: string) => {
    setSelectedCertForSign(certId);
    setActiveTab('sign');
  };

  return (
    <div style={{ display: 'flex', minHeight: '100vh', background: 'var(--bg-main)' }}>
      {/* Sidebar */}
      <Sidebar
        activeTab={activeTab}
        onSelectTab={setActiveTab}
        mockMode={status?.mockMode ?? true}
      />

      {/* Main Content Area */}
      <div style={{ flex: 1, display: 'flex', flexDirection: 'column', minWidth: 0 }}>
        <TopBar
          status={status}
          loading={loading}
          onRefresh={refreshAll}
          wsConnected={wsConnected}
        />

        <main style={{ flex: 1, padding: '28px 32px', overflowY: 'auto' }}>
          {activeTab === 'dashboard' && (
            <DashboardView
              status={status}
              providers={providers}
              certificates={certificates}
              onNavigate={setActiveTab}
              wsConnected={wsConnected}
            />
          )}

          {activeTab === 'certificates' && (
            <CertificatesView
              certificates={certificates}
              onRefresh={refreshAll}
              loading={loading}
              onSelectSign={handleSelectCertSign}
            />
          )}

          {activeTab === 'providers' && (
            <ProvidersView providers={providers} />
          )}

          {activeTab === 'sign' && (
            <SigningTestView
              certificates={certificates}
              selectedCertId={selectedCertForSign}
            />
          )}

          {activeTab === 'diagnostics' && (
            <BrowserDiagnosticsView />
          )}

          {activeTab === 'security' && (
            <SecurityView />
          )}

          {activeTab === 'audit' && (
            <AuditLogsView />
          )}

          {activeTab === 'settings' && (
            <SettingsView status={status} />
          )}

          {activeTab === 'about' && (
            <AboutView />
          )}
        </main>
      </div>
    </div>
  );
}

export default App;
