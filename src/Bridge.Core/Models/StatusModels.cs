using System.Collections.Generic;

namespace Bridge.Core.Models;

public record BridgeStatusResponse(
    string Service,
    string Version,
    string Status, // Running, Degraded, Stopped
    string Host,
    int Port,
    double UptimeSeconds,
    bool MockMode,
    string ActiveProviderId,
    int TotalCertificates,
    int SmartCardCertificates,
    int ActiveSessions,
    int WsClients,
    long TotalSignOperations,
    bool TlsEnabled
);

public record HealthResponse(
    string Status,
    string Service,
    string Version,
    string Timestamp
);
