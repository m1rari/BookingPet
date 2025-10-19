using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Payment.Application.Contracts;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace Payment.Infrastructure.ExternalServices;

/// <summary>
/// Payment Gateway client with Circuit Breaker, Retry and Timeout policies (Anticorruption Layer).
/// Demonstrates Polly resilience patterns and isolates external gateway from domain.
/// </summary>
public class PaymentGatewayClient : IPaymentGatewayService
{
    private readonly HttpClient _httpClient;
    private readonly ResiliencePipeline _resiliencePipeline;
    private readonly ILogger<PaymentGatewayClient> _logger;
    private readonly bool _mockMode; // For demo without real gateway

    public PaymentGatewayClient(HttpClient httpClient, ILogger<PaymentGatewayClient> logger, bool mockMode = true)
    {
        _httpClient = httpClient;
        _logger = logger;
        _mockMode = mockMode;

        // Build resilience pipeline with Circuit Breaker, Retry and Timeout
        _resiliencePipeline = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 3,
                BreakDuration = TimeSpan.FromSeconds(30),
                OnOpened = args =>
                {
                    _logger.LogWarning("⚠️ Circuit breaker OPENED due to failures - failing fast");
                    return ValueTask.CompletedTask;
                },
                OnClosed = args =>
                {
                    _logger.LogInformation("✅ Circuit breaker CLOSED - requests resumed");
                    return ValueTask.CompletedTask;
                },
                OnHalfOpened = args =>
                {
                    _logger.LogInformation("🔄 Circuit breaker HALF-OPEN - testing recovery");
                    return ValueTask.CompletedTask;
                }
            })
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                OnRetry = args =>
                {
                    _logger.LogWarning("🔄 Retrying payment request, attempt {Attempt} of 3", args.AttemptNumber + 1);
                    return ValueTask.CompletedTask;
                }
            })
            .AddTimeout(TimeSpan.FromSeconds(10))
            .Build();
    }

    public async Task<PaymentGatewayResponse> ProcessPaymentAsync(
        Guid transactionId,
        decimal amount,
        string currency,
        string paymentMethod,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing payment {TransactionId} for {Amount} {Currency} via {Method}",
            transactionId, amount, currency, paymentMethod);

        if (_mockMode)
        {
            // Mock implementation for demo (simulates successful payment)
            await Task.Delay(100, cancellationToken); // Simulate network delay

            // Simulate 10% failure rate for testing Circuit Breaker
            var random = new Random();
            if (random.Next(100) < 10)
            {
                _logger.LogWarning("❌ Mock payment FAILED (simulated failure)");
                return new PaymentGatewayResponse(
                    false,
                    null,
                    "Insufficient funds (simulated)",
                    "INSUFFICIENT_FUNDS");
            }

            var mockTransactionId = $"TXN_{transactionId:N}";
            _logger.LogInformation("✅ Mock payment COMPLETED: {TransactionId}", mockTransactionId);

            return new PaymentGatewayResponse(
                true,
                mockTransactionId,
                "Payment processed successfully (mock)");
        }

        try
        {
            var result = await _resiliencePipeline.ExecuteAsync(async ct =>
            {
                var request = new
                {
                    TransactionId = transactionId,
                    Amount = amount,
                    Currency = currency,
                    PaymentMethod = paymentMethod
                };

                var response = await _httpClient.PostAsJsonAsync("/api/payments", request, ct);
                response.EnsureSuccessStatusCode();
                
                var gatewayResult = await response.Content.ReadFromJsonAsync<ExternalPaymentResult>(ct);

                return new PaymentGatewayResponse(
                    gatewayResult?.Success ?? false,
                    gatewayResult?.TransactionId,
                    gatewayResult?.Message ?? "Unknown response");

            }, cancellationToken);

            return result;
        }
        catch (BrokenCircuitException)
        {
            _logger.LogError("❌ Circuit breaker is OPEN - payment gateway unavailable");
            return new PaymentGatewayResponse(
                false,
                null,
                "Payment service temporarily unavailable - please try again later",
                "CIRCUIT_OPEN");
        }
        catch (TimeoutException)
        {
            _logger.LogError("❌ Payment gateway timeout");
            return new PaymentGatewayResponse(
                false,
                null,
                "Payment gateway timeout",
                "TIMEOUT");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Payment processing error");
            return new PaymentGatewayResponse(
                false,
                null,
                $"Payment error: {ex.Message}",
                "GATEWAY_ERROR");
        }
    }

    public async Task<RefundGatewayResponse> ProcessRefundAsync(
        string externalTransactionId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing refund for transaction {TransactionId}, amount {Amount} {Currency}",
            externalTransactionId, amount, currency);

        if (_mockMode)
        {
            // Mock refund (always succeeds)
            await Task.Delay(100, cancellationToken);

            var refundId = $"RFD_{Guid.NewGuid():N}";
            _logger.LogInformation("✅ Mock refund COMPLETED: {RefundId}", refundId);

            return new RefundGatewayResponse(
                true,
                refundId,
                "Refund processed successfully (mock)");
        }

        try
        {
            var result = await _resiliencePipeline.ExecuteAsync(async ct =>
            {
                var request = new
                {
                    TransactionId = externalTransactionId,
                    Amount = amount,
                    Currency = currency
                };

                var response = await _httpClient.PostAsJsonAsync("/api/refunds", request, ct);
                response.EnsureSuccessStatusCode();

                var gatewayResult = await response.Content.ReadFromJsonAsync<ExternalRefundResult>(ct);

                return new RefundGatewayResponse(
                    gatewayResult?.Success ?? false,
                    gatewayResult?.RefundId,
                    gatewayResult?.Message ?? "Unknown response");

            }, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Refund processing error");
            return new RefundGatewayResponse(
                false,
                null,
                $"Refund error: {ex.Message}");
        }
    }
}

// External gateway response models (Anticorruption Layer)
internal record ExternalPaymentResult(bool Success, string? TransactionId, string Message);
internal record ExternalRefundResult(bool Success, string? RefundId, string Message);


