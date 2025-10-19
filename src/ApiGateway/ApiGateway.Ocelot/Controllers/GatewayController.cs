using Microsoft.AspNetCore.Mvc;
using Serilog;
using StackExchange.Redis;

namespace ApiGateway.Ocelot.Controllers;

/// <summary>
/// Контроллер для health checks и статистики API Gateway.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class GatewayController : ControllerBase
{
    private readonly IDatabase _redis;
    private readonly ILogger<GatewayController> _logger;

    public GatewayController(IDatabase redis, ILogger<GatewayController> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    /// <summary>
    /// Получить статистику API Gateway.
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        try
        {
            var stats = new
            {
                gateway = new
                {
                    name = "Booking Platform API Gateway",
                    version = "1.0.0",
                    uptime = Environment.TickCount64 / 1000,
                    timestamp = DateTime.UtcNow
                },
                services = new[]
                {
                    new { name = "Inventory Service", port = 5001, status = await CheckServiceHealth("http://localhost:5001/health") },
                    new { name = "Booking Service", port = 5002, status = await CheckServiceHealth("http://localhost:5002/health") },
                    new { name = "User Service", port = 5003, status = await CheckServiceHealth("http://localhost:5003/health") },
                    new { name = "Payment Service", port = 5004, status = await CheckServiceHealth("http://localhost:5004/health") }
                },
                cache = new
                {
                    redis_connected = await CheckRedisConnection(),
                    cache_keys_count = await GetCacheKeysCount()
                },
                rateLimiting = new
                {
                    enabled = true,
                    default_limit = "100 requests per minute"
                }
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting gateway stats");
            return StatusCode(500, new { error = "Failed to get gateway statistics" });
        }
    }

    /// <summary>
    /// Получить информацию о маршрутах.
    /// </summary>
    [HttpGet("routes")]
    public IActionResult GetRoutes()
    {
        var routes = new[]
        {
            new { path = "/api/v1/inventory/*", service = "Inventory Service", port = 5001, methods = new[] { "GET", "POST", "PUT", "DELETE" } },
            new { path = "/api/v1/bookings/*", service = "Booking Service", port = 5002, methods = new[] { "GET", "POST", "PUT", "DELETE" } },
            new { path = "/api/v1/users/*", service = "User Service", port = 5003, methods = new[] { "GET", "POST", "PUT", "DELETE" } },
            new { path = "/api/v1/auth/*", service = "User Service", port = 5003, methods = new[] { "GET", "POST" } },
            new { path = "/api/v1/payments/*", service = "Payment Service", port = 5004, methods = new[] { "GET", "POST", "PUT", "DELETE" } }
        };

        return Ok(new { routes });
    }

    /// <summary>
    /// Очистить кэш API Gateway.
    /// </summary>
    [HttpPost("cache/clear")]
    public async Task<IActionResult> ClearCache()
    {
        try
        {
            var server = _redis.Multiplexer.GetServer(_redis.Multiplexer.GetEndPoints().First());
            var keys = server.Keys(pattern: "api_gateway_cache:*");
            
            var deletedCount = await _redis.KeyDeleteAsync(keys.ToArray());
            
            _logger.LogInformation("🗑️ Cache cleared: {Count} keys deleted", deletedCount);
            
            return Ok(new { message = $"Cache cleared successfully. {deletedCount} keys deleted." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
            return StatusCode(500, new { error = "Failed to clear cache" });
        }
    }

    private async Task<string> CheckServiceHealth(string url)
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(5);
            
            var response = await httpClient.GetAsync(url);
            return response.IsSuccessStatusCode ? "healthy" : "unhealthy";
        }
        catch
        {
            return "unreachable";
        }
    }

    private async Task<bool> CheckRedisConnection()
    {
        try
        {
            await _redis.PingAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<long> GetCacheKeysCount()
    {
        try
        {
            var server = _redis.Multiplexer.GetServer(_redis.Multiplexer.GetEndPoints().First());
            var keys = server.Keys(pattern: "api_gateway_cache:*");
            return keys.Count();
        }
        catch
        {
            return 0;
        }
    }
}
