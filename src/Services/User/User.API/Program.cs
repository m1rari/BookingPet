using BuildingBlocks.Authentication;
using BuildingBlocks.Observability;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using User.Application.Commands.Handlers;
using User.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Serilog configuration
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.WithProperty("ServiceName", "User.API")
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "User API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new()
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommandHandler).Assembly);
});

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(RegisterUserCommandHandler).Assembly);

// Infrastructure services (Identity, Database, EventBus)
builder.Services.AddInfrastructure(builder.Configuration);

// JWT Authentication services for token generation
builder.Services.AddJwtTokenGeneration(builder.Configuration);

// Dual Authentication: User JWT + Client Credentials
builder.Services.AddAuthentication()
    // User JWT Authentication (for login endpoint)
    .AddJwtBearer("UserJWT", options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"] ?? "BookingPlatform",
            ValidAudience = jwtSettings["Audience"] ?? "BookingPlatformUsers",
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };
    })
    // Client Credentials Authentication (for service-to-service calls)
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
    // Policy for Client Credentials (service-to-service)
    options.AddPolicy("ApiScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "users.api");
    });
    
    // Policy for User JWT (user authentication)
    options.AddPolicy("UserAuth", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddAuthenticationSchemes("UserJWT");
    });
});

// Observability
builder.Services.AddCustomOpenTelemetry(builder.Configuration, "user-service");

// Health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("UserDB")!);

// CORS (optional)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

// Prometheus metrics endpoint
app.MapPrometheusScrapingEndpoint();

try
{
    Log.Information("Starting User API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "User API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
