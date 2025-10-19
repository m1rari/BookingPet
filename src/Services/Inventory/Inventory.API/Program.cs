using BuildingBlocks.Observability;
using Inventory.Application.Commands.Handlers;
using Inventory.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog configuration
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.WithProperty("ServiceName", "Inventory.API")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Inventory API", 
        Version = "v1",
        Description = "Inventory management service for booking platform"
    });
    
    // Add JWT Bearer authorization to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateResourceCommandHandler).Assembly);
});

// Infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

// 🔐 SERVICE AUTHENTICATION - accepts tokens from IdentityServer (Gateway)
builder.Services.AddServiceAuthentication(builder.Configuration, "inventory-service");

// Observability
builder.Services.AddCustomOpenTelemetry(builder.Configuration, "inventory-service");

// Health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("InventoryDB")!)
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!)
    .AddUrlGroup(
        new Uri(builder.Configuration["IdentityServer:Authority"] ?? "https://localhost:5001"), 
        "IdentityServer");

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventory API v1");
        c.OAuthClientId("booking.spa");
        c.OAuthAppName("Inventory API");
        c.OAuthUsePkce();
    });
}

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Service-Name", "Inventory.API");
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    await next();
});

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = 
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress);
        
        var clientId = httpContext.User?.FindFirst("client_id")?.Value;
        if (!string.IsNullOrEmpty(clientId))
        {
            diagnosticContext.Set("ClientId", clientId);
        }
    };
});

app.UseHttpsRedirection();

// 🔐 Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

// 📊 API Routes
app.MapControllers()
    .RequireAuthorization("ServicePolicy"); // 🔒 All endpoints require service authentication

// 🏥 Health check endpoints (no auth required)
app.MapHealthChecks("/health").AllowAnonymous();
app.MapHealthChecks("/health/ready").AllowAnonymous();
app.MapHealthChecks("/health/live").AllowAnonymous();

// 📊 Prometheus metrics endpoint (no auth required)
app.MapPrometheusScrapingEndpoint().AllowAnonymous();

// 📊 Service info endpoint
app.MapGet("/api/v1/inventory/info", () => new {
    Service = "Inventory.API",
    Version = "1.0.0",
    Environment = app.Environment.EnvironmentName,
    Timestamp = DateTime.UtcNow,
    Authentication = "IdentityServer (Client Credentials)"
}).AllowAnonymous();

try
{
    Log.Information("Starting Inventory API on {Environment}", app.Environment.EnvironmentName);
    Log.Information("IdentityServer Authority: {Authority}", builder.Configuration["IdentityServer:Authority"]);
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Inventory API terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}