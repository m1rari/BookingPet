using BuildingBlocks.Common.Domain;

namespace Payment.Domain.Events;

public record PaymentRefundedDomainEvent(
    Guid PaymentId,
    Guid BookingId,
    decimal Amount,
    string Reason
) : DomainEvent;

