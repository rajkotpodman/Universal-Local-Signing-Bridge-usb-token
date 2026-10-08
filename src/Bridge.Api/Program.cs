using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Bridge.Api.Middleware;
using Bridge.Api.Services;
using Bridge.Core.Configuration;
using Bridge.Core.Interfaces;
using Bridge.Core.Models;
using Bridge.Crypto;
using Bridge.Provider.Mock;
using Bridge.Provider.Pkcs11;
using Bridge.Provider.Windows;
using Bridge.Providers;
using Bridge.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. Serilog Setup
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/bridge-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// 2. Load Configuration and Apply Environment Overrides
var bridgeConfig = new BridgeConfiguration();
builder.Configuration.GetSection("Bridge").Bind(bridgeConfig);

var envMockMode = Environment.GetEnvironmentVariable("MOCK_MODE");
if (!string.IsNullOrWhiteSpace(envMockMode))
{
    bridgeConfig.MockMode = bool.TryParse(envMockMode, out var m) ? m : true;
}

var envProviderMode = Environment.GetEnvironmentVariable("BRIDGE_MODE");
if (!string.IsNullOrWhiteSpace(envProviderMode))
{
    bridgeConfig.ProviderMode = envProviderMode.ToUpperInvariant();
}

var envPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(envPort) && int.TryParse(envPort, out var p))
{
    bridgeConfig.Port = p;
}

builder.Services.AddSingleton(bridgeConfig);

// 3. Register Security and Storage
builder.Services.AddSingleton(new OriginValidator(bridgeConfig.AllowedOrigins));
builder.Services.AddSingleton(new RateLimiter(bridgeConfig.RateLimitPerMinute));
builder.Services.AddSingleton<IAuditRepository>(_ => new SqliteAuditRepository("audit.db"));
builder.Services.AddSingleton<ISessionStore>(_ => new SqliteSessionStore("sessions.db", bridgeConfig.SessionLifetimeSeconds));

// 4. Register Providers and ProviderManager
builder.Services.AddSingleton<MockSigningProvider>();
builder.Services.AddSingleton<WindowsCertificateProvider>();
builder.Services.AddSingleton<Pkcs11SigningProvider>();
builder.Services.AddSingleton(sp =>
{
    var mgr = new ProviderManager(bridgeConfig);
    mgr.RegisterProvider(sp.GetRequiredService<MockSigningProvider>());
    mgr.RegisterProvider(sp.GetRequiredService<WindowsCertificateProvider>());
    mgr.RegisterProvider(sp.GetRequiredService<Pkcs11SigningProvider>());
    return mgr;
});

// 5. Register WebSocket Service
builder.Services.AddSingleton<WebSocketBridgeService>();

// 6. Swagger OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Universal Local Signing Bridge API",
        Version = "v1",
        Description = "Enterprise localhost PKI & smart-card cryptographic signing middleware for modern web applications.",
        Contact = new OpenApiContact { Name = "Universal Local Signing Bridge Architecture" }
    });
});

// 7. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("BridgeLocalhostCors", policy =>
    {
        policy.WithOrigins(bridgeConfig.AllowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 8. Auto-Port Resolution
static int ResolvePort(string host, int preferredPort)
{
    for (int port = preferredPort; port < preferredPort + 50; port++)
    {
        try
        {
            using var listener = new TcpListener(IPAddress.Parse(host), port);
            listener.Start();
            listener.Stop();
            return port;
        }
        catch (SocketException) { }
    }
    return preferredPort;
}

int actualPort = ResolvePort(bridgeConfig.Host, bridgeConfig.Port);
bridgeConfig.Port = actualPort;
builder.WebHost.UseUrls($"http://{bridgeConfig.Host}:{actualPort}");

var app = builder.Build();
var startTime = DateTimeOffset.UtcNow;

// Pipeline
app.UseMiddleware<GlobalErrorHandlingMiddleware>();
app.UseMiddleware<SecurityMiddleware>();
app.UseCors("BridgeLocalhostCors");
app.UseWebSockets();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Universal Local Signing Bridge v1");
    c.RoutePrefix = "swagger";
});

app.UseDefaultFiles();
app.UseStaticFiles();

