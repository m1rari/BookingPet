using BuildingBlocks.EventBus;
using MassTransit;

namespace BuildingBlocks.EventBus.RabbitMQ;

/// <summary>
/// MassTransit implementation of IEventBus for RabbitMQ.
/// </summary>
public class MassTransitEventBus : IEventBus
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitEventBus(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
    }

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) 
        where T : IntegrationEvent
    {
        await _publishEndpoint.Publish(@event, cancellationToken);
    }
}

