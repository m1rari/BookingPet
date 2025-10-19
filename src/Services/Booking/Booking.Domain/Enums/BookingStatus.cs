namespace Booking.Domain.Enums;

/// <summary>
/// Status of a booking.
/// </summary>
public enum BookingStatus
{
    Pending = 0,        // Ожидает подтверждения
    Confirmed = 1,      // Подтверждено
    Cancelled = 2,      // Отменено
    Completed = 3,      // Завершено
    Failed = 4          // Не удалось создать
}


