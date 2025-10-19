using BuildingBlocks.Common.Results;
using MediatR;

namespace Booking.Application.Commands;

/// <summary>
/// Command to confirm a booking (called by event handlers when both resource and payment are confirmed).
/// </summary>
public record ConfirmBookingCommand(Guid BookingId) : IRequest<Result>;


