using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Booking.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for BookingDbContext to enable EF migrations
/// </summary>
public class BookingDbContextDesignTimeFactory : IDesignTimeDbContextFactory<BookingDbContext>
{
    public BookingDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BookingDbContext>();
        
        // Use default connection string for design-time
        var connectionString = "Host=localhost;Port=5433;Database=BookingDB;Username=postgres;Password=postgres";
        
        optionsBuilder.UseNpgsql(connectionString, options =>
        {
            options.MigrationsAssembly(typeof(BookingDbContext).Assembly.FullName);
        });

        // Create a simple context without mediator for design-time
        return new BookingDbContext(optionsBuilder.Options, null!);
    }
}
