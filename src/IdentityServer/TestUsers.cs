using Duende.IdentityServer.Test;
using System.Security.Claims;
using System.Text.Json;
using IdentityModel;

namespace IdentityServer;

/// <summary>
/// Test users for development and testing purposes
/// </summary>
public class TestUsers
{
    public static List<TestUser> Users
    {
        get
        {
            var address = new
            {
                street_address = "One Hacker Way",
                locality = "Heidelberg",
                postal_code = 69118,
                country = "Germany"
            };

            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "818727",
                    Username = "alice",
                    Password = "alice",
                    Claims =
                    {
                        new Claim(JwtClaimTypes.Name, "Alice Smith"),
                        new Claim(JwtClaimTypes.GivenName, "Alice"),
                        new Claim(JwtClaimTypes.FamilyName, "Smith"),
                        new Claim(JwtClaimTypes.Email, "alice@bookingplatform.com"),
                        new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                        new Claim(JwtClaimTypes.Role, "Administrator"),
                        new Claim(JwtClaimTypes.WebSite, "http://alice.com"),
                        new Claim(JwtClaimTypes.Address, JsonSerializer.Serialize(address),
                            IdentityServerConstants.ClaimValueTypes.Json)
                    }
                },
                new TestUser
                {
                    SubjectId = "88421113",
                    Username = "bob",
                    Password = "bob",
                    Claims =
                    {
                        new Claim(JwtClaimTypes.Name, "Bob Smith"),
                        new Claim(JwtClaimTypes.GivenName, "Bob"),
                        new Claim(JwtClaimTypes.FamilyName, "Smith"),
                        new Claim(JwtClaimTypes.Email, "bob@bookingplatform.com"),
                        new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                        new Claim(JwtClaimTypes.Role, "Manager"),
                        new Claim(JwtClaimTypes.WebSite, "http://bob.com"),
                        new Claim(JwtClaimTypes.Address, JsonSerializer.Serialize(address),
                            IdentityServerConstants.ClaimValueTypes.Json)
                    }
                },
                new TestUser
                {
                    SubjectId = "88421114",
                    Username = "charlie",
                    Password = "charlie",
                    Claims =
                    {
                        new Claim(JwtClaimTypes.Name, "Charlie Johnson"),
                        new Claim(JwtClaimTypes.GivenName, "Charlie"),
                        new Claim(JwtClaimTypes.FamilyName, "Johnson"),
                        new Claim(JwtClaimTypes.Email, "charlie@bookingplatform.com"),
                        new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                        new Claim(JwtClaimTypes.Role, "Customer"),
                        new Claim(JwtClaimTypes.WebSite, "http://charlie.com"),
                        new Claim(JwtClaimTypes.Address, JsonSerializer.Serialize(address),
                            IdentityServerConstants.ClaimValueTypes.Json)
                    }
                },
                new TestUser
                {
                    SubjectId = "88421115",
                    Username = "service_test",
                    Password = "service123",
                    Claims =
                    {
                        new Claim(JwtClaimTypes.Name, "Service Test Account"),
                        new Claim(JwtClaimTypes.GivenName, "Service"),
                        new Claim(JwtClaimTypes.FamilyName, "Account"),
                        new Claim(JwtClaimTypes.Email, "service@bookingplatform.com"),
                        new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                        new Claim(JwtClaimTypes.Role, "Service"),
                        new Claim("service_type", "test_service"),
                        new Claim(JwtClaimTypes.Address, JsonSerializer.Serialize(address),
                            IdentityServerConstants.ClaimValueTypes.Json)
                    }
                },
                new TestUser
                {
                    SubjectId = "demo_user_001",
                    Username = "demo",
                    Password = "demo",
                    Claims =
                    {
                        new Claim(JwtClaimTypes.Name, "Demo User"),
                        new Claim(JwtClaimTypes.GivenName, "Demo"),
                        new Claim(JwtClaimTypes.FamilyName, "User"),
                        new Claim(JwtClaimTypes.Email, "demo@example.com"),
                        new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                        new Claim(JwtClaimTypes.Role, "Customer"),
                        new Claim(JwtClaimTypes.WebSite, "http://demo.example.com"),
                        new Claim(JwtClaimTypes.Address, JsonSerializer.Serialize(new {
                            street_address = "Demo Street 123",
                            locality = "Demo City", 
                            postal_code = 12345,
                            country = "Demo Country"
                        }), IdentityServerConstants.ClaimValueTypes.Json)
                    }
                }
            };
        }
    }
}