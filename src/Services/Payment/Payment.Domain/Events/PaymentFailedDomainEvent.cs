using BuildingBlocks.Common.Domain;

namespace Payment.Domain.Events;

public record PaymentFailedDomainEvent(
    Guid PaymentId,
    Guid BookingId,
    string Reason
) : DomainEvent;

