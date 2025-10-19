using BuildingBlocks.EventBus;

namespace Booking.Application.IntegrationEvents;

/// <summary>
/// Integration event published when a booking is confirmed.
/// </summary>
public record BookingConfirmedIntegrationEvent(
    Guid BookingId,
    Guid ResourceId,
    Guid UserId
) : IntegrationEvent;


