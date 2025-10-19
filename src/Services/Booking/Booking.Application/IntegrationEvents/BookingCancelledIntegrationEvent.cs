using BuildingBlocks.EventBus;

namespace Booking.Application.IntegrationEvents;

/// <summary>
/// Integration event published when a booking is cancelled.
/// </summary>
public record BookingCancelledIntegrationEvent(
    Guid BookingId,
    Guid ResourceId,
    string Reason
) : IntegrationEvent;