// ==========================================
// REST API Endpoints
// ==========================================

// GET /health
app.MapGet("/health", () => Results.Ok(new HealthResponse(
    Status: "ok",
    Service: "Universal Local Signing Bridge",
    Version: "1.0.0",
    Timestamp: DateTimeOffset.UtcNow.ToString("O")
))).WithName("HealthCheck")
  .WithTags("System");

// GET /api/v1/status
app.MapGet("/api/v1/status", async (
    ProviderManager providerManager,
    ISessionStore sessionStore,
    IAuditRepository auditRepo,
    WebSocketBridgeService wsService) =>
{
    var certs = await providerManager.GetAllCertificatesAsync();
    var activeProv = providerManager.GetActiveProvider();
    var uptime = Math.Round((DateTimeOffset.UtcNow - startTime).TotalSeconds, 2);
    var activeSessions = await sessionStore.GetActiveSessionCountAsync();
    var totalSigns = await auditRepo.GetTotalOperationsCountAsync();

    return Results.Ok(new BridgeStatusResponse(
        Service: "Universal Local Signing Bridge",
        Version: "1.0.0",
        Status: "Running",
        Host: bridgeConfig.Host,
        Port: actualPort,
        UptimeSeconds: uptime,
        MockMode: bridgeConfig.MockMode || activeProv.GetProviderInfo().Status == ProviderStatus.MOCK,
        ActiveProviderId: activeProv.GetProviderInfo().Id,
        TotalCertificates: certs.Count,
        SmartCardCertificates: certs.Count(c => c.SmartCardBacked),
        ActiveSessions: activeSessions,
        WsClients: wsService.ConnectedClientCount,
        TotalSignOperations: totalSigns,
        TlsEnabled: bridgeConfig.EnableTls
    ));
}).WithName("GetServiceStatus")
  .WithTags("System");

// GET /api/v1/providers
app.MapGet("/api/v1/providers", (ProviderManager providerManager) =>
{
    var providers = providerManager.GetAllProvidersInfo();
    return Results.Ok(providers);
}).WithName("GetProviders")
  .WithTags("Providers");

// GET /api/v1/providers/world-catalog
app.MapGet("/api/v1/providers/world-catalog", () =>
{
    var list = WorldProviderCatalog.GetAllStatuses();
    var aladdin = list.FirstOrDefault(p => p.IsAladdinFamily && p.Id == "aladdin-etoken");
    var attached = WorldProviderCatalog.GetAttachedTokens();

    return Results.Ok(new
    {
        totalProviders = list.Count,
        aladdinFamilyDetected = list.Any(p => p.IsAladdinFamily && p.Readiness == "READY"),
        aladdinEtokenReady = aladdin?.Readiness == "READY",
        hasAttachedToken = attached.Count > 0,
        attachedTokenCount = attached.Count,
        attachedTokens = attached,
        providers = list
    });
}).WithName("GetWorldProvidersCatalog")
  .WithTags("Providers");

// POST /api/v1/providers/scan
app.MapPost("/api/v1/providers/scan", async (
    ProviderManager providerManager,
    WebSocketBridgeService wsService) =>
{
    var list = WorldProviderCatalog.GetAllStatuses();
    var certs = await providerManager.GetAllCertificatesAsync();
    var attached = WorldProviderCatalog.GetAttachedTokens();

    await wsService.BroadcastEventAsync("providers_scanned", new
    {
        providersCount = list.Count,
        readyCount = list.Count(p => p.Readiness == "READY"),
        certificatesCount = certs.Count,
        attachedTokensCount = attached.Count
    });

    return Results.Ok(new
    {
        success = true,
        scannedAt = DateTimeOffset.UtcNow.ToString("O"),
        totalProviders = list.Count,
        readyProviders = list.Count(p => p.Readiness == "READY"),
        certificatesFound = certs.Count,
        hasAttachedToken = attached.Count > 0,
        attachedTokenCount = attached.Count,
        attachedTokens = attached,
        providers = list
    });
}).WithName("ScanHardwareProviders")
  .WithTags("Providers");

// GET /api/v1/certificates
app.MapGet("/api/v1/certificates", async (ProviderManager providerManager) =>
{
    var certs = await providerManager.GetAllCertificatesAsync();
    return Results.Ok(certs);
}).WithName("GetCertificates")
  .WithTags("Certificates");

