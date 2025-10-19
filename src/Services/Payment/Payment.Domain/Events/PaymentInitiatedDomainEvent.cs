using BuildingBlocks.Common.Domain;

namespace Payment.Domain.Events;

public record PaymentInitiatedDomainEvent(
    Guid PaymentId,
    Guid BookingId,
    decimal Amount
) : DomainEvent;

