using BuildingBlocks.Common.Domain;
using BuildingBlocks.Common.Results;
using Booking.Domain.Enums;
using Booking.Domain.Events;
using Booking.Domain.ValueObjects;

namespace Booking.Domain.Aggregates;

/// <summary>
/// Booking aggregate root - represents a booking/reservation.
/// </summary>
public class Booking : AggregateRoot
{
    public Guid ResourceId { get; private set; }
    public Guid UserId { get; private set; }
    public BookingPeriod Period { get; private set; } = null!;
    public Money TotalPrice { get; private set; } = null!;
    public BookingStatus Status { get; private set; }
    public string? CancellationReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    // Optimistic concurrency token for EF Core
    public byte[]? RowVersion { get; private set; }

    // EF Core constructor
    private Booking() { }

    private Booking(
        Guid id,
        Guid resourceId,
        Guid userId,
        BookingPeriod period,
        Money totalPrice)
    {
        Id = id;
        ResourceId = resourceId;
        UserId = userId;
        Period = period;
        TotalPrice = totalPrice;
        Status = BookingStatus.Pending;
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new BookingCreatedDomainEvent(
            Id, ResourceId, UserId, Period.StartTime, Period.EndTime));
    }

    public static Booking Create(
        Guid resourceId,
        Guid userId,
        BookingPeriod period,
        Money totalPrice)
    {
        if (resourceId == Guid.Empty)
            throw new ArgumentException("Resource ID cannot be empty", nameof(resourceId));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        return new Booking(Guid.NewGuid(), resourceId, userId, period, totalPrice);
    }

    public Result Confirm()
    {
        if (Status != BookingStatus.Pending)
            return Result.Failure(new Error(
                "Booking.InvalidStatus",
                "Only pending bookings can be confirmed"));

        Status = BookingStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;

        AddDomainEvent(new BookingConfirmedDomainEvent(Id));

        return Result.Success();
    }

    public Result Cancel(string reason)
    {
        if (Status == BookingStatus.Cancelled)
            return Result.Failure(new Error(
                "Booking.AlreadyCancelled",
                "Booking is already cancelled"));

        if (Status == BookingStatus.Completed)
            return Result.Failure(new Error(
                "Booking.AlreadyCompleted",
                "Cannot cancel a completed booking"));

        // Check if it's too late to cancel (e.g., within 24 hours of start)
        if (Period.StartTime <= DateTime.UtcNow.AddHours(24))
        {
            return Result.Failure(new Error(
                "Booking.TooLateToCancel",
                "Cannot cancel booking less than 24 hours before start time"));
        }

        Status = BookingStatus.Cancelled;
        CancellationReason = reason;
        CancelledAt = DateTime.UtcNow;

        AddDomainEvent(new BookingCancelledDomainEvent(Id, reason));

        return Result.Success();
    }

    public Result MarkAsFailed()
    {
        if (Status != BookingStatus.Pending)
            return Result.Failure(new Error(
                "Booking.InvalidStatus",
                "Only pending bookings can be marked as failed"));

        Status = BookingStatus.Failed;
        return Result.Success();
    }

    public Result Complete()
    {
        if (Status != BookingStatus.Confirmed)
            return Result.Failure(new Error(
                "Booking.InvalidStatus",
                "Only confirmed bookings can be completed"));

        if (Period.EndTime > DateTime.UtcNow)
            return Result.Failure(new Error(
                "Booking.NotYetFinished",
                "Cannot complete a booking that hasn't finished yet"));

        Status = BookingStatus.Completed;
        return Result.Success();
    }

    public bool IsPending => Status == BookingStatus.Pending;
    public bool IsConfirmed => Status == BookingStatus.Confirmed;
    public bool IsCancelled => Status == BookingStatus.Cancelled;
    public bool IsActive => Status == BookingStatus.Confirmed || Status == BookingStatus.Pending;
}


