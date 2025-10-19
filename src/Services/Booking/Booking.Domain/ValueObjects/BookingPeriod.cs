using BuildingBlocks.Common.Domain;

namespace Booking.Domain.ValueObjects;

/// <summary>
/// Value object representing a booking time period.
/// </summary>
public class BookingPeriod : ValueObject
{
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }

    private BookingPeriod() { }

    private BookingPeriod(DateTime startTime, DateTime endTime)
    {
        StartTime = startTime;
        EndTime = endTime;
    }

    public static BookingPeriod Create(DateTime startTime, DateTime endTime)
    {
        if (startTime >= endTime)
            throw new ArgumentException("Start time must be before end time");

        if (startTime < DateTime.UtcNow.AddMinutes(-5)) // Allow 5 min tolerance
            throw new ArgumentException("Start time cannot be in the past");

        return new BookingPeriod(startTime, endTime);
    }

    public TimeSpan Duration => EndTime - StartTime;

    public decimal CalculateTotalHours()
    {
        return (decimal)Duration.TotalHours;
    }

    public bool OverlapsWith(BookingPeriod other)
    {
        return StartTime < other.EndTime && EndTime > other.StartTime;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return StartTime;
        yield return EndTime;
    }
}


