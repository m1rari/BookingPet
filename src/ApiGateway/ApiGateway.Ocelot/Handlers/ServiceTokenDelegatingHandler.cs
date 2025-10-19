using ApiGateway.Ocelot.Services;
using IdentityModel.Client;
using Microsoft.Extensions.Logging;

namespace ApiGateway.Ocelot.Handlers;

/// <summary>
/// DelegatingHandler that automatically adds service token to outgoing requests
/// </summary>
public class ServiceTokenDelegatingHandler : DelegatingHandler
{
    private readonly ITokenService _tokenService;
    private readonly ILogger<ServiceTokenDelegatingHandler> _logger;

    public ServiceTokenDelegatingHandler(
        ITokenService tokenService, 
        ILogger<ServiceTokenDelegatingHandler> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var token = await _tokenService.GetServiceTokenAsync();
            request.SetBearerToken(token);
            _logger.LogDebug("Added service token to request: {Uri}", request.RequestUri);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get service token for request: {Uri}", request.RequestUri);
            throw;
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
