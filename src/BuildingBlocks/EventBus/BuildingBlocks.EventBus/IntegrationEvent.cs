namespace BuildingBlocks.EventBus;

/// <summary>
/// Base class for integration events that are published across service boundaries.
/// </summary>
public abstract record IntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
}

