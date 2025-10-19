using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace User.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for UserDbContext to enable EF migrations
/// </summary>
public class UserDbContextDesignTimeFactory : IDesignTimeDbContextFactory<UserDbContext>
{
    public UserDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
        
        // Use default connection string for design-time
        var connectionString = "Host=localhost;Port=5434;Database=UserDB;Username=postgres;Password=postgres";
        
        optionsBuilder.UseNpgsql(connectionString, options =>
        {
            options.MigrationsAssembly(typeof(UserDbContext).Assembly.FullName);
        });

        return new UserDbContext(optionsBuilder.Options);
    }
}
