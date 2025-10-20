using Duende.IdentityServer;
using IdentityServer;
using Serilog;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Serilog configuration
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.WithProperty("ServiceName", "IdentityServer")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();

// IdentityServer configuration
var identityServerBuilder = builder.Services.AddIdentityServer(options =>
{
    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseInformationEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseSuccessEvents = true;
    
    // Emit static discovery document
    options.Discovery.ShowKeySet = true;
    
    // Configure endpoints
    options.Endpoints.EnableTokenEndpoint = true;
    options.Endpoints.EnableAuthorizeEndpoint = true;
    options.Endpoints.EnableDiscoveryEndpoint = true;
    options.Endpoints.EnableTokenRevocationEndpoint = true;
    options.Endpoints.EnableIntrospectionEndpoint = true;
})
.AddInMemoryIdentityResources(Config.IdentityResources)
.AddInMemoryApiResources(Config.ApiResources)
.AddInMemoryApiScopes(Config.ApiScopes)
.AddInMemoryClients(Config.Clients);

// Signing credential configuration
if (builder.Environment.IsDevelopment())
{
    // Development - use temporary signing credential
    identityServerBuilder.AddDeveloperSigningCredential();
    Log.Warning("Using developer signing credential - NOT for production!");
}
else
{
    // Production - load certificate from configuration
    var certificatePath = builder.Configuration["IdentityServer:Certificate:Path"];
    var certificatePassword = builder.Configuration["IdentityServer:Certificate:Password"];
    
    if (!string.IsNullOrEmpty(certificatePath) && File.Exists(certificatePath))
    {
        var certificate = new X509Certificate2(certificatePath, certificatePassword);
        identityServerBuilder.AddSigningCredential(certificate);
        Log.Information("Loaded signing certificate from {Path}", certificatePath);
    }
    else
    {
        identityServerBuilder.AddDeveloperSigningCredential();
        Log.Warning("Certificate not found, falling back to developer credential");
    }
}

// Test users for development
if (builder.Environment.IsDevelopment())
{
    // For development, we'll use a simple in-memory user store
    // Note: In production, you should use a proper user store like ASP.NET Core Identity
    Log.Information("Development mode: Using in-memory test users");
}

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Booking Platform IdentityServer", 
        Version = "v1",
        Description = "OAuth2/OpenID Connect Identity Server for microservices authentication and authorization",
        Contact = new() {
            Name = "Booking Platform Team",
            Email = "support@bookingplatform.com"
        }
    });
    
    // Add security definitions for OAuth2
    c.AddSecurityDefinition("oauth2", new()
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.OAuth2,
        Flows = new()
        {
            ClientCredentials = new()
            {
                TokenUrl = new Uri("/connect/token", UriKind.Relative),
                Scopes = new Dictionary<string, string>
                {
                    {"booking.internal", "Internal service communication"},
                    {"inventory.api", "Inventory API access"},
                    {"bookings.api", "Booking API access"},
                    {"users.api", "User API access"},
                    {"payments.api", "Payment API access"}
                }
            }
        }
    });
});

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",   // React dev server
                "http://localhost:4200",   // Angular dev server  
                "http://localhost:5000",   // API Gateway
                "https://localhost:5000",
                "http://localhost:8080"    // Potential frontend
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
    
    // Separate policy for development
    options.AddPolicy("Development", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Health checks
builder.Services.AddHealthChecks()
    .AddCheck("identity_server", () => 
    {
        // Simple health check - can be enhanced
        return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("IdentityServer is running");
    });

// Configure rate limiting for token endpoint
builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "IdentityServer v1");
        options.OAuthClientId("swagger");
        options.OAuthAppName("IdentityServer Swagger UI");
        options.OAuthUsePkce();
    });
    
    app.UseCors("Development");
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseCors("DefaultPolicy");
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    
    await next();
});

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = 
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress);
    };
});

app.UseHttpsRedirection();

// IdentityServer middleware - must be before authorization
app.UseIdentityServer();

app.UseAuthorization();
app.MapControllers();

// Health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");

// Discovery endpoint info
app.MapGet("/", () => new {
    Name = "Booking Platform IdentityServer",
    Version = "1.0.0",
    Environment = app.Environment.EnvironmentName,
    Endpoints = new {
        Discovery = "/.well-known/openid_configuration",
        Token = "/connect/token",
        Authorize = "/connect/authorize",
        Introspection = "/connect/introspect",
        Revocation = "/connect/revocation",
        Health = "/health"
    },
    Timestamp = DateTime.UtcNow
});

try
{
    Log.Information("Starting Booking Platform IdentityServer on {Environment}", app.Environment.EnvironmentName);
    Log.Information("IdentityServer will be available at: {Urls}", string.Join(", ", builder.WebHost.GetSetting("urls")?.Split(';') ?? new[] { "Not specified" }));
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "IdentityServer terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}