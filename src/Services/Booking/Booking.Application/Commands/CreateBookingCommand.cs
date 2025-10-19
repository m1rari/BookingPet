using BuildingBlocks.Common.Results;
using MediatR;

namespace Booking.Application.Commands;

/// <summary>
/// Command to create a new booking (initiates the Saga).
/// </summary>
public record CreateBookingCommand(
    Guid ResourceId,
    Guid UserId,
    DateTime StartTime,
    DateTime EndTime,
    decimal PricePerHour
) : IRequest<Result<Guid>>;


