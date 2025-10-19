using BookingPlatform.BuildingBlocks.Authentication;
using System.Net.Http.Headers;

namespace ApiGateway.Ocelot.Middleware;

/// <summary>
/// Middleware that adds service authentication tokens to downstream requests
/// </summary>
public class ServiceTokenMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ITokenService _tokenService;
    private readonly ILogger<ServiceTokenMiddleware> _logger;
    
    public ServiceTokenMiddleware(
        RequestDelegate next,
        ITokenService tokenService,
        ILogger<ServiceTokenMiddleware> logger)
    {
        _next = next;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only add service tokens to downstream API calls
        if (IsDownstreamApiCall(context.Request.Path))
        {
            try
            {
                // Get service token for downstream call
                var serviceToken = await _tokenService.GetCachedServiceTokenAsync();
                
                if (!string.IsNullOrEmpty(serviceToken))
                {
                    // Add service token header for downstream services
                    context.Request.Headers.Add("X-Service-Token", serviceToken);
                    
                    // Also add to Authorization header if not present (for service calls)
                    if (!context.Request.Headers.ContainsKey("Authorization"))
                    {
                        context.Request.Headers.Add("Authorization", $"Bearer {serviceToken}");
                    }
                    
                    _logger.LogDebug("Added service token to downstream request for path: {Path}", 
                        context.Request.Path);
                }
                else
                {
                    _logger.LogWarning("Failed to obtain service token for downstream call");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding service token to downstream request");
                // Continue without service token - let downstream service handle the auth failure
            }
        }
        
        await _next(context);
    }
    
    private static bool IsDownstreamApiCall(PathString path)
    {
        // Check if this is an API call that should be forwarded to services
        var pathValue = path.Value?.ToLowerInvariant();
        
        if (string.IsNullOrEmpty(pathValue))
            return false;
            
        // API routes that need service tokens
        var apiPrefixes = new[]
        {
            "/api/v1/inventory",
            "/api/v1/bookings", 
            "/api/v1/users",
            "/api/v1/payments",
            "/api/v1/reviews",
            "/api/v1/analytics"
        };
        
        return apiPrefixes.Any(prefix => pathValue.StartsWith(prefix));
    }
}

/// <summary>
/// Extension methods for registering ServiceTokenMiddleware
/// </summary>
public static class ServiceTokenMiddlewareExtensions
{
    public static IApplicationBuilder UseServiceTokenMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ServiceTokenMiddleware>();
    }
}