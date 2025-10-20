using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Inventory.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServiceAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = jwtSettings["Authority"];
                options.Audience = jwtSettings["Audience"];
                options.RequireHttpsMetadata = false;
            });
            
        return services;
    }
}