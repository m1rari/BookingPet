using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ApiGateway.Ocelot.Extensions;

/// <summary>
/// Extension methods for IServiceCollection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Gateway Authentication with JWT Bearer token validation
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddGatewayAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("IdentityServer");
        var authority = jwtSettings["Authority"] ?? "https://localhost:5001";
        var audience = jwtSettings["Audience"] ?? "api.gateway";

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Authority = authority;
                options.Audience = audience;
                options.RequireHttpsMetadata = !configuration.GetValue<bool>("Development:DisableHttps");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromMinutes(5),

                    // For development
                    ValidIssuer = authority,
                    ValidAudience = audience
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<JwtBearerEvents>>();
                        logger.LogWarning("JWT Authentication failed: {Exception}", context.Exception?.Message);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<JwtBearerEvents>>();
                        logger.LogDebug("JWT Token validated for user: {User}",
                            context.Principal?.Identity?.Name ?? "Unknown");
                        return Task.CompletedTask;
                    }
                };

                // Authorization policies
                services.AddAuthorization(options =>
                {
                    options.AddPolicy("UserPolicy", policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        policy.RequireClaim("scope", "api.access");
                    });

                    options.AddPolicy("AdminPolicy", policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        policy.RequireClaim("role", "Admin");
                    });

                    options.AddPolicy("ServicePolicy", policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        policy.RequireClaim("client_id");
                    });
                });


            });
        return services;
    }
}