using BuildingBlocks.EventBus;

namespace User.Application.IntegrationEvents;

/// <summary>
/// Integration event published when a new user is registered.
/// </summary>
public record UserRegisteredIntegrationEvent(
    Guid UserId,
    string Email,
    string FullName
) : IntegrationEvent;