// GET /api/v1/certificates/{id}
app.MapGet("/api/v1/certificates/{id}", async (string id, ProviderManager providerManager) =>
{
    try
    {
        var (_, cert) = await providerManager.ResolveCertificateAsync(id);
        return Results.Ok(cert);
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound(new ApiErrorResponse(new ErrorDetail(
            Code: ErrorCodes.CertificateNotFound,
            Message: $"Certificate with ID '{id}' was not found in any active provider."
        )));
    }
}).WithName("GetCertificateById")
  .WithTags("Certificates");

// POST /api/v1/session
app.MapPost("/api/v1/session", async (
    HttpContext ctx,
    [FromBody] SessionRequest? request,
    ISessionStore sessionStore) =>
{
    var reqOrigin = request?.Origin ?? ctx.Request.Headers.Origin.FirstOrDefault() ?? "http://localhost:5173";
    var appId = request?.ClientAppId ?? "BrowserPortal";

    var session = await sessionStore.CreateSessionAsync(appId, reqOrigin);
    return Results.Ok(new SessionResponse(
        Success: true,
        SessionId: session.SessionId,
        CreatedAt: session.CreatedAt,
        ExpiresAt: session.ExpiresAt,
        AllowedOrigin: session.AllowedOrigin
    ));
}).WithName("CreateSession")
  .WithTags("Sessions");

