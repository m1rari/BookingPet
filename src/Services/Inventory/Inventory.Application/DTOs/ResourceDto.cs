namespace Inventory.Application.DTOs;

/// <summary>
/// Resource data transfer object.
/// </summary>
public record ResourceDto(
    Guid Id,
    string Name,
    string Description,
    string Type,
    LocationDto Location,
    CapacityDto Capacity,
    decimal PricePerHour,
    string Status
);

public record LocationDto(
    string Address,
    string City,
    string Country,
    string? PostalCode,
    double? Latitude,
    double? Longitude
);

public record CapacityDto(
    int MaxPeople,
    int MinPeople
);

public record TimeSlotDto(
    DateTime StartTime,
    DateTime EndTime,
    string Status
);

