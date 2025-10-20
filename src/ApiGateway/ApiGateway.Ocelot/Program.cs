using ApiGateway.Ocelot.Extensions;
using ApiGateway.Ocelot.Handlers;
using ApiGateway.Ocelot.Middleware;
using ApiGateway.Ocelot.Services;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Serilog configuration
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.WithProperty("ServiceName", "ApiGateway")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 🔐 GATEWAY AUTHENTICATION - validates user tokens from IdentityServer
// builder.Services.AddGatewayAuthentication(builder.Configuration);

// 🔧 HttpClient and Token Service for service-to-service calls
builder.Services.AddHttpClient<TokenService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Memory cache for token caching
builder.Services.AddMemoryCache();

// Register TokenService for getting service tokens
builder.Services.AddSingleton<ITokenService, TokenService>();
// Service Token Handler for Ocelot downstream calls
builder.Services.AddTransient<ServiceTokenDelegatingHandler>();

// 📊 Redis for distributed caching and rate limiting
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
    options.InstanceName = "BookingPlatform.Gateway";
});

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
    return ConnectionMultiplexer.Connect(connectionString);
});

// Register IDatabase for direct Redis access
builder.Services.AddSingleton<IDatabase>(sp =>
{
    var connectionMultiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
    return connectionMultiplexer.GetDatabase();
});


// 🔧 Ocelot Configuration
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services
    .AddOcelot(builder.Configuration)
    .AddDelegatingHandler<ServiceTokenDelegatingHandler>(global: true);

// 📚 Swagger for Ocelot
builder.Services.AddSwaggerForOcelot(builder.Configuration);

// 🌐 CORS
builder.Services.AddCors(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.AddPolicy("Development", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    }
    
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",   // React dev
                "http://localhost:4200",   // Angular dev
                "https://bookingplatform.com",
                "https://www.bookingplatform.com"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// 🏥 Health Checks
builder.Services.AddHealthChecks()
    .AddRedis(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379")
    .AddUrlGroup(new Uri(builder.Configuration["IdentityServer:Authority"] ?? "https://localhost:5005"), "IdentityServer")
    .AddUrlGroup(new Uri("http://localhost:5001/health"), "Inventory.API")
    .AddUrlGroup(new Uri("http://localhost:5002/health"), "Booking.API")
    .AddUrlGroup(new Uri("http://localhost:5003/health"), "User.API")
    .AddUrlGroup(new Uri("http://localhost:5004/health"), "Payment.API");

var app = builder.Build();

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwaggerForOcelotUI(opt =>
    {
        opt.PathToSwaggerGenerator = "/swagger/docs";
        opt.OAuthClientId("booking.spa");
        opt.OAuthAppName("Booking Platform API Gateway");
        opt.OAuthUsePkce();
    });
    app.UseCors("Development");
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseCors("Production");
}

// 🔒 Security Headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Add("X-Powered-By", "BookingPlatform Gateway");
    
    await next();
});

// 📝 Logging
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = 
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].FirstOrDefault());
        
        var userId = httpContext.User?.Identity?.Name;
        if (!string.IsNullOrEmpty(userId))
        {
            diagnosticContext.Set("UserId", userId);
        }
    };
});


// 🌐 Custom Middleware
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ResponseCachingMiddleware>();
app.UseServiceTokenMiddleware(); // 🔑 Adds service tokens to downstream calls

app.UseHttpsRedirection();

// 🔐 Authentication & Authorization
// app.UseAuthentication();
// app.UseAuthorization();

// 🏥 Health Check Endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");

// 📊 Gateway Statistics Endpoint
app.MapGet("/api/v1/gateway/stats", async (HttpContext context) =>
{
    return new {
        Gateway = "Booking Platform API Gateway",
        Version = "1.0.0",
        Environment = app.Environment.EnvironmentName,
        Timestamp = DateTime.UtcNow,
        IsAuthenticated = context.User?.Identity?.IsAuthenticated ?? false,
        User = context.User?.Identity?.Name,
        Services = new {
            IdentityServer = builder.Configuration["IdentityServer:Authority"],
            InventoryApi = "http://localhost:5001",
            BookingApi = "http://localhost:5002",
            UserApi = "http://localhost:5003",
            PaymentApi = "http://localhost:5004"
        }
    };
}); // .RequireAuthorization("UserPolicy");

// 🔄 Token Refresh Endpoint
app.MapPost("/api/v1/gateway/refresh-service-tokens", async (ITokenService tokenService) =>
{
    tokenService.ClearTokenCache();
    return Results.Ok(new { Message = "Service token cache cleared", Timestamp = DateTime.UtcNow });
}); // .RequireAuthorization("AdminPolicy");

// 🎯 Ocelot Middleware - MUST be last!
await app.UseOcelot();

app.MapControllers();

try
{
    Log.Information("Starting Booking Platform API Gateway on {Environment}", app.Environment.EnvironmentName);
    Log.Information("Gateway will be available at: {Urls}", string.Join(", ", builder.WebHost.GetSetting("urls")?.Split(';') ?? new[] { "Not specified" }));
    Log.Information("IdentityServer Authority: {Authority}", builder.Configuration["IdentityServer:Authority"]);
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "API Gateway terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}