using BuildingBlocks.Common.Results;
using BuildingBlocks.EventBus;
using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Application.Contracts;
using Payment.Application.IntegrationEvents;

namespace Payment.Application.Commands.Handlers;

/// <summary>
/// Handler for RefundPaymentCommand.
/// </summary>
public class RefundPaymentCommandHandler : IRequestHandler<RefundPaymentCommand, Result>
{
    private readonly IPaymentRepository _repository;
    private readonly IPaymentGatewayService _paymentGateway;
    private readonly IEventBus _eventBus;
    private readonly ILogger<RefundPaymentCommandHandler> _logger;

    public RefundPaymentCommandHandler(
        IPaymentRepository repository,
        IPaymentGatewayService paymentGateway,
        IEventBus eventBus,
        ILogger<RefundPaymentCommandHandler> logger)
    {
        _repository = repository;
        _paymentGateway = paymentGateway;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<Result> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _repository.GetByIdAsync(request.PaymentId, cancellationToken);

        if (payment == null)
        {
            return Result.Failure(new Error(
                "Payment.NotFound",
                $"Payment with ID {request.PaymentId} not found"));
        }

        // Check if payment can be refunded
        var refundResult = payment.Refund(request.Reason);
        if (refundResult.IsFailure)
        {
            return refundResult;
        }

        try
        {
            // Process refund through gateway (with Circuit Breaker!)
            var gatewayResponse = await _paymentGateway.ProcessRefundAsync(
                payment.ExternalTransactionId!,
                payment.Amount.Amount,
                payment.Amount.Currency,
                cancellationToken);

            if (gatewayResponse.Success)
            {
                // Save refund
                await _repository.UnitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Payment {PaymentId} refunded successfully", payment.Id);

                // Publish refund event
                await _eventBus.PublishAsync(new PaymentRefundedIntegrationEvent(
                    payment.Id,
                    payment.BookingId,
                    payment.Amount.Amount,
                    request.Reason
                ), cancellationToken);

                return Result.Success();
            }
            else
            {
                _logger.LogWarning("Refund failed for payment {PaymentId}: {Reason}",
                    payment.Id, gatewayResponse.Message);

                return Result.Failure(new Error(
                    "Payment.RefundFailed",
                    gatewayResponse.Message));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding payment {PaymentId}", payment.Id);

            return Result.Failure(new Error(
                "Payment.RefundError",
                $"Refund error: {ex.Message}"));
        }
    }
}

