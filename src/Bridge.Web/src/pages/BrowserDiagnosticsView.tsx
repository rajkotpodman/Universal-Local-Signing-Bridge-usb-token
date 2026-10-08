import React, { useState, useEffect } from 'react';
import { Stethoscope, CheckCircle2, XCircle, RefreshCw, Play, HelpCircle, Activity } from 'lucide-react';
import { fetchHealth, fetchStatus, createSession, fetchCertificates, executeSign } from '../api';

interface DiagnosticStep {
  id: number;
  name: string;
  description: string;
  status: 'pending' | 'running' | 'success' | 'failed';
  details?: string;
  explanation?: string;
}

export const BrowserDiagnosticsView: React.FC = () => {
  const [steps, setSteps] = useState<DiagnosticStep[]>([
    {
      id: 1,
      name: 'Step 1: Browser → localhost loopback reachability',
      description: 'Verifies that the browser can connect to 127.0.0.1 without being blocked by network sandboxes or firewalls.',
      status: 'pending',
      explanation: 'Ensure the Universal Local Signing Bridge service is running and listening on 127.0.0.1:8080.'
    },
    {
      id: 2,
      name: 'Step 2: REST API availability',
      description: 'Validates that the ASP.NET Core REST API (/health and /api/v1/status) responds with valid JSON.',
      status: 'pending',
      explanation: 'The bridge HTTP listener is unreachable or port 8080 is blocked by another local application.'
    },
    {
      id: 3,
      name: 'Step 3: WebSocket bi-directional channel',
      description: 'Tests WebSocket handshake (/ws/v1) for real-time event streaming.',
      status: 'pending',
      explanation: 'WebSocket protocol was rejected or local proxy prevented upgrade handshake.'
    },
    {
      id: 4,
      name: 'Step 4: Cryptographic session negotiation',
      description: 'Requests a short-lived anti-CSRF session token bound to the current origin.',
      status: 'pending',
      explanation: 'Session generation failed. Verify that origin is allowed in appsettings.json.'
    },
    {
      id: 5,
      name: 'Step 5: Certificate provider enumeration',
      description: 'Queries active cryptographic providers and detects available keys.',
      status: 'pending',
      explanation: 'Windows certificate store is accessible, but no certificate with a usable private key was detected. Insert USB token or enable MOCK_MODE=true.'
    },
    {
      id: 6,
      name: 'Step 6: Hardware/Provider signing operation',
      description: 'Performs a test digital signature using the active certificate and provider.',
      status: 'pending',
      explanation: 'Signing operation failed. Verify token PIN prompt was not cancelled and driver middleware is installed.'
    }
  ]);

  const [isRunning, setIsRunning] = useState(false);
  const [overallResult, setOverallResult] = useState<'PASS' | 'FAIL' | null>(null);

  const updateStep = (id: number, updates: Partial<DiagnosticStep>) => {
    setSteps(prev => prev.map(s => s.id === id ? { ...s, ...updates } : s));
  };

  const runDiagnostics = async () => {
    setIsRunning(true);
    setOverallResult(null);

    // Reset steps
    setSteps(prev => prev.map(s => ({ ...s, status: 'pending', details: undefined })));

    let targetCertId: string | null = null;

    // --- Step 1 & 2: Health & REST ---
    updateStep(1, { status: 'running' });
    updateStep(2, { status: 'running' });
    try {
      const health = await fetchHealth();
      updateStep(1, { status: 'success', details: 'Loopback 127.0.0.1 successfully reached from browser.' });
      const status = await fetchStatus();
      updateStep(2, { status: 'success', details: `REST API v1 active. Service: ${status.service} (Uptime: ${status.uptimeSeconds}s)` });
    } catch (e: any) {
      updateStep(1, { status: 'failed', details: e.message });
      updateStep(2, { status: 'failed', details: 'Could not connect to localhost REST service.' });
      setIsRunning(false);
      setOverallResult('FAIL');
      return;
    }

    // --- Step 3: WebSocket ---
    updateStep(3, { status: 'running' });
    try {
      await new Promise<void>((resolve, reject) => {
        const wsUrl = `ws://${window.location.hostname}:8080/ws/v1`;
        const ws = new WebSocket(wsUrl);
        const timer = setTimeout(() => {
          ws.close();
          reject(new Error('WebSocket connection timed out (4s)'));
        }, 4000);

        ws.onopen = () => {
          clearTimeout(timer);
          ws.close();
          resolve();
        };
        ws.onerror = () => {
          clearTimeout(timer);
          reject(new Error('WebSocket handshake error'));
        };
      });
      updateStep(3, { status: 'success', details: 'WebSocket full-duplex channel connected successfully.' });
    } catch (e: any) {
      updateStep(3, { status: 'failed', details: e.message });
      // Non-fatal for REST signing, but marks step
    }

    // --- Step 4: Session ---
    updateStep(4, { status: 'running' });
    try {
      const session = await createSession();
      updateStep(4, { status: 'success', details: `Session token issued: ${session.sessionId.slice(0, 12)}... (Origin: ${session.allowedOrigin})` });
    } catch (e: any) {
      updateStep(4, { status: 'failed', details: e.message });
      setIsRunning(false);
      setOverallResult('FAIL');
      return;
    }

    // --- Step 5: Certificate Provider ---
    updateStep(5, { status: 'running' });
    try {
      const certs = await fetchCertificates();
      if (certs.length === 0) {
        updateStep(5, {
          status: 'failed',
          details: 'Zero certificates detected.',
          explanation: 'Windows certificate store is accessible, but no certificate with a usable private key was detected.'
        });
        setIsRunning(false);
        setOverallResult('FAIL');
        return;
      }

      const keyCert = certs.find(c => c.hasPrivateKey);
      if (!keyCert) {
        updateStep(5, {
          status: 'failed',
          details: 'Found certificates, but none possessed a usable private key.',
          explanation: 'Windows certificate store is accessible, but no certificate with a usable private key was detected.'
        });
        setIsRunning(false);
        setOverallResult('FAIL');
        return;
      }

      targetCertId = keyCert.id;
      updateStep(5, {
        status: 'success',
        details: `Discovered ${certs.length} certificate(s). Active Key: "${keyCert.subject}" (${keyCert.providerId})`
      });
    } catch (e: any) {
      updateStep(5, { status: 'failed', details: e.message });
      setIsRunning(false);
      setOverallResult('FAIL');
      return;
    }

    // --- Step 6: Signing ---
    updateStep(6, { status: 'running' });
    try {
      const testData = btoa('Diagnostic Self-Test @ ' + Date.now());
      const signRes = await executeSign({
        certificateId: targetCertId,
        hashAlgorithm: 'SHA-256',
        data: testData
      });

      updateStep(6, {
        status: 'success',
        details: `Generated signature via ${signRes.algorithm} in ${signRes.durationMs}ms.`
      });
      setOverallResult('PASS');
    } catch (e: any) {
      updateStep(6, { status: 'failed', details: e.message });
      setOverallResult('FAIL');
    }

    setIsRunning(false);
  };

  useEffect(() => {
    runDiagnostics();
  }, []);

  const passCount = steps.filter(s => s.status === 'success').length;

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 24 }}>
      {/* Header */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 14 }}>
        <div>
          <h2 style={{ fontSize: 20, fontWeight: 700, margin: '0 0 4px 0', display: 'flex', alignItems: 'center', gap: 10 }}>
            <Stethoscope size={20} color="#3b82f6" />
            One-Click Browser Diagnostics Wizard
          </h2>
          <p style={{ margin: 0, fontSize: 13, color: 'var(--text-muted)' }}>
            Automated 6-step verification of browser connectivity, REST endpoints, WebSockets, sessions, and digital signing.
          </p>
        </div>

        <button onClick={runDiagnostics} className="btn btn-primary" disabled={isRunning}>
          {isRunning ? (
            <>
              <RefreshCw size={14} className="spin" />
              <span>Running Audit...</span>
            </>
          ) : (
            <>
              <Play size={14} />
              <span>Run Diagnostics</span>
            </>
          )}
        </button>
      </div>

      {/* Outcome Scorecard */}
      <div className="glass-card" style={{
        padding: 24,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        flexWrap: 'wrap',
        gap: 20
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 16 }}>
          <div style={{
            width: 52,
            height: 52,
            borderRadius: '50%',
            background: overallResult === 'PASS' ? 'rgba(16, 185, 129, 0.15)' : overallResult === 'FAIL' ? 'rgba(244, 63, 94, 0.15)' : 'rgba(59, 130, 246, 0.15)',
            border: `2px solid ${overallResult === 'PASS' ? '#10b981' : overallResult === 'FAIL' ? '#f43f5e' : '#3b82f6'}`,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            fontSize: 18,
            fontWeight: 800
          }}>
            {passCount}/{steps.length}
          </div>
          <div>
            <h3 style={{ fontSize: 17, fontWeight: 700, margin: '0 0 4px 0' }}>
              Overall Diagnostic Verdict: {overallResult || 'IN PROGRESS...'}
            </h3>
            <p style={{ margin: 0, fontSize: 13, color: 'var(--text-muted)' }}>
              {overallResult === 'PASS'
                ? 'All diagnostic verifications passed. Browser is fully authorized to communicate with local cryptographic tokens.'
                : overallResult === 'FAIL'
                ? 'One or more diagnostic steps failed. Review the detailed human-readable explanation below.'
                : 'Evaluating localhost bridge stack...'}
            </p>
          </div>
        </div>

        {overallResult && (
          <span className={`badge ${overallResult === 'PASS' ? 'badge-success' : 'badge-danger'}`} style={{ fontSize: 14, padding: '6px 14px' }}>
            {overallResult === 'PASS' ? <CheckCircle2 size={16} /> : <XCircle size={16} />}
            <span>{overallResult === 'PASS' ? 'READY FOR SIGNING' : 'ACTION REQUIRED'}</span>
          </span>
        )}
      </div>

      {/* Steps List */}
      <div style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
        {steps.map(step => {
          const isSuccess = step.status === 'success';
          const isFailed = step.status === 'failed';
          const isRunningItem = step.status === 'running';

          return (
            <div
              key={step.id}
              className="glass-card"
              style={{
                padding: 18,
                borderLeft: `4px solid ${isSuccess ? '#10b981' : isFailed ? '#f43f5e' : isRunningItem ? '#3b82f6' : '#334155'}`,
                display: 'flex',
                flexDirection: 'column',
                gap: 6
              }}
            >
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                  {isSuccess && <CheckCircle2 size={18} color="#10b981" />}
                  {isFailed && <XCircle size={18} color="#f43f5e" />}
                  {isRunningItem && <RefreshCw size={18} color="#3b82f6" className="spin" />}
                  {step.status === 'pending' && <div style={{ width: 18, height: 18, borderRadius: '50%', background: '#334155' }} />}

                  <span style={{ fontSize: 14, fontWeight: 700, color: '#f8fafc' }}>
                    {step.name}
                  </span>
                </div>

                <span className={`badge ${isSuccess ? 'badge-success' : isFailed ? 'badge-danger' : isRunningItem ? 'badge-info' : 'badge-secondary'}`}>
                  {step.status.toUpperCase()}
                </span>
              </div>

              <div style={{ fontSize: 12, color: 'var(--text-muted)', marginLeft: 28 }}>
                {step.description}
              </div>

              {step.details && (
                <div style={{ fontSize: 12, color: isFailed ? '#f87171' : '#93c5fd', marginLeft: 28, marginTop: 4 }}>
                  {step.details}
                </div>
              )}

              {/* Human-readable explanation on failure */}
              {isFailed && step.explanation && (
                <div style={{
                  marginLeft: 28,
                  marginTop: 6,
                  padding: 10,
                  borderRadius: 8,
                  background: 'rgba(245, 158, 11, 0.1)',
                  border: '1px solid rgba(245, 158, 11, 0.25)',
                  fontSize: 12,
                  color: '#fbbf24',
                  display: 'flex',
                  alignItems: 'flex-start',
                  gap: 8
                }}>
                  <HelpCircle size={16} style={{ flexShrink: 0, marginTop: 1 }} />
                  <div>
                    <strong>Resolution guidance:</strong> {step.explanation}
                  </div>
                </div>
              )}
            </div>
          );
        })}
      </div>
    </div>
  );
};