// POST /api/v1/sign
app.MapPost("/api/v1/sign", async (
    HttpContext ctx,
    [FromBody] SignRequest signReq,
    ProviderManager providerManager,
    ISessionStore sessionStore,
    IAuditRepository auditRepo,
    WebSocketBridgeService wsService) =>
{
    var sw = Stopwatch.StartNew();
    var clientIp = ctx.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
    var origin = ctx.Request.Headers.Origin.FirstOrDefault() ?? "localhost";

    // 1. Session Authorization Verification
    var sessionToken = ctx.Request.Headers["X-Session-Token"].ToString();
    if (string.IsNullOrWhiteSpace(sessionToken) && ctx.Request.Headers.TryGetValue("Authorization", out var authHeader))
    {
        var authVal = authHeader.ToString();
        if (authVal.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            sessionToken = authVal.Substring("Bearer ".Length).Trim();
        }
    }

    if (!string.IsNullOrWhiteSpace(sessionToken))
    {
        var isSessionValid = await sessionStore.ValidateSessionAsync(sessionToken, origin);
        if (!isSessionValid)
        {
            await auditRepo.RecordAsync(new AuditEntry(
                Id: Guid.NewGuid().ToString("N"),
                Timestamp: DateTimeOffset.UtcNow,
                SessionIdHash: HashUtility.ComputeSha256Hex(sessionToken),
                CertificateId: signReq?.CertificateId,
                Provider: "SecurityFilter",
                Operation: "SIGN",
                Success: false,
                ErrorCode: ErrorCodes.InvalidSession,
                DurationMs: sw.Elapsed.TotalMilliseconds,
                ClientIp: clientIp,
                Origin: origin,
                PayloadDigestSha256: null,
                Details: "Session token is invalid or expired."
            ));

            return Results.Json(new ApiErrorResponse(new ErrorDetail(
                Code: ErrorCodes.InvalidSession,
                Message: "The provided session token is invalid or has expired."
            )), statusCode: StatusCodes.Status401Unauthorized);
        }
    }

    // 2. Validate Request
    if (signReq == null || string.IsNullOrWhiteSpace(signReq.CertificateId) || string.IsNullOrWhiteSpace(signReq.Data))
    {
        return Results.BadRequest(new ApiErrorResponse(new ErrorDetail(
            Code: ErrorCodes.MalformedRequest,
            Message: "CertificateId and Data (Base64) must be provided."
        )));
    }

    // 3. Algorithm Validation
    if (AlgorithmValidator.IsExplicitlyWeak(signReq.HashAlgorithm))
    {
        return Results.BadRequest(new ApiErrorResponse(new ErrorDetail(
            Code: ErrorCodes.WeakAlgorithmRejected,
            Message: $"Cryptographic algorithm '{signReq.HashAlgorithm}' is insecure and explicitly rejected. Use SHA-256, SHA-384, or SHA-512."
        )));
    }

    if (!AlgorithmValidator.IsAllowed(signReq.HashAlgorithm))
    {
        return Results.BadRequest(new ApiErrorResponse(new ErrorDetail(
            Code: ErrorCodes.UnsupportedAlgorithm,
            Message: $"Algorithm '{signReq.HashAlgorithm}' is unsupported. Permitted: SHA-256, SHA-384, SHA-512."
        )));
    }

    // 4. Resolve Certificate & Provider
    ISigningProvider provider;
    Certificate cert;
    try
    {
        (provider, cert) = await providerManager.ResolveCertificateAsync(signReq.CertificateId);
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound(new ApiErrorResponse(new ErrorDetail(
            Code: ErrorCodes.CertificateNotFound,
            Message: $"Certificate '{signReq.CertificateId}' not found."
        )));
    }

    if (!cert.HasPrivateKey)
    {
        return Results.BadRequest(new ApiErrorResponse(new ErrorDetail(
            Code: ErrorCodes.NoPrivateKey,
            Message: $"Certificate '{cert.Subject}' does not have an accessible private key."
        )));
    }

    // 5. Decode payload
    byte[] rawData;
    try
    {
        rawData = Convert.FromBase64String(signReq.Data);
    }
    catch (FormatException)
    {
        return Results.BadRequest(new ApiErrorResponse(new ErrorDetail(
            Code: ErrorCodes.MalformedRequest,
            Message: "Data payload must be valid Base64 encoded bytes."
        )));
    }

    var payloadDigest = HashUtility.ComputeSha256Hex(rawData);

    // 6. Broadcast WebSocket 'sign_started'
    await wsService.BroadcastEventAsync("sign_started", new
    {
        certificateId = cert.Id,
        subject = cert.Subject,
        algorithm = signReq.HashAlgorithm
    });

    // 7. Perform Hardware/Provider Sign Operation
    try
    {
        var signatureBytes = await provider.SignAsync(cert.Id, signReq.HashAlgorithm, rawData);
        sw.Stop();
        var durationMs = sw.Elapsed.TotalMilliseconds;
        var signatureBase64 = Convert.ToBase64String(signatureBytes);

        // Record Audit Entry (Never record raw data, PIN, or private keys!)
        await auditRepo.RecordAsync(new AuditEntry(
            Id: Guid.NewGuid().ToString("N"),
            Timestamp: DateTimeOffset.UtcNow,
            SessionIdHash: string.IsNullOrWhiteSpace(sessionToken) ? null : HashUtility.ComputeSha256Hex(sessionToken),
            CertificateId: cert.Id,
            Provider: provider.GetProviderInfo().Name,
            Operation: "SIGN",
            Success: true,
            ErrorCode: null,
            DurationMs: durationMs,
            ClientIp: clientIp,
            Origin: origin,
            PayloadDigestSha256: payloadDigest,
            Details: $"Signed with {signReq.HashAlgorithm} via {provider.GetProviderInfo().Type}"
        ));

        // Broadcast WebSocket 'sign_completed'
        await wsService.BroadcastEventAsync("sign_completed", new
        {
            certificateId = cert.Id,
            durationMs,
            success = true
        });

        return Results.Ok(new SignResponse(
            Success: true,
            Signature: signatureBase64,
            Certificate: new SignCertificateSummary(
                Subject: cert.Subject,
                Issuer: cert.Issuer,
                SerialNumber: cert.SerialNumber,
                Thumbprint: cert.Thumbprint
            ),
            Algorithm: signReq.HashAlgorithm,
            Timestamp: DateTimeOffset.UtcNow.ToString("O"),
            DurationMs: Math.Round(durationMs, 2),
            ProviderId: provider.GetProviderInfo().Id
        ));
    }
    catch (Exception ex)
    {
        sw.Stop();
        var durationMs = sw.Elapsed.TotalMilliseconds;

        await auditRepo.RecordAsync(new AuditEntry(
            Id: Guid.NewGuid().ToString("N"),
            Timestamp: DateTimeOffset.UtcNow,
            SessionIdHash: string.IsNullOrWhiteSpace(sessionToken) ? null : HashUtility.ComputeSha256Hex(sessionToken),
            CertificateId: cert.Id,
            Provider: provider.GetProviderInfo().Name,
            Operation: "SIGN",
            Success: false,
            ErrorCode: ErrorCodes.SigningFailed,
            DurationMs: durationMs,
            ClientIp: clientIp,
            Origin: origin,
            PayloadDigestSha256: payloadDigest,
            Details: ex.Message
        ));

        await wsService.BroadcastEventAsync("sign_failed", new
        {
            certificateId = cert.Id,
            error = ex.Message
        });

        return Results.Json(new ApiErrorResponse(new ErrorDetail(
            Code: ErrorCodes.SigningFailed,
            Message: $"Cryptographic provider signing failed: {ex.Message}",
            RequestId: ctx.TraceIdentifier
        )), statusCode: StatusCodes.Status500InternalServerError);
    }
}).WithName("SignData")
  .WithTags("Signing");

