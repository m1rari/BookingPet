using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ApiGateway.Ocelot.Extensions;

/// <summary>
/// Extension methods for Health Check configuration
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Adds a URL group health check
    /// </summary>
    /// <param name="builder">The health check builder</param>
    /// <param name="uri">The URI to check</param>
    /// <param name="name">The health check name</param>
    /// <param name="failureStatus">The failure status to report</param>
    /// <param name="tags">The health check tags</param>
    /// <param name="timeout">The timeout for the health check</param>
    /// <returns>The health check builder for chaining</returns>
    public static IHealthChecksBuilder AddUrlGroup(
        this IHealthChecksBuilder builder,
        Uri uri,
        string name,
        HealthStatus? failureStatus = null,
        IEnumerable<string>? tags = null,
        TimeSpan? timeout = null)
    {
        return builder.AddUrlGroup(
            new[] { uri },
            name,
            failureStatus,
            tags,
            timeout);
    }
    
    /// <summary>
    /// Adds URL group health checks for multiple URIs
    /// </summary>
    /// <param name="builder">The health check builder</param>
    /// <param name="uris">The URIs to check</param>
    /// <param name="name">The health check name</param>
    /// <param name="failureStatus">The failure status to report</param>
    /// <param name="tags">The health check tags</param>
    /// <param name="timeout">The timeout for the health check</param>
    /// <returns>The health check builder for chaining</returns>
    public static IHealthChecksBuilder AddUrlGroup(
        this IHealthChecksBuilder builder,
        IEnumerable<Uri> uris,
        string name,
        HealthStatus? failureStatus = null,
        IEnumerable<string>? tags = null,
        TimeSpan? timeout = null)
    {
        var options = new AspNetCore.Diagnostics.HealthChecks.UriHealthCheckOptions();
        
        foreach (var uri in uris)
        {
            options.AddUri(uri);
        }
        
        if (timeout.HasValue)
        {
            options.Timeout = timeout.Value;
        }
        
        return builder.Add(new HealthCheckRegistration(
            name,
            sp => new AspNetCore.Diagnostics.HealthChecks.UriHealthCheck(options),
            failureStatus,
            tags,
            timeout));
    }
}