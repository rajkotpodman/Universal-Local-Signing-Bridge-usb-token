using System.Net;
using System.Text.Json;
using Bridge.Core.Configuration;
using Bridge.Core.Models;
using Bridge.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bridge.Api.Middleware;

public class SecurityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly OriginValidator _originValidator;
    private readonly RateLimiter _rateLimiter;
    private readonly BridgeConfiguration _config;
    private readonly ILogger<SecurityMiddleware> _logger;

    public SecurityMiddleware(
        RequestDelegate next,
        OriginValidator originValidator,
        RateLimiter rateLimiter,
        BridgeConfiguration config,
        ILogger<SecurityMiddleware> logger)
    {
        _next = next;
        _originValidator = originValidator;
        _rateLimiter = rateLimiter;
        _config = config;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Enforce strict loopback only
        var remoteIp = context.Connection.RemoteIpAddress;
        if (remoteIp != null && !LocalhostEnforcer.IsLoopbackAddress(remoteIp))
        {
            _logger.LogWarning("Rejected non-loopback connection attempt from {RemoteIp}", remoteIp);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await WriteErrorJson(context, ErrorCodes.RemoteIpForbidden, "External network access is strictly forbidden. The bridge only listens on localhost.");
            return;
        }

        // 2. Add Security Headers
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("Referrer-Policy", "no-referrer");

        // 3. Payload size check
        if (context.Request.ContentLength.HasValue && context.Request.ContentLength.Value > _config.MaxRequestSizeBytes)
        {
            context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
            await WriteErrorJson(context, ErrorCodes.OversizedPayload, $"Request payload exceeds maximum allowed limit of {_config.MaxRequestSizeBytes / (1024 * 1024)} MB.");
            return;
        }

        // 4. Origin allowlist validation
        var origin = context.Request.Headers["Origin"].ToString();
        if (!string.IsNullOrWhiteSpace(origin) && !_originValidator.IsAllowed(origin))
        {
            _logger.LogWarning("Rejected unauthorized origin: {Origin}", origin);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await WriteErrorJson(context, ErrorCodes.InvalidOrigin, $"Origin '{origin}' is not in the bridge allowed origins list.");
            return;
        }

        // 5. Rate limiting on sensitive signing endpoint
        if (context.Request.Path.StartsWithSegments("/api/v1/sign", StringComparison.OrdinalIgnoreCase))
        {
            var clientKey = remoteIp?.ToString() ?? "localhost";
            if (!_rateLimiter.IsAllowed(clientKey))
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await WriteErrorJson(context, ErrorCodes.RateLimitExceeded, "Signing rate limit exceeded. Please wait before submitting more requests.");
                return;
            }
        }

        await _next(context);
    }

    private static async Task WriteErrorJson(HttpContext context, string code, string message)
    {
        context.Response.ContentType = "application/json";
        var err = new ApiErrorResponse(new ErrorDetail(code, message, context.TraceIdentifier));
        await JsonSerializer.SerializeAsync(context.Response.Body, err);
    }
}
