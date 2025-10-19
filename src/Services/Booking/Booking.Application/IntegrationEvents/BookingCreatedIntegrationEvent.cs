using BuildingBlocks.EventBus;

namespace Booking.Application.IntegrationEvents;

/// <summary>
/// Integration event published when a booking is created.
/// </summary>
public record BookingCreatedIntegrationEvent(
    Guid BookingId,
    Guid ResourceId,
    Guid UserId,
    DateTime StartTime,
    DateTime EndTime,
    decimal TotalAmount
) : IntegrationEvent;


