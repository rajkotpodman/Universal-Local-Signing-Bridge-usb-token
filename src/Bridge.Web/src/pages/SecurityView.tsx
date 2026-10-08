import React from 'react';
import { ShieldCheck, Lock, CheckCircle2, ShieldAlert } from 'lucide-react';

export const SecurityView: React.FC = () => {
  const policies = [
    {
      title: 'Zero Private Key Extraction',
      description: 'Private keys residing in hardware smart cards or OS key containers are marked non-exportable. Cryptographic signing occurs strictly inside the token or CNG engine.',
      status: 'HARDWARE BOUND'
    },
    {
      title: 'Zero PIN Interception & Storage',
      description: 'The bridge never collects, prompts for, or logs token PINs. All PIN challenges are rendered natively by Windows CSP/KSP dialogs or token middleware.',
      status: 'AIR-GAPPED'
    },
    {
      title: 'Strict Loopback-Only Binding',
      description: 'The HTTP server binds exclusively to IPv4 127.0.0.1. Remote IP packets are rejected at the network layer and middleware, preventing external exposure.',
      status: 'LOCAL LOOPBACK'
    },
    {
      title: 'Origin Allowlist & DNS Rebinding Defense',
      description: 'Incoming browser cross-origin requests must match configured authorized origins (default localhost & 127.0.0.1). Wildcards are disallowed.',
      status: 'ENFORCED'
    },
    {
      title: 'Cryptographic Session Authorization',
      description: 'Browser portals must negotiate origin-bound anti-CSRF session tokens before dispatching sensitive signing requests.',
      status: 'TOKEN BOUND'
    },
    {
      title: 'Cryptographic Algorithm Whitelist',
      description: 'Only modern algorithms (SHA-256, SHA-384, SHA-512) are allowed. Legacy algorithms like MD5 or SHA-1 are rejected automatically.',
      status: 'SHA-2 ONLY'
    },
    {
      title: 'Denial of Service Limits',
      description: 'Incoming payloads are strictly capped at 5 MB, and signing operations are rate-limited to 60 requests/minute per client IP.',
      status: '5 MB / 60 RPM'
    },
    {
      title: 'Audit Digest Integrity',
      description: 'Audit logs preserve SHA-256 digests of document payloads without storing confidential document contents or credentials.',
      status: 'SHA-256 DIGEST'
    }
  ];

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 20 }}>
      {/* Header */}
      <div>
        <h2 style={{ fontSize: 20, fontWeight: 700, margin: '0 0 4px 0', display: 'flex', alignItems: 'center', gap: 10 }}>
          <ShieldCheck size={20} color="#3b82f6" />
          Zero-Trust Security Model & Policy Specifications
        </h2>
        <p style={{ margin: 0, fontSize: 13, color: 'var(--text-muted)' }}>
          Cryptographic isolation, private key safety invariants, and local network boundary protections.
        </p>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))', gap: 16 }}>
        {policies.map(p => (
          <div key={p.title} className="glass-card" style={{ padding: 20, display: 'flex', flexDirection: 'column', gap: 10 }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
              <h3 style={{ fontSize: 15, fontWeight: 700, margin: 0, color: '#f8fafc' }}>
                {p.title}
              </h3>
              <span className="badge badge-success" style={{ fontSize: 11 }}>
                {p.status}
              </span>
            </div>

            <p style={{ margin: 0, fontSize: 13, color: 'var(--text-muted)', lineHeight: 1.5 }}>
              {p.description}
            </p>
          </div>
        ))}
      </div>
    </div>
  );
};
