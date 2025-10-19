namespace BuildingBlocks.EventBus;

/// <summary>
/// Interface for publishing and subscribing to integration events.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes an integration event to the message broker.
    /// </summary>
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) 
        where T : IntegrationEvent;
}

