using BuildingBlocks.Common.Results;
using MediatR;

namespace Payment.Application.Commands;

/// <summary>
/// Command to process a payment through external gateway.
/// </summary>
public record ProcessPaymentCommand(
    Guid BookingId,
    Guid UserId,
    decimal Amount,
    string Currency,
    string PaymentMethod = "CreditCard"
) : IRequest<Result<Guid>>;

