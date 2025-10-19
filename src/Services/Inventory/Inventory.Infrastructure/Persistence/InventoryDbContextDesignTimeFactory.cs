using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Inventory.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for InventoryDbContext to enable EF migrations
/// </summary>
public class InventoryDbContextDesignTimeFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<InventoryDbContext>();
        
        // Use default connection string for design-time
        var connectionString = "Host=localhost;Port=5432;Database=InventoryDB;Username=postgres;Password=postgres";
        
        optionsBuilder.UseNpgsql(connectionString, options =>
        {
            options.MigrationsAssembly(typeof(InventoryDbContext).Assembly.FullName);
        });

        // Create a simple context without mediator for design-time
        return new InventoryDbContext(optionsBuilder.Options, null!);
    }
}
