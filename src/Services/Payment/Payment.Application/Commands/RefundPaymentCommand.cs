using BuildingBlocks.Common.Results;
using MediatR;

namespace Payment.Application.Commands;

/// <summary>
/// Command to refund a payment.
/// </summary>
public record RefundPaymentCommand(
    Guid PaymentId,
    string Reason
) : IRequest<Result>;

