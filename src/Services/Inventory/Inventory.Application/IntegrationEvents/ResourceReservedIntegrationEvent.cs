using BuildingBlocks.EventBus;

namespace Inventory.Application.IntegrationEvents;

/// <summary>
/// Integration event published when a resource is reserved.
/// </summary>
public record ResourceReservedIntegrationEvent(
    Guid ResourceId,
    DateTime StartTime,
    DateTime EndTime,
    Guid ReservationId
) : IntegrationEvent;

