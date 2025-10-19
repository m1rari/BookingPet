using BuildingBlocks.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Payment.Infrastructure.Persistence;

/// <summary>
/// Database context for Payment Service (SQL Server).
/// </summary>
public class PaymentDbContext : DbContext, IUnitOfWork
{
    public DbSet<Domain.Aggregates.Payment> Payments => Set<Domain.Aggregates.Payment>();

    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentDbContext).Assembly);
    }

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        await base.SaveChangesAsync(cancellationToken);
        return true;
    }
}

