using BuildingBlocks.Common.Results;
using MediatR;

namespace Booking.Application.Commands;

/// <summary>
/// Command to cancel a booking.
/// </summary>
public record CancelBookingCommand(
    Guid BookingId,
    string Reason
) : IRequest<Result>;


