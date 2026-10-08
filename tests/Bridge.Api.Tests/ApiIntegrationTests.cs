using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Bridge.Core.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Bridge.Api.Tests;

public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var health = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.NotNull(health);
        Assert.Equal("ok", health.Status);
    }

    [Fact]
    public async Task GetStatus_ReturnsOperationalTelemetry()
    {
        var response = await _client.GetAsync("/api/v1/status");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var status = await response.Content.ReadFromJsonAsync<BridgeStatusResponse>();
        Assert.NotNull(status);
        Assert.Equal("Universal Local Signing Bridge", status.Service);
        Assert.Equal("Running", status.Status);
        Assert.True(status.TotalCertificates > 0);
    }

    [Fact]
    public async Task GetProviders_ReturnsConfiguredProviders()
    {
        var response = await _client.GetAsync("/api/v1/providers");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var providers = await response.Content.ReadFromJsonAsync<List<ProviderInfo>>();
        Assert.NotNull(providers);
        Assert.True(providers.Count >= 2);
    }

    [Fact]
    public async Task GetCertificates_ReturnsAvailableCertificates()
    {
        var response = await _client.GetAsync("/api/v1/certificates");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var certs = await response.Content.ReadFromJsonAsync<List<Certificate>>();
        Assert.NotNull(certs);
        Assert.NotEmpty(certs);
    }

    [Fact]
    public async Task CreateSession_ReturnsSessionToken()
    {
        var sessionReq = new SessionRequest("TestIntegrationApp", "http://localhost:5173");
        var response = await _client.PostAsJsonAsync("/api/v1/session", sessionReq);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var session = await response.Content.ReadFromJsonAsync<SessionResponse>();
        Assert.NotNull(session);
        Assert.True(session.Success);
        Assert.False(string.IsNullOrWhiteSpace(session.SessionId));
    }

    [Fact]
    public async Task SignData_WithValidMockCertificate_Succeeds()
    {
        // 1. Get first cert
        var certsResponse = await _client.GetAsync("/api/v1/certificates");
        var certs = await certsResponse.Content.ReadFromJsonAsync<List<Certificate>>();
        Assert.NotNull(certs);
        Assert.NotEmpty(certs);
        var targetCert = certs[0];

        // 2. Establish session
        var sessionResp = await _client.PostAsJsonAsync("/api/v1/session", new SessionRequest("App", "http://localhost:5173"));
        var session = await sessionResp.Content.ReadFromJsonAsync<SessionResponse>();
        Assert.NotNull(session);

        // 3. Post sign request
        var signReq = new SignRequest(
            CertificateId: targetCert.Id,
            HashAlgorithm: "SHA-256",
            Data: Convert.ToBase64String(Encoding.UTF8.GetBytes("Important document to sign"))
        );

        var reqMessage = new HttpRequestMessage(HttpMethod.Post, "/api/v1/sign")
        {
            Content = JsonContent.Create(signReq)
        };
        reqMessage.Headers.Add("X-Session-Token", session.SessionId);
        reqMessage.Headers.Add("Origin", "http://localhost:5173");

        var signResponse = await _client.SendAsync(reqMessage);
        Assert.Equal(HttpStatusCode.OK, signResponse.StatusCode);

        var result = await signResponse.Content.ReadFromJsonAsync<SignResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.False(string.IsNullOrWhiteSpace(result.Signature));
        Assert.Equal("SHA-256", result.Algorithm);
    }

    [Fact]
    public async Task SignData_WithWeakAlgorithm_ReturnsBadRequest()
    {
        var signReq = new SignRequest(
            CertificateId: "dummy",
            HashAlgorithm: "MD5",
            Data: Convert.ToBase64String(Encoding.UTF8.GetBytes("Data"))
        );

        var response = await _client.PostAsJsonAsync("/api/v1/sign", signReq);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var err = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(err);
        Assert.Equal(ErrorCodes.WeakAlgorithmRejected, err.Error.Code);
    }

    [Fact]
    public async Task Request_WithOversizedPayload_ReturnsPayloadTooLarge()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/sign");
        request.Content = new StringContent("{}", Encoding.UTF8, "application/json");
        request.Content.Headers.ContentLength = 10 * 1024 * 1024; // 10 MB > 5 MB limit

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
    }

    [Fact]
    public async Task Request_WithUnauthorizedOrigin_ReturnsForbidden()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", "http://malicious-site.com");

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetAuditLogs_ReturnsRecentLogs()
    {
        var response = await _client.GetAsync("/api/v1/audit?limit=10");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var logs = await response.Content.ReadFromJsonAsync<List<AuditEntry>>();
        Assert.NotNull(logs);
    }
}
