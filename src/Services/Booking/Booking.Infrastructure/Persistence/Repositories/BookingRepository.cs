using BuildingBlocks.Common.Persistence;
using Booking.Application.Contracts;
using Booking.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Booking.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Booking aggregate.
/// </summary>
public class BookingRepository : IBookingRepository
{
    private readonly BookingDbContext _context;

    public IUnitOfWork UnitOfWork => _context;

    public BookingRepository(BookingDbContext context)
    {
        _context = context;
    }

    public async Task<Domain.Aggregates.Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Domain.Aggregates.Booking>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Bookings
            .Where(b => b.Status != BookingStatus.Failed)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Domain.Aggregates.Booking>> GetBookingsByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Bookings
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Domain.Aggregates.Booking>> GetBookingsByResourceIdAsync(
        Guid resourceId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Bookings
            .Where(b => b.ResourceId == resourceId && 
                       (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending))
            .OrderBy(b => b.Period.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<Domain.Aggregates.Booking> AddAsync(Domain.Aggregates.Booking entity, CancellationToken cancellationToken = default)
    {
        await _context.Bookings.AddAsync(entity, cancellationToken);
        return entity;
    }

    public void Update(Domain.Aggregates.Booking entity)
    {
        _context.Bookings.Update(entity);
    }

    public void Remove(Domain.Aggregates.Booking entity)
    {
        _context.Bookings.Remove(entity);
    }
}


