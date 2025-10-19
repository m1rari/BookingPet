using BuildingBlocks.Common.Domain;

namespace Payment.Domain.Events;

public record PaymentCompletedDomainEvent(
    Guid PaymentId,
    Guid BookingId,
    string ExternalTransactionId
) : DomainEvent;

