using BuildingBlocks.Common.Domain;

namespace Inventory.Domain.Events;

/// <summary>
/// Domain event raised when a resource slot is reserved.
/// </summary>
public record ResourceReservedDomainEvent(
    Guid ResourceId,
    DateTime StartTime,
    DateTime EndTime,
    Guid ReservationId
) : DomainEvent;

