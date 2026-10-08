export interface Certificate {
  id: string;
  subject: string;
  issuer: string;
  serialNumber: string;
  thumbprint: string;
  validFrom: string;
  validTo: string;
  signatureAlgorithm: string;
  publicKeyAlgorithm: string;
  keySize: number;
  providerId: string;
  smartCardBacked: boolean;
  hasPrivateKey: boolean;
  status: 'VALID' | 'EXPIRING_SOON' | 'EXPIRED' | 'NOT_YET_VALID' | 'NO_PRIVATE_KEY' | 'REVOKED';
}

export interface ProviderInfo {
  id: string;
  name: string;
  type: string;
  description: string;
  status: 'AVAILABLE' | 'UNAVAILABLE' | 'ERROR' | 'MOCK';
  isHardwareBacked: boolean;
  supportedAlgorithms: string[];
  details?: string;
  canEnumerateTokens: boolean;
}

export interface BridgeStatus {
  service: string;
  version: string;
  status: string;
  host: string;
  port: number;
  uptimeSeconds: number;
  mockMode: boolean;
  activeProviderId: string;
  totalCertificates: number;
  smartCardCertificates: number;
  activeSessions: number;
  wsClients: number;
  totalSignOperations: number;
  tlsEnabled: boolean;
}

export interface SignRequest {
  certificateId: string;
  hashAlgorithm: string;
  data: string; // Base64
}

export interface SignResponse {
  success: boolean;
  signature: string;
  certificate: {
    subject: string;
    issuer: string;
    serialNumber: string;
    thumbprint: string;
  };
  algorithm: string;
  timestamp: string;
  durationMs: number;
  providerId: string;
}

export interface AuditEntry {
  id: string;
  timestamp: string;
  sessionIdHash?: string;
  certificateId?: string;
  provider: string;
  operation: string;
  success: boolean;
  errorCode?: string;
  durationMs: number;
  clientIp?: string;
  origin?: string;
  payloadDigestSha256?: string;
  details?: string;
}

export interface SessionResponse {
  success: boolean;
  sessionId: string;
  createdAt: string;
  expiresAt: string;
  allowedOrigin: string;
}

export interface WorldProviderStatus {
  id: string;
  manufacturer: string;
  brandName: string;
  modelSupport: string;
  cspName: string;
  cspInstalled: boolean;
  minidriverName: string;
  pkcs11Path?: string;
  pkcs11Available: boolean;
  description: string;
  isAladdinFamily: boolean;
  readiness: 'READY' | 'INSTALLED' | 'DRIVER_REQUIRED';
  statusDetails: string;
}

export interface AttachedTokenInfo {
  deviceName: string;
  manufacturer: string;
  hardwareId: string;
  status: string;
  class: string;
  isAladdinToken: boolean;
  providerId: string;
  details: string;
}

export interface WorldCatalogResponse {
  totalProviders: number;
  aladdinFamilyDetected: boolean;
  aladdinEtokenReady: boolean;
  hasAttachedToken: boolean;
  attachedTokenCount: number;
  attachedTokens: AttachedTokenInfo[];
  providers: WorldProviderStatus[];
}

export interface ScanProvidersResponse {
  success: boolean;
  scannedAt: string;
  totalProviders: number;
  readyProviders: number;
  certificatesFound: number;
  hasAttachedToken: boolean;
  attachedTokenCount: number;
  attachedTokens: AttachedTokenInfo[];
  providers: WorldProviderStatus[];
}


