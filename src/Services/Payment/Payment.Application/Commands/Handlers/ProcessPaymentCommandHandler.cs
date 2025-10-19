using BuildingBlocks.Common.Results;
using BuildingBlocks.EventBus;
using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Application.Contracts;
using Payment.Application.IntegrationEvents;
using Payment.Domain.ValueObjects;

namespace Payment.Application.Commands.Handlers;

/// <summary>
/// Handler for ProcessPaymentCommand with Circuit Breaker resilience.
/// </summary>
public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, Result<Guid>>
{
    private readonly IPaymentRepository _repository;
    private readonly IPaymentGatewayService _paymentGateway;
    private readonly IEventBus _eventBus;
    private readonly ILogger<ProcessPaymentCommandHandler> _logger;

    public ProcessPaymentCommandHandler(
        IPaymentRepository repository,
        IPaymentGatewayService paymentGateway,
        IEventBus eventBus,
        ILogger<ProcessPaymentCommandHandler> logger)
    {
        _repository = repository;
        _paymentGateway = paymentGateway;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing payment for booking {BookingId}, amount {Amount} {Currency}",
            request.BookingId, request.Amount, request.Currency);

        // Create payment aggregate
        var money = Money.Create(request.Amount, request.Currency);
        var payment = Domain.Aggregates.Payment.Create(
            request.BookingId,
            request.UserId,
            money,
            request.PaymentMethod);

        // Save payment in Pending state
        await _repository.AddAsync(payment, cancellationToken);
        await _repository.UnitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            // Call external payment gateway (with Circuit Breaker!)
            var gatewayResponse = await _paymentGateway.ProcessPaymentAsync(
                payment.Id,
                request.Amount,
                request.Currency,
                request.PaymentMethod,
                cancellationToken);

            if (gatewayResponse.Success && !string.IsNullOrEmpty(gatewayResponse.TransactionId))
            {
                // Complete payment
                payment.Complete(gatewayResponse.TransactionId);
                await _repository.UnitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Payment {PaymentId} completed successfully, external transaction {TransactionId}",
                    payment.Id, gatewayResponse.TransactionId);

                // Publish success event
                await _eventBus.PublishAsync(new PaymentCompletedIntegrationEvent(
                    payment.Id,
                    payment.BookingId,
                    gatewayResponse.TransactionId
                ), cancellationToken);

                return Result.Success(payment.Id);
            }
            else
            {
                // Mark as failed
                payment.Fail(gatewayResponse.Message);
                await _repository.UnitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogWarning("Payment {PaymentId} failed: {Reason}", payment.Id, gatewayResponse.Message);

                // Publish failure event
                await _eventBus.PublishAsync(new PaymentFailedIntegrationEvent(
                    payment.Id,
                    payment.BookingId,
                    gatewayResponse.Message
                ), cancellationToken);

                return Result.Failure<Guid>(new Error(
                    "Payment.ProcessingFailed",
                    gatewayResponse.Message));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment {PaymentId}", payment.Id);

            // Mark as failed
            payment.Fail(ex.Message);
            await _repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            // Publish failure event
            await _eventBus.PublishAsync(new PaymentFailedIntegrationEvent(
                payment.Id,
                payment.BookingId,
                ex.Message
            ), cancellationToken);

            return Result.Failure<Guid>(new Error(
                "Payment.GatewayError",
                $"Payment gateway error: {ex.Message}"));
        }
    }
}

