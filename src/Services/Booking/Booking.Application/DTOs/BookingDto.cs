namespace Booking.Application.DTOs;

/// <summary>
/// Booking data transfer object.
/// </summary>
public record BookingDto(
    Guid Id,
    Guid ResourceId,
    Guid UserId,
    DateTime StartTime,
    DateTime EndTime,
    decimal TotalPrice,
    string Currency,
    string Status,
    DateTime CreatedAt,
    DateTime? ConfirmedAt,
    DateTime? CancelledAt,
    string? CancellationReason
);

public record CreateBookingDto(
    Guid ResourceId,
    Guid UserId,
    DateTime StartTime,
    DateTime EndTime
);


