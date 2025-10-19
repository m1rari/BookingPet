using BuildingBlocks.Common.Domain;
using Inventory.Domain.Enums;

namespace Inventory.Domain.ValueObjects;

/// <summary>
/// Value object representing a time slot for resource availability.
/// </summary>
public class TimeSlot : ValueObject
{
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public SlotStatus Status { get; private set; }

    private TimeSlot() { }

    private TimeSlot(DateTime startTime, DateTime endTime, SlotStatus status = SlotStatus.Available)
    {
        StartTime = startTime;
        EndTime = endTime;
        Status = status;
    }

    public static TimeSlot Create(DateTime startTime, DateTime endTime, SlotStatus status = SlotStatus.Available)
    {
        if (startTime >= endTime)
            throw new ArgumentException("Start time must be before end time");

        if (startTime < DateTime.UtcNow)
            throw new ArgumentException("Start time cannot be in the past");

        return new TimeSlot(startTime, endTime, status);
    }

    public TimeSpan Duration => EndTime - StartTime;

    public bool OverlapsWith(TimeSlot other)
    {
        return StartTime < other.EndTime && EndTime > other.StartTime;
    }

    public TimeSlot MarkAsReserved()
    {
        return new TimeSlot(StartTime, EndTime, SlotStatus.Reserved);
    }

    public TimeSlot MarkAsBlocked()
    {
        return new TimeSlot(StartTime, EndTime, SlotStatus.Blocked);
    }

    public bool IsAvailable => Status == SlotStatus.Available;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return StartTime;
        yield return EndTime;
    }
}

