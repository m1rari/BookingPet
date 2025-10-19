using BuildingBlocks.Common.Domain;

namespace Inventory.Domain.ValueObjects;

/// <summary>
/// Value object representing a physical location.
/// </summary>
public class Location : ValueObject
{
    public string Address { get; private set; }
    public string City { get; private set; }
    public string Country { get; private set; }
    public string? PostalCode { get; private set; }
    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }

    private Location() { }

    private Location(
        string address,
        string city,
        string country,
        string? postalCode = null,
        double? latitude = null,
        double? longitude = null)
    {
        Address = address;
        City = city;
        Country = country;
        PostalCode = postalCode;
        Latitude = latitude;
        Longitude = longitude;
    }

    public static Location Create(
        string address,
        string city,
        string country,
        string? postalCode = null,
        double? latitude = null,
        double? longitude = null)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address cannot be empty", nameof(address));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty", nameof(city));

        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be empty", nameof(country));

        return new Location(address, city, country, postalCode, latitude, longitude);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Address;
        yield return City;
        yield return Country;
        if (PostalCode != null) yield return PostalCode;
        if (Latitude.HasValue) yield return Latitude.Value;
        if (Longitude.HasValue) yield return Longitude.Value;
    }
}

