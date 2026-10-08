import {
  Certificate,
  ProviderInfo,
  BridgeStatus,
  SignRequest,
  SignResponse,
  AuditEntry,
  SessionResponse,
  WorldCatalogResponse,
  ScanProvidersResponse
} from './types';

const BASE_URL = window.location.origin;

let cachedSessionId: string | null = null;

export async function fetchHealth(): Promise<{ status: string; service: string }> {
  const res = await fetch(`${BASE_URL}/health`);
  if (!res.ok) throw new Error(`Health probe failed: HTTP ${res.status}`);
  return res.json();
}

export async function fetchStatus(): Promise<BridgeStatus> {
  const res = await fetch(`${BASE_URL}/api/v1/status`);
  if (!res.ok) throw new Error(`Status query failed: HTTP ${res.status}`);
  return res.json();
}

export async function fetchProviders(): Promise<ProviderInfo[]> {
  const res = await fetch(`${BASE_URL}/api/v1/providers`);
  if (!res.ok) throw new Error(`Providers query failed: HTTP ${res.status}`);
  return res.json();
}

export async function fetchWorldCatalog(): Promise<WorldCatalogResponse> {
  const res = await fetch(`${BASE_URL}/api/v1/providers/world-catalog`);
  if (!res.ok) throw new Error(`World catalog query failed: HTTP ${res.status}`);
  return res.json();
}

export async function scanProviders(): Promise<ScanProvidersResponse> {
  const res = await fetch(`${BASE_URL}/api/v1/providers/scan`, {
    method: 'POST'
  });
  if (!res.ok) throw new Error(`Hardware scan failed: HTTP ${res.status}`);
  return res.json();
}

export async function fetchCertificates(): Promise<Certificate[]> {
  const res = await fetch(`${BASE_URL}/api/v1/certificates`);
  if (!res.ok) throw new Error(`Certificates query failed: HTTP ${res.status}`);
  return res.json();
}

export async function createSession(): Promise<SessionResponse> {
  const res = await fetch(`${BASE_URL}/api/v1/session`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ clientAppId: 'WebUI', origin: window.location.origin })
  });
  if (!res.ok) throw new Error(`Session negotiation failed: HTTP ${res.status}`);
  const data: SessionResponse = await res.json();
  cachedSessionId = data.sessionId;
  return data;
}

export async function executeSign(req: SignRequest): Promise<SignResponse> {
  if (!cachedSessionId) {
    try {
      await createSession();
    } catch { }
  }

  const headers: Record<string, string> = {
    'Content-Type': 'application/json'
  };
  if (cachedSessionId) {
    headers['X-Session-Token'] = cachedSessionId;
  }

  const res = await fetch(`${BASE_URL}/api/v1/sign`, {
    method: 'POST',
    headers,
    body: JSON.stringify(req)
  });

  if (!res.ok) {
    const errorBody = await res.json().catch(() => null);
    const msg = errorBody?.error?.message || `Signing failed with HTTP ${res.status}`;
    throw new Error(msg);
  }

  return res.json();
}

export async function fetchAuditLogs(limit: number = 50): Promise<AuditEntry[]> {
  const res = await fetch(`${BASE_URL}/api/v1/audit?limit=${limit}`);
  if (!res.ok) throw new Error(`Audit query failed: HTTP ${res.status}`);
  return res.json();
}
