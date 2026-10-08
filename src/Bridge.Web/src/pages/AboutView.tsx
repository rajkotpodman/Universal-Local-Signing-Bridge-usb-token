import React from 'react';
import { Info, Shield, CheckCircle2, AlertTriangle } from 'lucide-react';

export const AboutView: React.FC = () => {
  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 20 }}>
      <div>
        <h2 style={{ fontSize: 20, fontWeight: 700, margin: '0 0 4px 0', display: 'flex', alignItems: 'center', gap: 10 }}>
          <Info size={20} color="#3b82f6" />
          About Universal Local Signing Bridge
        </h2>
        <p style={{ margin: 0, fontSize: 13, color: 'var(--text-muted)' }}>
          Vision, technology stack, cryptographic standards, and enterprise compliance.
        </p>
      </div>

      <div className="glass-card" style={{ padding: 22, display: 'flex', flexDirection: 'column', gap: 14 }}>
        <h3 style={{ fontSize: 15, fontWeight: 700, margin: 0 }}>Project Vision</h3>
        <p style={{ margin: 0, fontSize: 13, color: 'var(--text-muted)', lineHeight: 1.6 }}>
          With modern web browsers deprecating NPAPI plugins, ActiveX controls, and Java applets, web portals (government, banking, tax, customs, digital identity) require a secure, lightweight localhost bridge to communicate with local hardware security modules (HSMs), smart cards, and USB cryptographic tokens.
        </p>
        <p style={{ margin: 0, fontSize: 13, color: 'var(--text-muted)', lineHeight: 1.6 }}>
          The Universal Local Signing Bridge provides this middleware while enforcing zero-trust boundaries: private keys remain hardware-bound and never leave the token chip, and PIN entry is strictly delegated to the operating system or token vendor UI.
        </p>
      </div>

      {/* Legal & Security Disclaimer */}
      <div className="glass-card" style={{ padding: 22, borderLeft: '4px solid #f59e0b' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginBottom: 8 }}>
          <AlertTriangle size={18} color="#f59e0b" />
          <h3 style={{ fontSize: 15, fontWeight: 700, margin: 0, color: '#fbbf24' }}>
            Cryptographic & Mock Disclaimer
          </h3>
        </div>
        <p style={{ margin: 0, fontSize: 13, color: 'var(--text-muted)', lineHeight: 1.6 }}>
          In Development Mock Mode (<code style={{ color: '#fbbf24' }}>MOCK_MODE=true</code>), generated digital signatures are produced by ephemeral in-memory software keys and are strictly intended for integration testing. They are <strong>NOT</strong> legally binding digital signatures. Production deployments must connect to legitimate certified cryptographic hardware tokens under Windows CNG or PKCS#11 providers.
        </p>
      </div>
    </div>
  );
};
