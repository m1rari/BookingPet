namespace ApiGateway.Ocelot.Services;

/// <summary>
/// Service for managing service tokens (Client Credentials)
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Gets a valid service token for inter-service communication
    /// </summary>
    /// <returns>Access token for service-to-service calls</returns>
    Task<string> GetServiceTokenAsync();
}
