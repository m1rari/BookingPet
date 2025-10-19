namespace Payment.Application.DTOs;

public record PaymentDto(
    Guid Id,
    Guid BookingId,
    Guid UserId,
    decimal Amount,
    string Currency,
    string Status,
    string? ExternalTransactionId,
    string? PaymentMethod,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    DateTime? RefundedAt,
    string? FailureReason
);

