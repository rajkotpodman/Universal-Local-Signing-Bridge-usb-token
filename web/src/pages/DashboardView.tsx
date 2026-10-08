import React, { useState, useRef } from 'react';
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
  Cpu,
  Upload,
  FileCheck,
  Download,
  AlertCircle,
  Sparkles,
  CheckCircle2
} from 'lucide-react';
import { BridgeStatus, ProviderInfo, Certificate } from '../types';
import { useLanguage } from '../context/LanguageContext';
import { executePdfSign } from '../api';

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
  const { t, language } = useLanguage();
  const isMock = status?.mockMode ?? true;
  const availableProviders = providers.filter(p => p.status === 'AVAILABLE' || p.status === 'MOCK');

  // 1-Click Signer State
  const [selectedCertId, setSelectedCertId] = useState<string>(certificates[0]?.id || '');
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [fileBase64, setFileBase64] = useState<string | null>(null);
  const [signingReason, setSigningReason] = useState<string>('Digitally Approved & Verified');
  const [isSigning, setIsSigning] = useState(false);
  const [signSuccess, setSignSuccess] = useState<string | null>(null);
  const [signError, setSignError] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    processFile(file);
  };

  const handleDrop = (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    const file = e.dataTransfer.files?.[0];
    if (file) processFile(file);
  };

  const processFile = (file: File) => {
    setSelectedFile(file);
    setSignSuccess(null);
    setSignError(null);

    const reader = new FileReader();
    reader.onload = () => {
      const result = reader.result as string;
      const b64 = result.split(',')[1] || result;
      setFileBase64(b64);
    };
    reader.readAsDataURL(file);
  };

  const handleOneClickSign = async () => {
    const cert = certificates.find(c => c.id === selectedCertId) || certificates[0];
    if (!cert) {
      setSignError(language === 'gu' ? 'કૃપા કરીને સહી કરવા માટે સર્ટિફિકેટ પસંદ કરો.' : 'Please select a signing certificate.');
      return;
    }

    setIsSigning(true);
    setSignError(null);
    setSignSuccess(null);

    try {
      const res = await executePdfSign({
        certificateId: cert.id,
        pdfBase64: fileBase64 || undefined,
        createSampleIfEmpty: !fileBase64,
        enableVisualSignature: true,
        pageNumber: 1,
        reason: signingReason,
        location: 'Localhost Bridge'
      });

      // Auto-download signed file
      const byteCharacters = atob(res.signedPdfBase64);
      const byteNumbers = new Array(byteCharacters.length);
      for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
      }
      const byteArray = new Uint8Array(byteNumbers);
      const blob = new Blob([byteArray], { type: 'application/pdf' });
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = selectedFile
        ? `Signed-${selectedFile.name}`
        : `Signed-Document-${Date.now()}.pdf`;
      a.click();
      URL.revokeObjectURL(url);

      setSignSuccess(language === 'gu'
        ? `સહી સફળ! ફાઇલ (${Math.round(res.totalBytes / 1024)} KB) આપમેળે ડાઉનલોડ થઈ ગઈ છે.`
        : `Signing Successful! Downloaded (${Math.round(res.totalBytes / 1024)} KB) in ${res.durationMs}ms.`);
    } catch (err: any) {
      setSignError(err.message || 'Signing failed.');
    } finally {
      setIsSigning(false);
    }
  };

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
              {isMock ? (language === 'gu' ? 'ડેવલપમેન્ટ મોક પ્રોવાઇડર સક્રિય' : 'DEVELOPMENT MOCK PROVIDER ACTIVE') : (language === 'gu' ? 'વિન્ડોઝ હાર્ડવેર ટોકન મોડ સક્રિય' : 'WINDOWS HARDWARE TOKEN MODE')}
            </span>
            <span style={{ fontSize: 13, color: 'var(--text-dim)' }}>
              127.0.0.1:{status?.port || 8080}
            </span>
          </div>
          <h2 style={{ fontSize: 22, fontWeight: 800, margin: '0 0 6px 0', color: '#f8fafc' }}>
            {t('bridgeTitle')}
          </h2>
          <p style={{ margin: 0, color: 'var(--text-muted)', fontSize: 13, maxWidth: 650, lineHeight: 1.5 }}>
            {language === 'gu'
              ? 'તમારા વેબ બ્રાઉઝરને USB ટોકન અને વિન્ડોઝ ડિજિટલ સહી (DSC) સાથે સુરક્ષિત રીતે જોડતો સૌથી ઝડપી સેતુ.'
              : 'High-performance localhost middleware connecting browser applications to cryptographic hardware tokens without browser extensions or Java applets.'}
          </p>
        </div>

        <div style={{ display: 'flex', gap: 10 }}>
          <button onClick={() => onNavigate('diagnostics')} className="btn btn-primary">
            <Activity size={15} />
            <span>{t('diagnostics')}</span>
          </button>
          <button onClick={() => onNavigate('sign')} className="btn btn-secondary">
            <PenTool size={15} />
            <span>{t('sign')}</span>
          </button>
        </div>
      </div>

      {/* 1-Click Quick PDF Signer Widget */}
      <div className="glass-card" style={{
        padding: 24,
        background: 'linear-gradient(135deg, rgba(16, 185, 129, 0.08) 0%, rgba(15, 23, 42, 0.5) 100%)',
        border: '1px solid rgba(52, 211, 153, 0.3)'
      }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            <Sparkles size={22} color="#34d399" />
            <div>
              <h3 style={{ margin: 0, fontSize: 17, fontWeight: 800, color: '#f8fafc' }}>
                {t('oneClickTitle')}
              </h3>
              <p style={{ margin: '2px 0 0 0', fontSize: 13, color: 'var(--text-muted)' }}>
                {t('oneClickSub')}
              </p>
            </div>
          </div>
          <span className="badge badge-success" style={{ fontSize: 11 }}>
            ISO 32000-1 PAdES-BES
          </span>
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))', gap: 20 }}>
          {/* Dropzone */}
          <div
            onDragOver={e => e.preventDefault()}
            onDrop={handleDrop}
            onClick={() => fileInputRef.current?.click()}
            style={{
              border: '2px dashed rgba(52, 211, 153, 0.4)',
              borderRadius: 12,
              padding: 24,
              textAlign: 'center',
              cursor: 'pointer',
              background: 'rgba(0, 0, 0, 0.2)',
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'center',
              justifyContent: 'center',
              gap: 10,
              minHeight: 140
            }}
          >
            <input
              type="file"
              ref={fileInputRef}
              onChange={handleFileChange}
              accept=".pdf"
              style={{ display: 'none' }}
            />
            <Upload size={32} color="#34d399" />
            <div>
              <div style={{ fontSize: 14, fontWeight: 700, color: '#f8fafc' }}>
                {selectedFile ? selectedFile.name : t('selectOrDrop')}
              </div>
              <div style={{ fontSize: 12, color: 'var(--text-dim)', marginTop: 4 }}>
                {selectedFile ? `${Math.round(selectedFile.size / 1024)} KB` : (language === 'gu' ? 'PDF ફાઇલ પસંદ કરવા અહીં ક્લિક કરો' : 'Supports standard PDF files up to 25 MB')}
              </div>
            </div>
          </div>

          {/* Quick Options & Button */}
          <div style={{ display: 'flex', flexDirection: 'column', gap: 12, justifyContent: 'center' }}>
            <div>
              <label style={{ display: 'block', fontSize: 12, fontWeight: 600, color: 'var(--text-muted)', marginBottom: 4 }}>
                {t('selectCertLabel')}
              </label>
              <select
                className="form-input"
                value={selectedCertId || certificates[0]?.id || ''}
                onChange={e => setSelectedCertId(e.target.value)}
                style={{ fontSize: 13 }}
              >
                {certificates.map(c => (
                  <option key={c.id} value={c.id}>
                    {c.subject} [{c.providerId}]
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label style={{ display: 'block', fontSize: 12, fontWeight: 600, color: 'var(--text-muted)', marginBottom: 4 }}>
                {t('signingReason')}
              </label>
              <input
                type="text"
                className="form-input"
                value={signingReason}
                onChange={e => setSigningReason(e.target.value)}
                style={{ fontSize: 13 }}
              />
            </div>

            <button
              onClick={handleOneClickSign}
              disabled={isSigning || certificates.length === 0}
              className="btn btn-primary"
              style={{ padding: '12px 18px', fontWeight: 700, fontSize: 14 }}
            >
              {isSigning ? (
                <>
                  <span className="pulse-dot" />
                  <span>{language === 'gu' ? 'ટોકન વડે સહી થઈ રહી છે...' : 'Signing with Token...'}</span>
                </>
              ) : (
                <>
                  <FileCheck size={16} />
                  <span>{t('signAndDownload')}</span>
                </>
              )}
            </button>
          </div>
        </div>

        {signSuccess && (
          <div style={{
            marginTop: 16,
            background: 'rgba(16, 185, 129, 0.15)',
            border: '1px solid rgba(16, 185, 129, 0.4)',
            borderRadius: 8,
            padding: 12,
            fontSize: 13,
            color: '#34d399',
            display: 'flex',
            alignItems: 'center',
            gap: 10
          }}>
            <CheckCircle2 size={18} />
            <span>{signSuccess}</span>
          </div>
        )}

        {signError && (
          <div style={{
            marginTop: 16,
            background: 'rgba(244, 63, 94, 0.15)',
            border: '1px solid rgba(244, 63, 94, 0.4)',
            borderRadius: 8,
            padding: 12,
            fontSize: 13,
            color: '#f43f5e',
            display: 'flex',
            alignItems: 'center',
            gap: 10
          }}>
            <AlertCircle size={18} />
            <span>{signError}</span>
          </div>
        )}
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
            <span style={{ fontSize: 12, color: 'var(--text-dim)', fontWeight: 600 }}>{t('serviceCard')}</span>
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
            <span style={{ fontSize: 12, color: 'var(--text-dim)', fontWeight: 600 }}>{t('apiCard')}</span>
            <Globe size={16} color="#60a5fa" />
          </div>
          <div style={{ fontSize: 20, fontWeight: 800, color: '#f8fafc' }}>
            v1.0.0
          </div>
          <span style={{ fontSize: 11, color: 'var(--text-dim)', marginTop: 4, display: 'block' }}>
            REST & WebSocket
          </span>
        </div>

        {/* Metric 3: Active Providers */}
        <div className="glass-card" style={{ padding: 18, cursor: 'pointer' }} onClick={() => onNavigate('providers')}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 10 }}>
            <span style={{ fontSize: 12, color: 'var(--text-dim)', fontWeight: 600 }}>{t('providersCard')}</span>
            <Layers size={16} color="#a855f7" />
          </div>
          <div style={{ fontSize: 20, fontWeight: 800, color: '#f8fafc' }}>
            {availableProviders.length}
          </div>
          <span style={{ fontSize: 11, color: 'var(--text-dim)', marginTop: 4, display: 'block' }}>
            Aladdin & PC/SC WinSCard
          </span>
        </div>

        {/* Metric 4: Total Certificates */}
        <div className="glass-card" style={{ padding: 18, cursor: 'pointer' }} onClick={() => onNavigate('certificates')}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 10 }}>
            <span style={{ fontSize: 12, color: 'var(--text-dim)', fontWeight: 600 }}>{t('certificatesCard')}</span>
            <KeyRound size={16} color="#f59e0b" />
          </div>
          <div style={{ fontSize: 20, fontWeight: 800, color: '#f8fafc' }}>
            {status?.totalCertificates ?? certificates.length}
          </div>
          <span style={{ fontSize: 11, color: 'var(--text-dim)', marginTop: 4, display: 'block' }}>
            DSC Certificates Available
          </span>
        </div>

        {/* Metric 5: Smart Cards */}
        <div className="glass-card" style={{ padding: 18, cursor: 'pointer' }} onClick={() => onNavigate('providers')}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 10 }}>
            <span style={{ fontSize: 12, color: 'var(--text-dim)', fontWeight: 600 }}>{t('smartCardsCard')}</span>
            <Cpu size={16} color="#ec4899" />
          </div>
          <div style={{ fontSize: 20, fontWeight: 800, color: '#f8fafc' }}>
            {status?.smartCardCertificates ?? certificates.filter(c => c.smartCardBacked).length}
          </div>
          <span style={{ fontSize: 11, color: 'var(--text-dim)', marginTop: 4, display: 'block' }}>
            Hardware Backed Keys
          </span>
        </div>

        {/* Metric 6: Total Signatures */}
        <div className="glass-card" style={{ padding: 18, cursor: 'pointer' }} onClick={() => onNavigate('audit')}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 10 }}>
            <span style={{ fontSize: 12, color: 'var(--text-dim)', fontWeight: 600 }}>{t('operationsCard')}</span>
            <PenTool size={16} color="#34d399" />
          </div>
          <div style={{ fontSize: 20, fontWeight: 800, color: '#f8fafc' }}>
            {status?.totalSignOperations ?? 0}
          </div>
          <span style={{ fontSize: 11, color: 'var(--text-dim)', marginTop: 4, display: 'block' }}>
            PAdES, XAdES, CAdES
          </span>
        </div>
      </div>
    </div>
  );
};
