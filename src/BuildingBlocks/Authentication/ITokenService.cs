namespace BookingPlatform.BuildingBlocks.Authentication;

/// <summary>
/// Service for managing authentication tokens between services
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Gets a client credentials token for service-to-service communication
    /// </summary>
    /// <param name="scopes">Required scopes for the token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Access token</returns>
    Task<string> GetServiceTokenAsync(string[]? scopes = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a cached token or requests a new one if expired
    /// </summary>
    /// <param name="scopes">Required scopes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Access token</returns>
    Task<string> GetCachedServiceTokenAsync(string[]? scopes = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validates if a token is still valid
    /// </summary>
    /// <param name="token">Token to validate</param>
    /// <returns>True if token is valid</returns>
    Task<bool> IsTokenValidAsync(string token);
    
    /// <summary>
    /// Clears cached tokens (useful for credential rotation)
    /// </summary>
    void ClearTokenCache();
}