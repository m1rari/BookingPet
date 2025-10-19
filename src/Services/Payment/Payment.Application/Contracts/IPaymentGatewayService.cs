namespace Payment.Application.Contracts;

/// <summary>
/// Interface for payment gateway service (Anticorruption Layer).
/// </summary>
public interface IPaymentGatewayService
{
    Task<PaymentGatewayResponse> ProcessPaymentAsync(
        Guid transactionId,
        decimal amount,
        string currency,
        string paymentMethod,
        CancellationToken cancellationToken = default);

    Task<RefundGatewayResponse> ProcessRefundAsync(
        string externalTransactionId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default);
}

public record PaymentGatewayResponse(
    bool Success,
    string? TransactionId,
    string Message,
    string? ErrorCode = null
);

public record RefundGatewayResponse(
    bool Success,
    string? RefundId,
    string Message
);

