using BuildingBlocks.Common.Domain;

namespace Booking.Domain.Events;

/// <summary>
/// Domain event raised when a booking is created.
/// </summary>
public record BookingCreatedDomainEvent(
    Guid BookingId,
    Guid ResourceId,
    Guid UserId,
    DateTime StartTime,
    DateTime EndTime
) : DomainEvent;