// GET /api/v1/hardware/readers
app.MapGet("/api/v1/hardware/readers", () =>
{
    var readers = PcscHardwareMonitor.ScanReaders();
    return Results.Ok(new
    {
        totalReaders = readers.Count,
        hasCardInserted = readers.Any(r => r.CardPresent),
        readers = readers
    });
}).WithName("ScanSmartCardReaders")
  .WithTags("Hardware");

// POST /api/v1/sign/pdf
app.MapPost("/api/v1/sign/pdf", async (
    HttpContext ctx,
    [FromBody] PdfSignRequest pdfReq,
    ProviderManager providerManager,
    ISessionStore sessionStore,
    IAuditRepository auditRepo,
    WebSocketBridgeService wsService) =>
{
    var sw = Stopwatch.StartNew();
    var clientIp = ctx.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
    var origin = ctx.Request.Headers.Origin.FirstOrDefault() ?? "localhost";

    if (pdfReq == null || string.IsNullOrWhiteSpace(pdfReq.CertificateId))
    {
        return Results.BadRequest(new ApiErrorResponse(new ErrorDetail(
            Code: ErrorCodes.MalformedRequest,
            Message: "CertificateId must be provided."
        )));
    }

    ISigningProvider provider;
    Certificate cert;
    try
    {
        (provider, cert) = await providerManager.ResolveCertificateAsync(pdfReq.CertificateId);
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound(new ApiErrorResponse(new ErrorDetail(
            Code: ErrorCodes.CertificateNotFound,
            Message: $"Certificate '{pdfReq.CertificateId}' not found."
        )));
    }

    var x509Cert = await provider.GetX509CertificateAsync(cert.Id);
    if (x509Cert == null)
    {
        return Results.BadRequest(new ApiErrorResponse(new ErrorDetail(
            Code: ErrorCodes.SigningFailed,
            Message: "Underlying X509 certificate object could not be resolved from provider."
        )));
    }

    byte[] pdfBytes;
    if (string.IsNullOrWhiteSpace(pdfReq.PdfBase64))
    {
        if (pdfReq.CreateSampleIfEmpty)
        {
            pdfBytes = PdfSignerEngine.GenerateBasePdfDocument("Standard PDF created for signing by Universal Local Signing Bridge");
        }
        else
        {
            return Results.BadRequest(new ApiErrorResponse(new ErrorDetail(
                Code: ErrorCodes.MalformedRequest,
                Message: "PdfBase64 payload is required."
            )));
        }
    }
    else
    {
        try
        {
            pdfBytes = Convert.FromBase64String(pdfReq.PdfBase64);
        }
        catch (FormatException)
        {
            return Results.BadRequest(new ApiErrorResponse(new ErrorDetail(
                Code: ErrorCodes.MalformedRequest,
                Message: "PdfBase64 payload must be valid Base64."
            )));
        }
    }

    var visualOptions = new VisualSignatureOptions(
        Enabled: pdfReq.EnableVisualSignature,
        PageNumber: pdfReq.PageNumber,
        X: pdfReq.X,
        Y: pdfReq.Y,
        Width: pdfReq.Width,
        Height: pdfReq.Height,
        SignerName: cert.Subject,
        Reason: pdfReq.Reason,
        Location: pdfReq.Location,
        ContactInfo: pdfReq.ContactInfo
    );

    try
    {
        var result = PdfSignerEngine.SignPdf(pdfBytes, x509Cert, visualOptions);
        string? tsToken = null;

        if (!string.IsNullOrWhiteSpace(pdfReq.TimestampUrl))
        {
            try
            {
                var hashBytes = Convert.FromHexString(result.DocumentHashSha256);
                var tsRes = await TsaClient.RequestTimestampAsync(hashBytes, pdfReq.TimestampUrl);
                if (tsRes.Success)
                {
                    tsToken = tsRes.TimestampTokenBase64;
                }
            }
            catch { }
        }

        sw.Stop();
        var durationMs = sw.Elapsed.TotalMilliseconds;

        await auditRepo.RecordAsync(new AuditEntry(
            Id: Guid.NewGuid().ToString("N"),
            Timestamp: DateTimeOffset.UtcNow,
            SessionIdHash: null,
            CertificateId: cert.Id,
            Provider: provider.GetProviderInfo().Name,
            Operation: "SIGN_PDF_PADES",
            Success: true,
            ErrorCode: null,
            DurationMs: durationMs,
            ClientIp: clientIp,
            Origin: origin,
            PayloadDigestSha256: result.DocumentHashSha256,
            Details: $"PAdES-BES signed {result.TotalBytes} bytes via {provider.GetProviderInfo().Type}"
        ));

        await wsService.BroadcastEventAsync("pdf_signed", new
        {
            certificateId = cert.Id,
            hash = result.DocumentHashSha256,
            bytes = result.TotalBytes,
            durationMs
        });

        return Results.Ok(new PdfSignResponse(
            Success: true,
            SignedPdfBase64: Convert.ToBase64String(result.SignedPdfBytes),
            DocumentHashSha256: result.DocumentHashSha256,
            SignerSubject: result.SignerSubject,
            SignerThumbprint: result.SignerThumbprint,
            SignatureFormat: result.SignatureFormat,
            TotalBytes: result.TotalBytes,
            Timestamp: result.Timestamp,
            DurationMs: Math.Round(durationMs, 2),
            ProviderId: provider.GetProviderInfo().Id,
            TimestampTokenBase64: tsToken
        ));
    }
    catch (Exception ex)
    {
        return Results.Json(new ApiErrorResponse(new ErrorDetail(
            Code: ErrorCodes.SigningFailed,
            Message: $"PAdES PDF signing failed: {ex.Message}",
            RequestId: ctx.TraceIdentifier
        )), statusCode: StatusCodes.Status500InternalServerError);
    }
}).WithName("SignPdfDocument")
  .WithTags("Signing");

