using Duende.IdentityServer.Models;
using Duende.IdentityServer;

namespace IdentityServer;

/// <summary>
/// Enhanced configuration for IdentityServer with Gateway + Client Credentials architecture
/// </summary>
public static class Config
{
    /// <summary>
    /// Identity resources for user authentication
    /// </summary>
    public static IEnumerable<IdentityResource> IdentityResources =>
        new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
            new IdentityResource
            {
                Name = "role",
                UserClaims = new List<string> {"role"}
            }
        };

    /// <summary>
    /// API Resources representing our microservices
    /// </summary>
    public static IEnumerable<ApiResource> ApiResources =>
        new List<ApiResource>
        {
            new ApiResource("booking.api", "Booking Platform API")
            {
                Scopes = { "booking.full_access", "booking.read", "booking.write" },
                UserClaims = { "role", "name", "email" }
            },
            new ApiResource("booking.internal", "Internal Service Communication")
            {
                Scopes = { "booking.internal" }
            }
        };

    /// <summary>
    /// API Scopes for granular access control
    /// </summary>
    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            // Public API scopes (for users through Gateway)
            new ApiScope("booking.full_access", "Full access to Booking API"),
            new ApiScope("booking.read", "Read access to Booking API"),
            new ApiScope("booking.write", "Write access to Booking API"),
            
            // Internal service communication
            new ApiScope("booking.internal", "Internal service communication"),
            
            // Individual service scopes for fine-grained access
            new ApiScope("inventory.api", "Inventory API access"),
            new ApiScope("bookings.api", "Booking API access"),
            new ApiScope("users.api", "User API access"),
            new ApiScope("payments.api", "Payment API access"),
            new ApiScope("reviews.api", "Review API access"),
            new ApiScope("analytics.api", "Analytics API access")
        };

    /// <summary>
    /// Clients configuration for different authentication flows
    /// </summary>
    public static IEnumerable<Client> Clients =>
        new List<Client>
        {
            // ==== USER AUTHENTICATION CLIENTS ====
            
            // Web Application (SPA) - for user login
            new Client
            {
                ClientId = "booking.spa",
                ClientName = "Booking Platform SPA",
                AllowedGrantTypes = GrantTypes.Code,
                RequireClientSecret = false,
                RequirePkce = true,
                
                RedirectUris = { "http://localhost:3000/callback", "http://localhost:4200/callback" },
                PostLogoutRedirectUris = { "http://localhost:3000", "http://localhost:4200" },
                AllowedCorsOrigins = { "http://localhost:3000", "http://localhost:4200" },
                
                AllowedScopes = {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    "role",
                    "booking.full_access"
                },
                
                AccessTokenLifetime = 900, // 15 minutes
                RefreshTokenUsage = TokenUsage.ReUse,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                SlidingRefreshTokenLifetime = 86400 // 24 hours
            },
            
            // API Gateway - validates user tokens and gets service tokens
            new Client
            {
                ClientId = "booking.gateway",
                ClientName = "API Gateway",
                ClientSecrets = { new Secret("gateway_super_secret_key_2024".Sha256()) },
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                
                AllowedScopes = {
                    "booking.internal",
                    "inventory.api",
                    "bookings.api",
                    "users.api",
                    "payments.api",
                    "reviews.api",
                    "analytics.api"
                },
                
                AccessTokenLifetime = 3600, // 1 hour
                ClientClaimsPrefix = string.Empty,
                Claims = {
                    new ClientClaim("service_type", "gateway"),
                    new ClientClaim("trusted", "true")
                }
            },
            
            // ==== SERVICE-TO-SERVICE CLIENTS ====
            
            // Booking Service - can call inventory, users, payments
            new Client
            {
                ClientId = "booking.service",
                ClientName = "Booking Service",
                ClientSecrets = { new Secret("booking_service_secret_2024".Sha256()) },
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                
                AllowedScopes = {
                    "inventory.api",
                    "users.api",
                    "payments.api",
                    "booking.internal"
                },
                
                AccessTokenLifetime = 3600,
                ClientClaimsPrefix = string.Empty,
                Claims = {
                    new ClientClaim("service_type", "booking"),
                    new ClientClaim("service_name", "booking-service")
                }
            },
            
            // Payment Service - can call bookings, users
            new Client
            {
                ClientId = "payment.service",
                ClientName = "Payment Service",
                ClientSecrets = { new Secret("payment_service_secret_2024".Sha256()) },
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                
                AllowedScopes = {
                    "bookings.api",
                    "users.api",
                    "booking.internal"
                },
                
                AccessTokenLifetime = 3600,
                ClientClaimsPrefix = string.Empty,
                Claims = {
                    new ClientClaim("service_type", "payment"),
                    new ClientClaim("service_name", "payment-service")
                }
            },
            
            // Inventory Service - internal only
            new Client
            {
                ClientId = "inventory.service",
                ClientName = "Inventory Service",
                ClientSecrets = { new Secret("inventory_service_secret_2024".Sha256()) },
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                
                AllowedScopes = {
                    "users.api",
                    "booking.internal"
                },
                
                AccessTokenLifetime = 3600,
                ClientClaimsPrefix = string.Empty,
                Claims = {
                    new ClientClaim("service_type", "inventory"),
                    new ClientClaim("service_name", "inventory-service")
                }
            },
            
            // User Service - internal only
            new Client
            {
                ClientId = "user.service",
                ClientName = "User Service",
                ClientSecrets = { new Secret("user_service_secret_2024".Sha256()) },
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                
                AllowedScopes = {
                    "booking.internal"
                },
                
                AccessTokenLifetime = 3600,
                ClientClaimsPrefix = string.Empty,
                Claims = {
                    new ClientClaim("service_type", "user"),
                    new ClientClaim("service_name", "user-service")
                }
            },
            
            // Review Service - can call bookings, users
            new Client
            {
                ClientId = "review.service",
                ClientName = "Review Service",
                ClientSecrets = { new Secret("review_service_secret_2024".Sha256()) },
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                
                AllowedScopes = {
                    "bookings.api",
                    "users.api",
                    "booking.internal"
                },
                
                AccessTokenLifetime = 3600,
                ClientClaimsPrefix = string.Empty,
                Claims = {
                    new ClientClaim("service_type", "review"),
                    new ClientClaim("service_name", "review-service")
                }
            },
            
            // Analytics Service - read access to all services
            new Client
            {
                ClientId = "analytics.service",
                ClientName = "Analytics Service",
                ClientSecrets = { new Secret("analytics_service_secret_2024".Sha256()) },
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                
                AllowedScopes = {
                    "inventory.api",
                    "bookings.api",
                    "users.api",
                    "payments.api",
                    "reviews.api",
                    "booking.internal"
                },
                
                AccessTokenLifetime = 3600,
                ClientClaimsPrefix = string.Empty,
                Claims = {
                    new ClientClaim("service_type", "analytics"),
                    new ClientClaim("service_name", "analytics-service"),
                    new ClientClaim("access_level", "read_only")
                }
            }
        };
}