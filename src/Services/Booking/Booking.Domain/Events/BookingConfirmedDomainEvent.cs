using BuildingBlocks.Common.Domain;

namespace Booking.Domain.Events;

/// <summary>
/// Domain event raised when a booking is confirmed.
/// </summary>
public record BookingConfirmedDomainEvent(Guid BookingId) : DomainEvent;


