using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace BookingPlatform.BuildingBlocks.Authentication;

/// <summary>
/// Extensions for configuring service-to-service authentication
/// </summary>
public static class ServiceAuthenticationExtensions
{
    /// <summary>
    /// Configures JWT Bearer authentication for microservices
    /// Accepts tokens from IdentityServer with specific client claims
    /// </summary>
    public static IServiceCollection AddServiceAuthentication(this IServiceCollection services, 
        IConfiguration configuration, string serviceName)
    {
        var identityServerUrl = configuration["IdentityServer:Authority"] ?? "https://localhost:5001";
        var requiredScope = configuration["IdentityServer:RequiredScope"] ?? "booking.internal";
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Authority = identityServerUrl;
                options.RequireHttpsMetadata = false; // Only for development
                
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateAudience = false, // We validate scopes instead
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
                
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<JwtBearerEvents>>();
                            
                        var claims = context.Principal?.Claims.ToList();
                        
                        // Log token validation for debugging
                        logger.LogDebug("Token validated for service {ServiceName}. Claims: {Claims}", 
                            serviceName, string.Join(", ", claims?.Select(c => $"{c.Type}={c.Value}") ?? Array.Empty<string>()));
                            
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<JwtBearerEvents>>();
                            
                        logger.LogWarning("Authentication failed for service {ServiceName}: {Exception}", 
                            serviceName, context.Exception.Message);
                            
                        return Task.CompletedTask;
                    }
                };
            });
        
        // Add authorization policies
        services.AddAuthorization(options =>
        {
            // Policy for API Gateway calls (trusted gateway)
            options.AddPolicy("GatewayPolicy", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("client_id", "booking.gateway");
                policy.RequireClaim("scope", requiredScope);
            });
            
            // Policy for service-to-service calls
            options.AddPolicy("ServicePolicy", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(context =>
                {
                    // Allow calls from gateway or other trusted services
                    var clientId = context.User.FindFirst("client_id")?.Value;
                    var serviceType = context.User.FindFirst("service_type")?.Value;
                    
                    return !string.IsNullOrEmpty(clientId) && (
                        clientId == "booking.gateway" ||
                        clientId.EndsWith(".service") ||
                        serviceType == "gateway");
                });
            });
            
            // Policy for internal service operations  
            options.AddPolicy("InternalPolicy", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", requiredScope);
            });
            
            // Default policy
            options.DefaultPolicy = options.GetPolicy("ServicePolicy") ?? 
                throw new InvalidOperationException("ServicePolicy not found");
        });
        
        return services;
    }
    
    /// <summary>
    /// Configures authentication for API Gateway
    /// Validates user tokens and provides service tokens for downstream calls
    /// </summary>
    public static IServiceCollection AddGatewayAuthentication(this IServiceCollection services, 
        IConfiguration configuration)
    {
        var identityServerUrl = configuration["IdentityServer:Authority"] ?? "https://localhost:5001";
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Authority = identityServerUrl;
                options.RequireHttpsMetadata = false; // Only for development
                
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    RoleClaimType = "role"
                };
                
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<JwtBearerEvents>>();
                            
                        var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        var userName = context.Principal?.FindFirst(ClaimTypes.Name)?.Value;
                        var role = context.Principal?.FindFirst(ClaimTypes.Role)?.Value;
                        
                        logger.LogInformation("User authenticated: {UserId} ({UserName}) with role {Role}", 
                            userId, userName, role);
                            
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<JwtBearerEvents>>();
                            
                        logger.LogWarning("Gateway authentication failed: {Exception}", 
                            context.Exception.Message);
                            
                        return Task.CompletedTask;
                    }
                };
            });
        
        // Add authorization policies for gateway
        services.AddAuthorization(options =>
        {
            options.AddPolicy("UserPolicy", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", "booking.full_access");
            });
            
            options.AddPolicy("AdminPolicy", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("role", "Administrator");
            });
            
            options.AddPolicy("ManagerPolicy", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(context =>
                {
                    var role = context.User.FindFirst("role")?.Value;
                    return role == "Administrator" || role == "Manager";
                });
            });
        });
        
        return services;
    }
}