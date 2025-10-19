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
    /// <param name="options">SwaggerForOcelotUI options</param>
    /// <param name="clientId">OAuth Client ID</param>
    /// <returns>SwaggerForOcelotUI options for chaining</returns>
    public static SwaggerForOcelotUIOptions OAuthClientId(
        this SwaggerForOcelotUIOptions options, 
        string clientId)
    {
        // Configure OAuth client ID for Swagger UI
        options.OAuthConfigObject.ClientId = clientId;
        return options;
    }
    
    /// <summary>
    /// Extension method to configure OAuth app name for SwaggerForOcelotUI
    /// </summary>
    /// <param name="options">SwaggerForOcelotUI options</param>
    /// <param name="appName">OAuth App Name</param>
    /// <returns>SwaggerForOcelotUI options for chaining</returns>
    public static SwaggerForOcelotUIOptions OAuthAppName(
        this SwaggerForOcelotUIOptions options, 
        string appName)
    {
        // Configure OAuth app name for Swagger UI
        options.OAuthConfigObject.AppName = appName;
        return options;
    }
    
    /// <summary>
    /// Extension method to enable PKCE for SwaggerForOcelotUI
    /// </summary>
    /// <param name="options">SwaggerForOcelotUI options</param>
    /// <returns>SwaggerForOcelotUI options for chaining</returns>
    public static SwaggerForOcelotUIOptions OAuthUsePkce(
        this SwaggerForOcelotUIOptions options)
    {
        // Enable PKCE for OAuth in Swagger UI
        options.OAuthConfigObject.UsePkceWithAuthorizationCodeGrant = true;
        return options;
    }
}