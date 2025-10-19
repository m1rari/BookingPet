using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ApiGateway.Ocelot.Services;

/// <summary>
/// Implementation of TokenService for Client Credentials flow
/// </summary>
public class TokenService : ITokenService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TokenService> _logger;
    
    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public TokenService(
        IHttpClientFactory httpClientFactory, 
        IConfiguration configuration,
        ILogger<TokenService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> GetServiceTokenAsync()
    {
        // Check cache first
        if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry)
        {
            _logger.LogDebug("Using cached service token");
            return _cachedToken;
        }

        await _semaphore.WaitAsync();
        try
        {
            // Double-check after acquiring lock
            if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry)
            {
                _logger.LogDebug("Using cached service token (after lock)");
                return _cachedToken;
            }

            _logger.LogInformation("Requesting new service token from IdentityServer");

            var client = _httpClientFactory.CreateClient();
            
            // Get discovery document
            var disco = await client.GetDiscoveryDocumentAsync(
                _configuration["IdentityServer:Authority"]);
            
            if (disco.IsError)
            {
                _logger.LogError("Discovery error: {Error}", disco.Error);
                throw new Exception($"Discovery error: {disco.Error}");
            }

            // Request client credentials token
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(
                new ClientCredentialsTokenRequest
                {
                    Address = disco.TokenEndpoint,
                    ClientId = _configuration["IdentityServer:ClientId"],
                    ClientSecret = _configuration["IdentityServer:ClientSecret"],
                    Scope = "inventory.api bookings.api users.api payments.api reviews.api analytics.api"
                });

            if (tokenResponse.IsError)
            {
                _logger.LogError("Token error: {Error}", tokenResponse.Error);
                throw new Exception($"Token error: {tokenResponse.Error}");
            }

            _cachedToken = tokenResponse.AccessToken;
            _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60); // -60 for safety
            
            _logger.LogInformation("Successfully obtained new service token, expires at {Expiry}", _tokenExpiry);
            
            return _cachedToken!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get service token");
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    /// <summary>
    /// Gets a cached service token, same as GetServiceTokenAsync but with different name for compatibility
    /// </summary>
    /// <returns>Cached access token for service-to-service calls</returns>
    public async Task<string> GetCachedServiceTokenAsync()
    {
        return await GetServiceTokenAsync();
    }
    
    /// <summary>
    /// Clears the token cache, forcing a new token to be retrieved on the next call
    /// </summary>
    public void ClearTokenCache()
    {
        _logger.LogInformation("Clearing service token cache");
        _cachedToken = null;
        _tokenExpiry = DateTime.MinValue;
    }
}