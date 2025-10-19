using BuildingBlocks.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Payment.Application.Contracts;
using Payment.Domain.Enums;

namespace Payment.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Payment aggregate.
/// </summary>
public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext _context;

    public IUnitOfWork UnitOfWork => _context;

    public PaymentRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public async Task<Domain.Aggregates.Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Domain.Aggregates.Payment?> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.BookingId == bookingId, cancellationToken);
    }

    public async Task<IEnumerable<Domain.Aggregates.Payment>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Domain.Aggregates.Payment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Where(p => p.Status != PaymentStatus.Failed)
            .ToListAsync(cancellationToken);
    }

    public async Task<Domain.Aggregates.Payment> AddAsync(Domain.Aggregates.Payment entity, CancellationToken cancellationToken = default)
    {
        await _context.Payments.AddAsync(entity, cancellationToken);
        return entity;
    }

    public void Update(Domain.Aggregates.Payment entity)
    {
        _context.Payments.Update(entity);
    }

    public void Remove(Domain.Aggregates.Payment entity)
    {
        _context.Payments.Remove(entity);
    }
}

