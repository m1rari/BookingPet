using Microsoft.AspNetCore.Http;
using Serilog;
using System.Net;

namespace ApiGateway.Ocelot.Middleware;

/// <summary>
/// Middleware для обработки ошибок и возврата стандартизированных ответов.
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "❌ API Gateway Error: {Path} | CorrelationId: {CorrelationId}",
                context.Request.Path,
                context.Items["CorrelationId"]);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = new
            {
                message = "Internal server error",
                correlationId = context.Items["CorrelationId"],
                timestamp = DateTime.UtcNow
            }
        };

        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
}
