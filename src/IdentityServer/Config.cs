using Duende.IdentityServer.Models;

namespace IdentityServer;

/// <summary>
/// Configuration for IdentityServer API scopes and clients
/// </summary>
public static class Config
{
    /// <summary>
    /// API Scopes for microservices
    /// </summary>
    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            new ApiScope("inventory.api", "Inventory API"),
            new ApiScope("bookings.api", "Booking API"),
            new ApiScope("users.api", "User API"),
            new ApiScope("payments.api", "Payment API"),
            new ApiScope("reviews.api", "Review API"),
            new ApiScope("analytics.api", "Analytics API")
        };

    /// <summary>
    /// Clients for Client Credentials flow
    /// </summary>
    public static IEnumerable<Client> Clients =>
        new List<Client>
        {
            // API Gateway client - can call all services
            new Client
            {
                ClientId = "api-gateway",
                ClientSecrets = { new Secret("gateway-secret-key".Sha256()) },
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = { 
                    "inventory.api", 
                    "bookings.api", 
                    "users.api", 
                    "payments.api",
                    "reviews.api",
                    "analytics.api"
                },
                AccessTokenLifetime = 3600, // 1 hour
                ClientClaimsPrefix = string.Empty
            },
            
            // Booking service client - can call inventory and users
            new Client
            {
                ClientId = "booking-service",
                ClientSecrets = { new Secret("booking-secret".Sha256()) },
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = { "inventory.api", "users.api" },
                AccessTokenLifetime = 3600,
                ClientClaimsPrefix = string.Empty
            },
            
            // Payment service client - can call bookings and users
            new Client
            {
                ClientId = "payment-service",
                ClientSecrets = { new Secret("payment-secret".Sha256()) },
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = { "bookings.api", "users.api" },
                AccessTokenLifetime = 3600,
                ClientClaimsPrefix = string.Empty
            },
            
            // Review service client - can call bookings and users
            new Client
            {
                ClientId = "review-service",
                ClientSecrets = { new Secret("review-secret".Sha256()) },
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = { "bookings.api", "users.api" },
                AccessTokenLifetime = 3600,
                ClientClaimsPrefix = string.Empty
            },
            
            // Analytics service client - can call all services for reporting
            new Client
            {
                ClientId = "analytics-service",
                ClientSecrets = { new Secret("analytics-secret".Sha256()) },
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = { 
                    "inventory.api", 
                    "bookings.api", 
                    "users.api", 
                    "payments.api",
                    "reviews.api"
                },
                AccessTokenLifetime = 3600,
                ClientClaimsPrefix = string.Empty
            }
        };
}
