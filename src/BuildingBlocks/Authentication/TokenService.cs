using IdentityModel.Client;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BookingPlatform.BuildingBlocks.Authentication;

/// <summary>
/// Implementation of token service for client credentials authentication
/// </summary>
public class TokenService : ITokenService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TokenService> _logger;
    private readonly TokenServiceOptions _options;
    
    public TokenService(
        HttpClient httpClient, 
        IMemoryCache cache, 
        ILogger<TokenService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
        _options = new TokenServiceOptions(configuration);
    }

    public async Task<string> GetServiceTokenAsync(string[]? scopes = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Requesting new service token from IdentityServer");
            
            // Get discovery document
            var discoveryDoc = await _httpClient.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = _options.Authority,
                Policy = { RequireHttps = _options.RequireHttps }
            }, cancellationToken);
            
            if (discoveryDoc.IsError)
            {
                _logger.LogError("Failed to get discovery document: {Error}", discoveryDoc.Error);
                throw new InvalidOperationException($"Failed to get discovery document: {discoveryDoc.Error}");
            }

            // Request client credentials token
            var tokenRequest = new ClientCredentialsTokenRequest
            {
                Address = discoveryDoc.TokenEndpoint,
                ClientId = _options.ClientId,
                ClientSecret = _options.ClientSecret,
                Scope = string.Join(" ", scopes ?? _options.DefaultScopes)
            };

            var tokenResponse = await _httpClient.RequestClientCredentialsTokenAsync(tokenRequest, cancellationToken);
            
            if (tokenResponse.IsError)
            {
                _logger.LogError("Failed to get access token: {Error} - {ErrorDescription}", 
                    tokenResponse.Error, tokenResponse.ErrorDescription);
                throw new InvalidOperationException($"Failed to get access token: {tokenResponse.Error}");
            }

            _logger.LogDebug("Successfully obtained service token. Expires in: {ExpiresIn} seconds", 
                tokenResponse.ExpiresIn);
            
            return tokenResponse.AccessToken!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service token");
            throw;
        }
    }

    public async Task<string> GetCachedServiceTokenAsync(string[]? scopes = null, CancellationToken cancellationToken = default)
    {
        var scopeKey = string.Join(":", scopes ?? _options.DefaultScopes);
        var cacheKey = $"service_token:{_options.ClientId}:{scopeKey}";
        
        if (_cache.TryGetValue(cacheKey, out string? cachedToken) && !string.IsNullOrEmpty(cachedToken))
        {
            _logger.LogDebug("Using cached service token");
            
            // Check if token is close to expiration (refresh 5 minutes before expiry)
            if (await IsTokenValidAsync(cachedToken))
            {
                return cachedToken;
            }
            
            _logger.LogDebug("Cached token is expired or close to expiry, requesting new token");
            _cache.Remove(cacheKey);
        }

        // Get new token
        var newToken = await GetServiceTokenAsync(scopes, cancellationToken);
        
        // Cache the token (expire 5 minutes before the actual expiry)
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_options.TokenCacheDurationSeconds - 300),
            SlidingExpiration = null,
            Priority = CacheItemPriority.High
        };
        
        _cache.Set(cacheKey, newToken, cacheOptions);
        
        return newToken;
    }

    public async Task<bool> IsTokenValidAsync(string token)
    {
        try
        {
            if (string.IsNullOrEmpty(token))
                return false;
            
            // Simple JWT expiration check without full validation
            var tokenParts = token.Split('.');
            if (tokenParts.Length != 3)
                return false;
            
            // Decode payload
            var payload = tokenParts[1];
            // Add padding if necessary
            while (payload.Length % 4 != 0)
                payload += "=";
            
            var payloadBytes = Convert.FromBase64String(payload);
            var payloadJson = System.Text.Encoding.UTF8.GetString(payloadBytes);
            
            using var document = JsonDocument.Parse(payloadJson);
            
            if (document.RootElement.TryGetProperty("exp", out var expElement))
            {
                var exp = expElement.GetInt64();
                var expiryTime = DateTimeOffset.FromUnixTimeSeconds(exp);
                
                // Token is valid if it expires more than 5 minutes from now
                return expiryTime > DateTimeOffset.UtcNow.AddMinutes(5);
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error validating token");
            return false;
        }
    }

    public void ClearTokenCache()
    {
        _logger.LogInformation("Clearing token cache for client {ClientId}", _options.ClientId);
        
        // Note: MemoryCache doesn't have a clear all method, 
        // but we can track keys if needed in the future
        if (_cache is MemoryCache memoryCache)
        {
            memoryCache.Compact(1.0); // Remove all expired items
        }
    }
}

/// <summary>
/// Configuration options for TokenService
/// </summary>
public class TokenServiceOptions
{
    public string Authority { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string[] DefaultScopes { get; set; } = Array.Empty<string>();
    public bool RequireHttps { get; set; } = false;
    public int TokenCacheDurationSeconds { get; set; } = 3600; // 1 hour
    
    public TokenServiceOptions(IConfiguration configuration)
    {
        var section = configuration.GetSection("IdentityServer");
        
        Authority = section["Authority"] ?? "https://localhost:5001";
        ClientId = section["ClientId"] ?? throw new InvalidOperationException("ClientId is required");
        ClientSecret = section["ClientSecret"] ?? throw new InvalidOperationException("ClientSecret is required");
        RequireHttps = section.GetValue<bool>("RequireHttps", false);
        TokenCacheDurationSeconds = section.GetValue<int>("TokenCacheDurationSeconds", 3600);
        
        var scopesConfig = section["DefaultScopes"];
        DefaultScopes = !string.IsNullOrEmpty(scopesConfig) 
            ? scopesConfig.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .ToArray()
            : new[] { "booking.internal" };
    }
}