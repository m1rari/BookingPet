using Microsoft.AspNetCore.Http;
using Serilog;
using StackExchange.Redis;
using System.Text.Json;

namespace ApiGateway.Ocelot.Middleware;

/// <summary>
/// Middleware для кэширования ответов от микросервисов.
/// </summary>
public class ResponseCachingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDatabase _redis;

    public ResponseCachingMiddleware(RequestDelegate next, IDatabase redis)
    {
        _next = next;
        _redis = redis;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Кэшируем только GET запросы
        if (context.Request.Method != "GET")
        {
            await _next(context);
            return;
        }

        var cacheKey = GenerateCacheKey(context.Request);
        
        // Проверяем кэш
        var cachedResponse = await _redis.StringGetAsync(cacheKey);
        if (cachedResponse.HasValue)
        {
            Log.Information("📦 Cache HIT: {Path} | CorrelationId: {CorrelationId}",
                context.Request.Path,
                context.Items["CorrelationId"]);

            var cachedData = JsonSerializer.Deserialize<CachedResponse>(cachedResponse.ToString())!;
            
            context.Response.StatusCode = cachedData.StatusCode;
            context.Response.ContentType = cachedData.ContentType;
            await context.Response.WriteAsync(cachedData.Body);
            return;
        }

        // Если нет в кэше, выполняем запрос
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        // Кэшируем ответ если он успешный
        if (context.Response.StatusCode == 200)
        {
            responseBody.Seek(0, SeekOrigin.Begin);
            var responseBodyText = await new StreamReader(responseBody).ReadToEndAsync();

            var cacheData = new CachedResponse
            {
                StatusCode = context.Response.StatusCode,
                ContentType = context.Response.ContentType ?? "application/json",
                Body = responseBodyText,
                CachedAt = DateTime.UtcNow
            };

            await _redis.StringSetAsync(cacheKey, JsonSerializer.Serialize(cacheData), TimeSpan.FromMinutes(5));

            Log.Information("💾 Cache STORED: {Path} | CorrelationId: {CorrelationId}",
                context.Request.Path,
                context.Items["CorrelationId"]);
        }

        // Возвращаем оригинальный stream
        responseBody.Seek(0, SeekOrigin.Begin);
        await responseBody.CopyToAsync(originalBodyStream);
    }

    private static string GenerateCacheKey(HttpRequest request)
    {
        var path = request.Path.Value ?? "";
        var query = request.QueryString.Value ?? "";
        return $"api_gateway_cache:{path}:{query}";
    }

    private class CachedResponse
    {
        public int StatusCode { get; set; }
        public string ContentType { get; set; } = "";
        public string Body { get; set; } = "";
        public DateTime CachedAt { get; set; }
    }
}
