using BuildingBlocks.Common.Domain;

namespace Inventory.Domain.Events;

/// <summary>
/// Domain event raised when a new resource is created.
/// </summary>
public record ResourceCreatedDomainEvent(
    Guid ResourceId,
    string Name,
    string Type
) : DomainEvent;

