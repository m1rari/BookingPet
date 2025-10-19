using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;
using ApiGateway.Ocelot.Services;
using ApiGateway.Ocelot.Handlers;

var builder = WebApplication.CreateBuilder(args);

// ❌ УДАЛИТЕ эту строку - она вызывает дублирование
// builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Serilog configuration
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.WithProperty("ServiceName", "ApiGateway")
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"] ?? "BookingPlatform",
            ValidAudience = jwtSettings["Audience"] ?? "BookingPlatformUsers",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// HttpClient for getting tokens from IdentityServer
builder.Services.AddHttpClient();

// Register TokenService
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddTransient<ServiceTokenDelegatingHandler>();

// Redis
builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
    return StackExchange.Redis.ConnectionMultiplexer.Connect(connectionString);
});

builder.Services.AddSingleton<StackExchange.Redis.IDatabase>(sp =>
{
    var multiplexer = sp.GetRequiredService<StackExchange.Redis.IConnectionMultiplexer>();
    return multiplexer.GetDatabase();
});

// ПРАВИЛЬНЫЙ способ загрузки конфигурации Ocelot
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services
    .AddOcelot(builder.Configuration)
    .AddDelegatingHandler<ServiceTokenDelegatingHandler>(global: true); // Применяется ко всем маршрутам

// SwaggerForOcelot
builder.Services.AddSwaggerForOcelot(builder.Configuration);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Health checks
builder.Services.AddHealthChecks()
    .AddRedis(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379");

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerForOcelotUI(opt =>
    {
        opt.PathToSwaggerGenerator = "/swagger/docs";
    });
}

app.UseSerilogRequestLogging();

// Custom middleware
app.UseMiddleware<ApiGateway.Ocelot.Middleware.ErrorHandlingMiddleware>();
app.UseMiddleware<ApiGateway.Ocelot.Middleware.RequestLoggingMiddleware>();
app.UseMiddleware<ApiGateway.Ocelot.Middleware.ResponseCachingMiddleware>();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

// Ocelot middleware
await app.UseOcelot();

app.MapControllers();

try
{
    Log.Information("Starting API Gateway");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "API Gateway terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
