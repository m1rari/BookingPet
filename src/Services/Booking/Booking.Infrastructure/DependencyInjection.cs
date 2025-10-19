using Booking.Application.Contracts;
using Booking.Application.Sagas;
using Booking.Infrastructure.Persistence;
using Booking.Infrastructure.Persistence.Repositories;
using BuildingBlocks.EventBus.RabbitMQ;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Booking.Infrastructure;

/// <summary>
/// Dependency injection extensions for Infrastructure layer.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<BookingDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("BookingDB"),
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(BookingDbContext).Assembly.FullName);
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null);
                }));

        // Repositories
        services.AddScoped<IBookingRepository, BookingRepository>();

        // Sagas
        services.AddScoped<CreateBookingSaga>();

        // Event Bus
        services.AddMassTransitRabbitMq(configuration);

        return services;
    }
}


