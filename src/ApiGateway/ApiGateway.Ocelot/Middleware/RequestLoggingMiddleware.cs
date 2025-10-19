using Microsoft.AspNetCore.Http;
using Serilog;
using System.Diagnostics;

namespace ApiGateway.Ocelot.Middleware;

/// <summary>
/// Middleware для логирования запросов и добавления correlation ID.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
            ?? Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        var stopwatch = Stopwatch.StartNew();

        Log.Information("🚀 API Gateway Request: {Method} {Path} | CorrelationId: {CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            correlationId);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            Log.Information("✅ API Gateway Response: {StatusCode} | Duration: {Duration}ms | CorrelationId: {CorrelationId}",
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                correlationId);
        }
    }
}
