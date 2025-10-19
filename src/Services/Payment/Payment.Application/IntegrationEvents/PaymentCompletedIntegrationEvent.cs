using BuildingBlocks.EventBus;

namespace Payment.Application.IntegrationEvents;

public record PaymentCompletedIntegrationEvent(
    Guid PaymentId,
    Guid BookingId,
    string ExternalTransactionId
) : IntegrationEvent;

