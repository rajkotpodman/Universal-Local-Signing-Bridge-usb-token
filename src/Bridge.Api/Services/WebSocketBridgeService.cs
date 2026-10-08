using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Bridge.Core.Models;
using Bridge.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bridge.Api.Services;

public class WebSocketBridgeService
{
    private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();
    private readonly ProviderManager _providerManager;
    private readonly ILogger<WebSocketBridgeService> _logger;

    public WebSocketBridgeService(ProviderManager providerManager, ILogger<WebSocketBridgeService> logger)
    {
        _providerManager = providerManager;
        _logger = logger;
    }

    public int ConnectedClientCount => _sockets.Count;

    public async Task HandleWebSocketAsync(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        using var ws = await context.WebSockets.AcceptWebSocketAsync();
        var socketId = Guid.NewGuid().ToString("N");
        _sockets[socketId] = ws;

        _logger.LogInformation("WebSocket client connected: {SocketId}", socketId);

        // 1. Send initial 'connected' event
        await SendJsonAsync(ws, new
        {
            @event = "connected",
            socketId,
            timestamp = DateTimeOffset.UtcNow,
            message = "Connected to Universal Local Signing Bridge v1.0"
        });

        // 2. Send current 'service_status' and 'provider_status'
        await SendJsonAsync(ws, new
        {
            @event = "service_status",
            status = "Running",
            providers = _providerManager.GetAllProvidersInfo()
        });

        var buffer = new byte[4096];
        try
        {
            while (ws.State == WebSocketState.Open)
            {
                var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    break;
                }

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    await HandleIncomingMessage(ws, msg);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "WebSocket disconnected: {SocketId}", socketId);
        }
        finally
        {
            _sockets.TryRemove(socketId, out _);
            _logger.LogInformation("WebSocket client removed: {SocketId}", socketId);
        }
    }

    public async Task BroadcastEventAsync(string eventName, object data)
    {
        var payload = JsonSerializer.Serialize(new
        {
            @event = eventName,
            timestamp = DateTimeOffset.UtcNow,
            data
        });
        var bytes = Encoding.UTF8.GetBytes(payload);
        var segment = new ArraySegment<byte>(bytes);

        foreach (var (id, ws) in _sockets)
        {
            if (ws.State == WebSocketState.Open)
            {
                try
                {
                    await ws.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch
                {
                    // Ignore broadcast errors for individual socket
                }
            }
        }
    }

    private async Task HandleIncomingMessage(WebSocket ws, string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.TryGetProperty("action", out var actionProp))
            {
                var action = actionProp.GetString();
                if (action == "ping")
                {
                    await SendJsonAsync(ws, new { @event = "pong", timestamp = DateTimeOffset.UtcNow });
                }
                else if (action == "get_certificates")
                {
                    var certs = await _providerManager.GetAllCertificatesAsync();
                    await SendJsonAsync(ws, new { @event = "certificate_changed", certificates = certs });
                }
                else if (action == "get_providers")
                {
                    var provs = _providerManager.GetAllProvidersInfo();
                    await SendJsonAsync(ws, new { @event = "provider_status", providers = provs });
                }
            }
        }
        catch { }
    }

    private static async Task SendJsonAsync(WebSocket ws, object obj)
    {
        if (ws.State == WebSocketState.Open)
        {
            var json = JsonSerializer.Serialize(obj);
            var bytes = Encoding.UTF8.GetBytes(json);
            await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
