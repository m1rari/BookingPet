using BuildingBlocks.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Booking.Infrastructure.Persistence;

/// <summary>
/// Database context for Booking Service with optimistic concurrency support.
/// </summary>
public class BookingDbContext : DbContext, IUnitOfWork
{
    private readonly IMediator _mediator;

    public DbSet<Domain.Aggregates.Booking> Bookings => Set<Domain.Aggregates.Booking>();

    public BookingDbContext(DbContextOptions<BookingDbContext> options, IMediator mediator)
        : base(options)
    {
        _mediator = mediator;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingDbContext).Assembly);
    }

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        // Dispatch domain events before saving
        await DispatchDomainEventsAsync(cancellationToken);

        try
        {
            await base.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            // Optimistic concurrency conflict occurred
            // This will be handled by the application layer
            throw;
        }
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        if (_mediator == null) return; // Skip domain events during design-time
        
        var domainEntities = ChangeTracker
            .Entries<Domain.Aggregates.Booking>()
            .Where(x => x.Entity.DomainEvents.Any())
            .Select(x => x.Entity)
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(x => x.DomainEvents)
            .ToList();

        domainEntities.ForEach(entity => entity.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }
    }
}


