using BuildingBlocks.EventBus;

namespace Payment.Application.IntegrationEvents;

public record PaymentRefundedIntegrationEvent(
    Guid PaymentId,
    Guid BookingId,
    decimal Amount,
    string Reason
) : IntegrationEvent;

