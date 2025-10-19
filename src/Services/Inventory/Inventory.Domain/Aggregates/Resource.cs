using BuildingBlocks.Common.Domain;
using BuildingBlocks.Common.Results;
using Inventory.Domain.Enums;
using Inventory.Domain.Events;
using Inventory.Domain.ValueObjects;

namespace Inventory.Domain.Aggregates;

/// <summary>
/// Resource aggregate root - represents a bookable resource (conference room, coworking space, etc.)
/// </summary>
public class Resource : AggregateRoot
{
    private readonly List<TimeSlot> _availableSlots = new();

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ResourceType Type { get; private set; }
    public Location Location { get; private set; } = null!;
    public Capacity Capacity { get; private set; } = null!;
    public decimal PricePerHour { get; private set; }
    public ResourceStatus Status { get; private set; }
    public IReadOnlyCollection<TimeSlot> AvailableSlots => _availableSlots.AsReadOnly();

    // EF Core constructor
    private Resource() { }

    private Resource(
        Guid id,
        string name,
        string description,
        ResourceType type,
        Location location,
        Capacity capacity,
        decimal pricePerHour)
    {
        Id = id;
        Name = name;
        Description = description;
        Type = type;
        Location = location;
        Capacity = capacity;
        PricePerHour = pricePerHour;
        Status = ResourceStatus.Active;

        AddDomainEvent(new ResourceCreatedDomainEvent(Id, Name, Type.ToString()));
    }

    public static Resource Create(
        string name,
        string description,
        ResourceType type,
        Location location,
        Capacity capacity,
        decimal pricePerHour)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Resource name cannot be empty", nameof(name));

        if (pricePerHour < 0)
            throw new ArgumentException("Price cannot be negative", nameof(pricePerHour));

        return new Resource(
            Guid.NewGuid(),
            name,
            description,
            type,
            location,
            capacity,
            pricePerHour);
    }

    public Result<Guid> ReserveSlot(DateTime startTime, DateTime endTime)
    {
        // Check if resource is active
        if (Status != ResourceStatus.Active)
            return Result.Failure<Guid>(new Error(
                "Resource.NotActive",
                "Resource is not available for booking"));

        // Validate time range
        if (startTime >= endTime)
            return Result.Failure<Guid>(new Error(
                "Resource.InvalidTimeRange",
                "Start time must be before end time"));

        if (startTime < DateTime.UtcNow)
            return Result.Failure<Guid>(new Error(
                "Resource.PastTime",
                "Cannot reserve a slot in the past"));

        // Check for conflicts with existing reservations
        var newSlot = TimeSlot.Create(startTime, endTime, SlotStatus.Reserved);
        var hasConflict = _availableSlots.Any(slot =>
            slot.Status == SlotStatus.Reserved && slot.OverlapsWith(newSlot));

        if (hasConflict)
            return Result.Failure<Guid>(new Error(
                "Resource.SlotConflict",
                "The requested time slot conflicts with an existing reservation"));

        // Create reservation
        var reservationId = Guid.NewGuid();
        _availableSlots.Add(newSlot);

        // Raise domain event
        AddDomainEvent(new ResourceReservedDomainEvent(
            Id,
            startTime,
            endTime,
            reservationId));

        return Result.Success(reservationId);
    }

    public Result ReleaseSlot(DateTime startTime, DateTime endTime)
    {
        var slotToRelease = _availableSlots.FirstOrDefault(s =>
            s.StartTime == startTime && s.EndTime == endTime && s.Status == SlotStatus.Reserved);

        if (slotToRelease == null)
            return Result.Failure(new Error(
                "Resource.SlotNotFound",
                "No reserved slot found for the specified time range"));

        _availableSlots.Remove(slotToRelease);

        return Result.Success();
    }

    public void UpdateDetails(string name, string description, decimal pricePerHour)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Resource name cannot be empty", nameof(name));

        if (pricePerHour < 0)
            throw new ArgumentException("Price cannot be negative", nameof(pricePerHour));

        Name = name;
        Description = description;
        PricePerHour = pricePerHour;
    }

    public void ChangeStatus(ResourceStatus status)
    {
        Status = status;
    }

    public bool IsAvailableAt(DateTime startTime, DateTime endTime)
    {
        if (Status != ResourceStatus.Active)
            return false;

        var checkSlot = TimeSlot.Create(startTime, endTime);
        return !_availableSlots.Any(slot =>
            slot.Status == SlotStatus.Reserved && slot.OverlapsWith(checkSlot));
    }
}

