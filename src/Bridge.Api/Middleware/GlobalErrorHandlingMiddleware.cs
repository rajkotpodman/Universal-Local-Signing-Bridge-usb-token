using System;
using System.Text.Json;
using System.Threading.Tasks;
using Bridge.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bridge.Api.Middleware;

public class GlobalErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalErrorHandlingMiddleware> _logger;

    public GlobalErrorHandlingMiddleware(RequestDelegate next, ILogger<GlobalErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);

            if (!context.Response.HasStarted)
            {
                context.Response.Clear();
                context.Response.StatusCode = ex switch
                {
                    KeyNotFoundException => StatusCodes.Status404NotFound,
                    ArgumentException => StatusCodes.Status400BadRequest,
                    InvalidOperationException => StatusCodes.Status400BadRequest,
                    UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                    _ => StatusCodes.Status500InternalServerError
                };

                context.Response.ContentType = "application/json";

                var code = ex switch
                {
                    KeyNotFoundException => ErrorCodes.CertificateNotFound,
                    ArgumentException => ErrorCodes.MalformedRequest,
                    _ => ErrorCodes.SigningFailed
                };

                var errorResponse = new ApiErrorResponse(new ErrorDetail(
                    Code: code,
                    Message: ex.Message,
                    RequestId: context.TraceIdentifier
                ));

                await JsonSerializer.SerializeAsync(context.Response.Body, errorResponse);
            }
        }
    }
}