// POST /api/v1/sign/cades
app.MapPost("/api/v1/sign/cades", async (
    HttpContext ctx,
    [FromBody] CadesSignRequest req,
    ProviderManager providerManager,
    IAuditRepository auditRepo) =>
{
    var sw = Stopwatch.StartNew();
    var clientIp = ctx.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
    var origin = ctx.Request.Headers.Origin.FirstOrDefault() ?? "localhost";

    if (req == null || string.IsNullOrWhiteSpace(req.CertificateId) || string.IsNullOrWhiteSpace(req.Data))
    {
        return Results.BadRequest(new ApiErrorResponse(new ErrorDetail(
            Code: ErrorCodes.MalformedRequest,
            Message: "CertificateId and Data must be provided."
        )));
    }

    ISigningProvider provider;
    Certificate cert;
    try
    {
        (provider, cert) = await providerManager.ResolveCertificateAsync(req.CertificateId);
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound(new ApiErrorResponse(new ErrorDetail(
            Code: ErrorCodes.CertificateNotFound,
            Message: $"Certificate '{req.CertificateId}' not found."
        )));
    }

    var x509Cert = await provider.GetX509CertificateAsync(cert.Id);
    if (x509Cert == null)
    {
        return Results.BadRequest(new ApiErrorResponse(new ErrorDetail(
            Code: ErrorCodes.SigningFailed,
            Message: "Underlying X509 certificate object could not be resolved from provider."
        )));
    }

    byte[] rawBytes;
    try
    {
        rawBytes = Convert.FromBase64String(req.Data);
    }
    catch (FormatException)
    {
        return Results.BadRequest(new ApiErrorResponse(new ErrorDetail(
            Code: ErrorCodes.MalformedRequest,
            Message: "Data payload must be Base64 encoded."
        )));
    }

    try
    {
        var hashAlgo = AlgorithmValidator.ToHashAlgorithmName(req.HashAlgorithm);
        var result = CadesSignerEngine.SignDetached(rawBytes, x509Cert, hashAlgo, req.Detached);
        sw.Stop();

        await auditRepo.RecordAsync(new AuditEntry(
            Id: Guid.NewGuid().ToString("N"),
            Timestamp: DateTimeOffset.UtcNow,
            SessionIdHash: null,
            CertificateId: cert.Id,
            Provider: provider.GetProviderInfo().Name,
            Operation: "SIGN_CADES",
            Success: true,
            ErrorCode: null,
            DurationMs: sw.Elapsed.TotalMilliseconds,
            ClientIp: clientIp,
            Origin: origin,
            PayloadDigestSha256: HashUtility.ComputeSha256Hex(rawBytes),
            Details: $"CAdES-BES signature generated ({req.HashAlgorithm})"
        ));

        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Json(new ApiErrorResponse(new ErrorDetail(
            Code: ErrorCodes.SigningFailed,
            Message: $"CAdES signing failed: {ex.Message}",
            RequestId: ctx.TraceIdentifier
        )), statusCode: StatusCodes.Status500InternalServerError);
    }
}).WithName("SignCades")
  .WithTags("Signing");

