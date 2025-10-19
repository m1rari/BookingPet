using BuildingBlocks.Common.Domain;

namespace Booking.Domain.Events;

/// <summary>
/// Domain event raised when a booking is cancelled.
/// </summary>
public record BookingCancelledDomainEvent(
    Guid BookingId,
    string Reason
) : DomainEvent;


