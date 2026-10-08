import React, { useState } from 'react';
import { RefreshCw, Activity, CheckCircle2, Globe, HelpCircle, X, ShieldCheck, Key, FileCheck } from 'lucide-react';
import { BridgeStatus } from '../types';
import { useLanguage } from '../context/LanguageContext';

interface TopBarProps {
  status: BridgeStatus | null;
  loading: boolean;
  onRefresh: () => void;
  wsConnected: boolean;
}

export const TopBar: React.FC<TopBarProps> = ({ status, loading, onRefresh, wsConnected }) => {
  const { language, setLanguage, t } = useLanguage();
  const [showHelpModal, setShowHelpModal] = useState(false);

  return (
    <>
      <header style={{
        height: 64,
        borderBottom: '1px solid var(--border-card)',
        background: 'rgba(8, 12, 20, 0.85)',
        backdropFilter: 'blur(16px)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        padding: '0 24px',
        position: 'sticky',
        top: 0,
        zIndex: 40
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 14 }}>
          <h1 style={{ fontSize: 16, fontWeight: 700, margin: 0, color: '#f8fafc' }}>
            {t('bridgeTitle')}
          </h1>
          <span style={{
            fontSize: 12,
            color: 'var(--text-dim)',
            fontFamily: 'var(--font-mono)',
            background: 'rgba(255, 255, 255, 0.04)',
            padding: '3px 8px',
            borderRadius: 6
          }}>
            127.0.0.1:{status?.port || 8080}
          </span>
        </div>

        <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
          {/* Status Badges */}
          <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
            <div className="badge badge-success" style={{ fontSize: 11 }}>
              <span className="pulse-dot" />
              <span>{t('serviceRunning')}</span>
            </div>

            <div className={`badge ${wsConnected ? 'badge-success' : 'badge-warning'}`} style={{ fontSize: 11 }}>
              <Activity size={11} />
              <span>WS: {wsConnected ? 'Live' : 'Syncing'}</span>
            </div>
          </div>

          {/* Language Switcher */}
          <div style={{
            display: 'flex',
            background: 'rgba(255, 255, 255, 0.06)',
            borderRadius: 8,
            padding: 2,
            border: '1px solid rgba(255, 255, 255, 0.08)'
          }}>
            <button
              type="button"
              onClick={() => setLanguage('en')}
              style={{
                background: language === 'en' ? '#2563eb' : 'transparent',
                color: language === 'en' ? '#fff' : 'var(--text-muted)',
                border: 'none',
                borderRadius: 6,
                padding: '4px 8px',
                fontSize: 12,
                fontWeight: 600,
                cursor: 'pointer',
                display: 'flex',
                alignItems: 'center',
                gap: 4
              }}
            >
              <span>🇺🇸</span> EN
            </button>
            <button
              type="button"
              onClick={() => setLanguage('gu')}
              style={{
                background: language === 'gu' ? '#2563eb' : 'transparent',
                color: language === 'gu' ? '#fff' : 'var(--text-muted)',
                border: 'none',
                borderRadius: 6,
                padding: '4px 8px',
                fontSize: 12,
                fontWeight: 600,
                cursor: 'pointer',
                display: 'flex',
                alignItems: 'center',
                gap: 4
              }}
            >
              <span>🇮🇳</span> ગુજ
            </button>
          </div>

          {/* Quick Help Modal Trigger */}
          <button
            onClick={() => setShowHelpModal(true)}
            className="btn btn-secondary btn-sm"
            style={{ display: 'flex', alignItems: 'center', gap: 6, padding: '6px 12px', fontSize: 12 }}
            title="Quick User Guide"
          >
            <HelpCircle size={14} color="#38bdf8" />
            <span>{t('quickHelp')}</span>
          </button>

          {/* Refresh Action */}
          <button
            onClick={onRefresh}
            className="btn btn-secondary btn-sm"
            disabled={loading}
            style={{ padding: '6px 10px' }}
            title="Refresh All"
          >
            <RefreshCw size={13} className={loading ? 'spin' : ''} />
          </button>
        </div>
      </header>

      {/* Quick Help Modal */}
      {showHelpModal && (
        <div style={{
          position: 'fixed',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          background: 'rgba(0, 0, 0, 0.75)',
          backdropFilter: 'blur(8px)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          zIndex: 100,
          padding: 20
        }}>
          <div className="glass-card" style={{
            maxWidth: 600,
            width: '100%',
            padding: 28,
            background: '#0d1322',
            border: '1px solid rgba(59, 130, 246, 0.4)',
            boxShadow: '0 20px 50px rgba(0, 0, 0, 0.6)',
            position: 'relative',
            maxHeight: '90vh',
            overflowY: 'auto'
          }}>
            <button
              onClick={() => setShowHelpModal(false)}
              style={{
                position: 'absolute',
                top: 18,
                right: 18,
                background: 'rgba(255, 255, 255, 0.05)',
                border: 'none',
                color: '#94a3b8',
                borderRadius: '50%',
                width: 32,
                height: 32,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                cursor: 'pointer'
              }}
            >
              <X size={18} />
            </button>

            <h2 style={{ fontSize: 20, fontWeight: 800, margin: '0 0 6px 0', color: '#f8fafc', display: 'flex', alignItems: 'center', gap: 10 }}>
              <ShieldCheck size={24} color="#38bdf8" />
              {language === 'gu' ? 'સરળ ઉપયોગ માર્ગદર્શિકા' : 'Quick How-To-Use Guide'}
            </h2>
            <p style={{ margin: '0 0 20px 0', fontSize: 13, color: 'var(--text-muted)' }}>
              {language === 'gu'
                ? 'કોઈપણ ટેકનિકલ જ્ઞાન વગર માત્ર ૪ સરળ પગલાંમાં કોઈપણ દસ્તાવેજ પર ડિજિટલ સહી કરો.'
                : 'Digitally sign any PDF or document in 4 simple steps without technical knowledge.'}
            </p>

            <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
              {/* Step 1 */}
              <div style={{
                background: 'rgba(255, 255, 255, 0.03)',
                border: '1px solid rgba(255, 255, 255, 0.08)',
                borderRadius: 10,
                padding: 14,
                display: 'flex',
                gap: 12
              }}>
                <div style={{ width: 32, height: 32, borderRadius: 8, background: 'rgba(59, 130, 246, 0.2)', color: '#60a5fa', display: 'flex', alignItems: 'center', justifyContent: 'center', fontWeight: 700 }}>
                  1
                </div>
                <div>
                  <h4 style={{ margin: '0 0 4px 0', fontSize: 14, color: '#f8fafc' }}>
                    {language === 'gu' ? 'USB ટોકન કમ્પ્યુટરમાં લગાવો' : 'Plug in your USB Token'}
                  </h4>
                  <p style={{ margin: 0, fontSize: 12, color: 'var(--text-muted)' }}>
                    {language === 'gu'
                      ? 'તમારી DSC પેનડ્રાઈવ (Aladdin eToken, ePass2003, ProxKey, mToken) ને કમ્પ્યુટરના USB પોર્ટમાં લગાવો.'
                      : 'Insert your USB DSC smart-card key into any USB slot on your computer.'}
                  </p>
                </div>
              </div>

              {/* Step 2 */}
              <div style={{
                background: 'rgba(255, 255, 255, 0.03)',
                border: '1px solid rgba(255, 255, 255, 0.08)',
                borderRadius: 10,
                padding: 14,
                display: 'flex',
                gap: 12
              }}>
                <div style={{ width: 32, height: 32, borderRadius: 8, background: 'rgba(16, 185, 129, 0.2)', color: '#34d399', display: 'flex', alignItems: 'center', justifyContent: 'center', fontWeight: 700 }}>
                  2
                </div>
                <div>
                  <h4 style={{ margin: '0 0 4px 0', fontSize: 14, color: '#f8fafc' }}>
                    {language === 'gu' ? 'બ્રિજ સોફ્ટવેર શરૂ કરો' : 'Start the Bridge Service'}
                  </h4>
                  <p style={{ margin: 0, fontSize: 12, color: 'var(--text-muted)' }}>
                    {language === 'gu'
                      ? 'ફોલ્ડરમાં આવેલ "run.bat" ફાઇલ પર ડબલ-ક્લિક કરો. તે આપમેળે શરૂ થઈને બ્રાઉઝર ખોલશે.'
                      : 'Double-click "run.bat" in the project folder to start the service and auto-open this dashboard.'}
                  </p>
                </div>
              </div>

              {/* Step 3 */}
              <div style={{
                background: 'rgba(255, 255, 255, 0.03)',
                border: '1px solid rgba(255, 255, 255, 0.08)',
                borderRadius: 10,
                padding: 14,
                display: 'flex',
                gap: 12
              }}>
                <div style={{ width: 32, height: 32, borderRadius: 8, background: 'rgba(245, 158, 11, 0.2)', color: '#fbbf24', display: 'flex', alignItems: 'center', justifyContent: 'center', fontWeight: 700 }}>
                  3
                </div>
                <div>
                  <h4 style={{ margin: '0 0 4px 0', fontSize: 14, color: '#f8fafc' }}>
                    {language === 'gu' ? 'PDF ફાઇલ પસંદ કરો' : 'Drop or Choose your PDF File'}
                  </h4>
                  <p style={{ margin: 0, fontSize: 12, color: 'var(--text-muted)' }}>
                    {language === 'gu'
                      ? 'ડેશબોર્ડ પર "૧-ક્લિક ઝડપી પીડીએફ સહી" બોક્સમાં તમારી PDF ફાઇલ ખેંચીને મૂકો અથવા પસંદ કરો.'
                      : 'Drag & drop any invoice or document into the "1-Click Quick PDF Signer" box on the dashboard.'}
                  </p>
                </div>
              </div>

              {/* Step 4 */}
              <div style={{
                background: 'rgba(255, 255, 255, 0.03)',
                border: '1px solid rgba(255, 255, 255, 0.08)',
                borderRadius: 10,
                padding: 14,
                display: 'flex',
                gap: 12
              }}>
                <div style={{ width: 32, height: 32, borderRadius: 8, background: 'rgba(168, 85, 247, 0.2)', color: '#c084fc', display: 'flex', alignItems: 'center', justifyContent: 'center', fontWeight: 700 }}>
                  4
                </div>
                <div>
                  <h4 style={{ margin: '0 0 4px 0', fontSize: 14, color: '#f8fafc' }}>
                    {language === 'gu' ? 'સહી કરો અને ડાઉનલોડ કરો' : 'Sign & Download'}
                  </h4>
                  <p style={{ margin: 0, fontSize: 12, color: 'var(--text-muted)' }}>
                    {language === 'gu'
                      ? '"ટોકન વડે સહી કરો" દબાવો. વિન્ડોઝમાં ટોકન પિન દાખલ કરો અને સહી થયેલી નવી પીડીએફ ડાઉનલોડ થઈ જશે!'
                      : 'Click "Sign Document with Token", enter your hardware PIN in the Windows prompt, and enjoy your signed PDF!'}
                  </p>
                </div>
              </div>
            </div>

            <button
              onClick={() => setShowHelpModal(false)}
              className="btn btn-primary"
              style={{ width: '100%', marginTop: 20, padding: '10px 16px' }}
            >
              {language === 'gu' ? 'સમજાયું (Got it!)' : 'Got it!'}
            </button>
          </div>
        </div>
      )}
    </>
  );
};
