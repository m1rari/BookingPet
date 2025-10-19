using BuildingBlocks.Common.Domain;
using BuildingBlocks.Common.Results;
using Payment.Domain.Enums;
using Payment.Domain.Events;
using Payment.Domain.ValueObjects;

namespace Payment.Domain.Aggregates;

/// <summary>
/// Payment aggregate root - represents a payment transaction.
/// </summary>
public class Payment : AggregateRoot
{
    public Guid BookingId { get; private set; }
    public Guid UserId { get; private set; }
    public Money Amount { get; private set; } = null!;
    public PaymentStatus Status { get; private set; }
    public string? ExternalTransactionId { get; private set; }
    public string? PaymentMethod { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? RefundedAt { get; private set; }

    // EF Core constructor
    private Payment() { }

    private Payment(
        Guid id,
        Guid bookingId,
        Guid userId,
        Money amount,
        string paymentMethod)
    {
        Id = id;
        BookingId = bookingId;
        UserId = userId;
        Amount = amount;
        PaymentMethod = paymentMethod;
        Status = PaymentStatus.Pending;
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new PaymentInitiatedDomainEvent(Id, BookingId, Amount.Amount));
    }

    public static Payment Create(
        Guid bookingId,
        Guid userId,
        Money amount,
        string paymentMethod = "CreditCard")
    {
        if (bookingId == Guid.Empty)
            throw new ArgumentException("Booking ID cannot be empty", nameof(bookingId));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (string.IsNullOrWhiteSpace(paymentMethod))
            throw new ArgumentException("Payment method cannot be empty", nameof(paymentMethod));

        return new Payment(Guid.NewGuid(), bookingId, userId, amount, paymentMethod);
    }

    public Result Complete(string externalTransactionId)
    {
        if (Status != PaymentStatus.Pending)
            return Result.Failure(new Error(
                "Payment.InvalidStatus",
                "Only pending payments can be completed"));

        if (string.IsNullOrWhiteSpace(externalTransactionId))
            throw new ArgumentException("External transaction ID is required", nameof(externalTransactionId));

        Status = PaymentStatus.Completed;
        ExternalTransactionId = externalTransactionId;
        CompletedAt = DateTime.UtcNow;

        AddDomainEvent(new PaymentCompletedDomainEvent(Id, BookingId, ExternalTransactionId));

        return Result.Success();
    }

    public Result Fail(string reason)
    {
        if (Status != PaymentStatus.Pending)
            return Result.Failure(new Error(
                "Payment.InvalidStatus",
                "Only pending payments can be marked as failed"));

        Status = PaymentStatus.Failed;
        FailureReason = reason;

        AddDomainEvent(new PaymentFailedDomainEvent(Id, BookingId, reason));

        return Result.Success();
    }

    public Result Refund(string reason)
    {
        if (Status != PaymentStatus.Completed)
            return Result.Failure(new Error(
                "Payment.CannotRefund",
                "Only completed payments can be refunded"));

        // Check if refund window is still open (e.g., 30 days)
        if (CompletedAt.HasValue && DateTime.UtcNow - CompletedAt.Value > TimeSpan.FromDays(30))
        {
            return Result.Failure(new Error(
                "Payment.RefundWindowClosed",
                "Refund window (30 days) has expired"));
        }

        Status = PaymentStatus.Refunded;
        RefundedAt = DateTime.UtcNow;
        FailureReason = reason;

        AddDomainEvent(new PaymentRefundedDomainEvent(Id, BookingId, Amount.Amount, reason));

        return Result.Success();
    }

    public bool IsCompleted => Status == PaymentStatus.Completed;
    public bool IsPending => Status == PaymentStatus.Pending;
    public bool IsRefundable => Status == PaymentStatus.Completed && 
        CompletedAt.HasValue && 
        DateTime.UtcNow - CompletedAt.Value <= TimeSpan.FromDays(30);
}

