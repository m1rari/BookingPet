using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Commands;

namespace Payment.API.Controllers;

/// <summary>
/// Controller for payment operations.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IMediator mediator, ILogger<PaymentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Process a payment (with Circuit Breaker!).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentRequest request)
    {
        var command = new ProcessPaymentCommand(
            request.BookingId,
            request.UserId,
            request.Amount,
            request.Currency,
            request.PaymentMethod);

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger.LogWarning("Payment processing failed: {Error}", result.Error.Message);

            return result.Error.Code switch
            {
                "Payment.GatewayError" => StatusCode(503, new { error = result.Error.Message }),
                _ => BadRequest(new { error = result.Error.Message })
            };
        }

        return Ok(new
        {
            paymentId = result.Value,
            message = "Payment processed successfully"
        });
    }

    /// <summary>
    /// Refund a payment.
    /// </summary>
    [HttpPost("{id}/refund")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RefundPayment(Guid id, [FromBody] RefundPaymentRequest request)
    {
        var command = new RefundPaymentCommand(id, request.Reason);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger.LogWarning("Refund failed for payment {PaymentId}: {Error}", id, result.Error.Message);

            return result.Error.Code switch
            {
                "Payment.NotFound" => NotFound(new { error = result.Error.Message }),
                _ => BadRequest(new { error = result.Error.Message })
            };
        }

        return Ok(new { message = "Payment refunded successfully" });
    }
}

// Request DTOs
public record ProcessPaymentRequest(
    Guid BookingId,
    Guid UserId,
    decimal Amount,
    string Currency = "USD",
    string PaymentMethod = "CreditCard"
);

public record RefundPaymentRequest(
    string Reason
);

