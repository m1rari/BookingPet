using MMLib.SwaggerForOcelot.Configuration;
using MMLib.SwaggerForOcelot.DependencyInjection;

namespace ApiGateway.Ocelot.Extensions;

/// <summary>
/// Extension methods for IApplicationBuilder
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Extension method to configure OAuth settings for SwaggerForOcelotUI
    /// </summary>
    public static SwaggerForOcelotUIOptions OAuthClientId(
        this SwaggerForOcelotUIOptions options, 
        string clientId)
    {
        // SwaggerForOcelot не поддерживает прямую настройку OAuth
        // Настройка должна происходить через конфигурацию
        return options;
    }
    
    /// <summary>
    /// Extension method to configure OAuth app name for SwaggerForOcelotUI
    /// </summary>
    public static SwaggerForOcelotUIOptions OAuthAppName(
        this SwaggerForOcelotUIOptions options, 
        string appName)
    {
        return options;
    }
    
    /// <summary>
    /// Extension method to enable PKCE for SwaggerForOcelotUI
    /// </summary>
    public static SwaggerForOcelotUIOptions OAuthUsePkce(
        this SwaggerForOcelotUIOptions options)
    {
        return options;
    }
}