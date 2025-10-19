using BuildingBlocks.Common.Persistence;

namespace Payment.Application.Contracts;

public interface IPaymentRepository : IRepository<Domain.Aggregates.Payment>
{
    Task<Domain.Aggregates.Payment?> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Domain.Aggregates.Payment>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

