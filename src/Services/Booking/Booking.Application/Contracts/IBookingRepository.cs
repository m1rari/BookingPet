using BuildingBlocks.Common.Persistence;

namespace Booking.Application.Contracts;

/// <summary>
/// Repository interface for Booking aggregate.
/// </summary>
public interface IBookingRepository : IRepository<Domain.Aggregates.Booking>
{
    Task<IEnumerable<Domain.Aggregates.Booking>> GetBookingsByUserIdAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<Domain.Aggregates.Booking>> GetBookingsByResourceIdAsync(
        Guid resourceId, 
        CancellationToken cancellationToken = default);
}


