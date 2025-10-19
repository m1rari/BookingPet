using BuildingBlocks.EventBus;

namespace Payment.Application.IntegrationEvents;

public record PaymentFailedIntegrationEvent(
    Guid PaymentId,
    Guid BookingId,
    string Reason
) : IntegrationEvent;

