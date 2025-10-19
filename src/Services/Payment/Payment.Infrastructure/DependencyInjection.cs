using BuildingBlocks.EventBus.RabbitMQ;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application.Contracts;
using Payment.Infrastructure.ExternalServices;
using Payment.Infrastructure.Persistence;
using Payment.Infrastructure.Persistence.Repositories;

namespace Payment.Infrastructure;

/// <summary>
/// Dependency injection extensions for Infrastructure layer.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database - SQL Server
        services.AddDbContext<PaymentDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("PaymentDB"),
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(PaymentDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null);
                }));

        // Repositories
        services.AddScoped<IPaymentRepository, PaymentRepository>();

        // Payment Gateway with Circuit Breaker
        services.AddHttpClient<IPaymentGatewayService, PaymentGatewayClient>(client =>
        {
            var gatewayUrl = configuration["PaymentGateway:Url"] ?? "https://api.payment-gateway.com";
            client.BaseAddress = new Uri(gatewayUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Alternative: Register with mock mode for demo
        services.AddScoped<IPaymentGatewayService>(sp =>
        {
            var httpClient = sp.GetRequiredService<HttpClient>();
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<PaymentGatewayClient>>();
            var mockMode = configuration.GetValue<bool>("PaymentGateway:MockMode", true);
            return new PaymentGatewayClient(httpClient, logger, mockMode);
        });

        // Event Bus
        services.AddMassTransitRabbitMq(configuration);

        return services;
    }
}

