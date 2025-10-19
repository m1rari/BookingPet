using BuildingBlocks.Observability;
using Payment.Application.Commands.Handlers;
using Payment.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog configuration
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.WithProperty("ServiceName", "Payment.API")
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Payment API", Version = "v1" });
});

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(ProcessPaymentCommandHandler).Assembly);
});

// Infrastructure services (Database, Payment Gateway, EventBus)
builder.Services.AddInfrastructure(builder.Configuration);

// JWT Bearer Authentication for Client Credentials from IdentityServer
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration["IdentityServer:Authority"];
        options.RequireHttpsMetadata = false; // Only for development!
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false, // For Client Credentials
            ValidateIssuer = true
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "payments.api");
    });
});

// Observability
builder.Services.AddCustomOpenTelemetry(builder.Configuration, "payment-service");

// Health checks
builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("PaymentDB")!);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

// Enable authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireAuthorization("ApiScope"); // Protect all controllers

// Health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

// Prometheus metrics endpoint
app.MapPrometheusScrapingEndpoint();

try
{
    Log.Information("Starting Payment API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Payment API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