// POST /api/v1/sign/xml
app.MapPost("/api/v1/sign/xml", async (
    HttpContext ctx,
    [FromBody] XadesSignRequest req,
    ProviderManager providerManager,
    IAuditRepository auditRepo) =>
{
    var sw = Stopwatch.StartNew();
    var clientIp = ctx.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
    var origin = ctx.Request.Headers.Origin.FirstOrDefault() ?? "localhost";

    if (req == null || string.IsNullOrWhiteSpace(req.CertificateId) || string.IsNullOrWhiteSpace(req.XmlContent))
    {
        return Results.BadRequest(new ApiErrorResponse(new ErrorDetail(
            Code: ErrorCodes.MalformedRequest,
            Message: "CertificateId and XmlContent must be provided."
        )));
    }

    ISigningProvider provider;
    Certificate cert;
    try
    {
        (provider, cert) = await providerManager.ResolveCertificateAsync(req.CertificateId);
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound(new ApiErrorResponse(new ErrorDetail(
            Code: ErrorCodes.CertificateNotFound,
            Message: $"Certificate '{req.CertificateId}' not found."
        )));
    }

    var x509Cert = await provider.GetX509CertificateAsync(cert.Id);
    if (x509Cert == null)
    {
        return Results.BadRequest(new ApiErrorResponse(new ErrorDetail(
            Code: ErrorCodes.SigningFailed,
            Message: "Underlying X509 certificate object could not be resolved from provider."
        )));
    }

    try
    {
        var result = XadesSignerEngine.SignEnveloped(req.XmlContent, x509Cert);
        sw.Stop();

        await auditRepo.RecordAsync(new AuditEntry(
            Id: Guid.NewGuid().ToString("N"),
            Timestamp: DateTimeOffset.UtcNow,
            SessionIdHash: null,
            CertificateId: cert.Id,
            Provider: provider.GetProviderInfo().Name,
            Operation: "SIGN_XML_XADES",
            Success: true,
            ErrorCode: null,
            DurationMs: sw.Elapsed.TotalMilliseconds,
            ClientIp: clientIp,
            Origin: origin,
            PayloadDigestSha256: null,
            Details: "XAdES / XMLDSIG enveloped signature generated"
        ));

        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Json(new ApiErrorResponse(new ErrorDetail(
            Code: ErrorCodes.SigningFailed,
            Message: $"XML signing failed: {ex.Message}",
            RequestId: ctx.TraceIdentifier
        )), statusCode: StatusCodes.Status500InternalServerError);
    }
}).WithName("SignXml")
  .WithTags("Signing");

// POST /api/v1/timestamp
app.MapPost("/api/v1/timestamp", async ([FromBody] TimestampRequest req) =>
{
    if (req == null || string.IsNullOrWhiteSpace(req.DataDigestBase64))
    {
        return Results.BadRequest(new ApiErrorResponse(new ErrorDetail(
            Code: ErrorCodes.MalformedRequest,
            Message: "DataDigestBase64 must be provided."
        )));
    }

    byte[] digestBytes;
    try
    {
        digestBytes = Convert.FromBase64String(req.DataDigestBase64);
    }
    catch (FormatException)
    {
        return Results.BadRequest(new ApiErrorResponse(new ErrorDetail(
            Code: ErrorCodes.MalformedRequest,
            Message: "DataDigestBase64 must be valid Base64."
        )));
    }

    var tsaUrl = string.IsNullOrWhiteSpace(req.TsaUrl) ? "http://timestamp.digicert.com" : req.TsaUrl;
    var result = await TsaClient.RequestTimestampAsync(digestBytes, tsaUrl);
    return Results.Ok(result);
}).WithName("RequestTimestamp")
  .WithTags("Timestamp");

// POST /api/v1/sign/batch
app.MapPost("/api/v1/sign/batch", async (
    [FromBody] BatchSignRequest req,
    ProviderManager providerManager) =>
{
    var sw = Stopwatch.StartNew();
    if (req == null || string.IsNullOrWhiteSpace(req.CertificateId) || req.Items == null || req.Items.Count == 0)
    {
        return Results.BadRequest(new ApiErrorResponse(new ErrorDetail(
            Code: ErrorCodes.MalformedRequest,
            Message: "CertificateId and non-empty Items list required."
        )));
    }

    ISigningProvider provider;
    Certificate cert;
    try
    {
        (provider, cert) = await providerManager.ResolveCertificateAsync(req.CertificateId);
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound(new ApiErrorResponse(new ErrorDetail(
            Code: ErrorCodes.CertificateNotFound,
            Message: $"Certificate '{req.CertificateId}' not found."
        )));
    }

    var results = new List<BatchSignItemResult>();
    int successCount = 0;

    foreach (var item in req.Items)
    {
        try
        {
            var rawBytes = Convert.FromBase64String(item.DataBase64);
            var sig = await provider.SignAsync(cert.Id, req.HashAlgorithm, rawBytes);
            results.Add(new BatchSignItemResult(item.ItemId, true, Convert.ToBase64String(sig), null));
            successCount++;
        }
        catch (Exception ex)
        {
            results.Add(new BatchSignItemResult(item.ItemId, false, null, ex.Message));
        }
    }

    sw.Stop();
    return Results.Ok(new BatchSignResponse(
        Success: successCount == req.Items.Count,
        Results: results,
        TotalProcessed: req.Items.Count,
        SuccessCount: successCount,
        DurationMs: Math.Round(sw.Elapsed.TotalMilliseconds, 2)
    ));
}).WithName("BatchSign")
  .WithTags("Signing");

// GET /api/v1/audit
app.MapGet("/api/v1/audit", async (IAuditRepository auditRepo, [FromQuery] int limit = 50) =>
{
    var logs = await auditRepo.GetRecentAsync(Math.Clamp(limit, 1, 200));
    return Results.Ok(logs);
}).WithName("GetAuditLogs")
  .WithTags("Audit");

// WebSocket /ws/v1
app.Map("/ws/v1", async (HttpContext ctx, WebSocketBridgeService wsService) =>
{
    await wsService.HandleWebSocketAsync(ctx);
});

// SPA Fallback
app.MapFallbackToFile("index.html");

Log.Information("==========================================================");
Log.Information("  Universal Local Signing Bridge v1.0.0 (.NET 8)");
Log.Information("  Host: {Host}:{Port}", bridgeConfig.Host, actualPort);
Log.Information("  Mode: {Mode} (MockMode={MockMode})", bridgeConfig.ProviderMode, bridgeConfig.MockMode);
Log.Information("  Swagger: http://{Host}:{Port}/swagger", bridgeConfig.Host, actualPort);
Log.Information("==========================================================");

app.Run();

public partial class Program { }
