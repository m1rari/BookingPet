using BuildingBlocks.Common.Domain;

namespace Inventory.Domain.ValueObjects;

/// <summary>
/// Value object representing resource capacity.
/// </summary>
public class Capacity : ValueObject
{
    public int MaxPeople { get; private set; }
    public int MinPeople { get; private set; }

    private Capacity() { }

    private Capacity(int maxPeople, int minPeople = 1)
    {
        MaxPeople = maxPeople;
        MinPeople = minPeople;
    }

    public static Capacity Create(int maxPeople, int minPeople = 1)
    {
        if (maxPeople <= 0)
            throw new ArgumentException("Max people must be greater than zero", nameof(maxPeople));

        if (minPeople <= 0)
            throw new ArgumentException("Min people must be greater than zero", nameof(minPeople));

        if (minPeople > maxPeople)
            throw new ArgumentException("Min people cannot exceed max people", nameof(minPeople));

        return new Capacity(maxPeople, minPeople);
    }

    public bool CanAccommodate(int numberOfPeople)
    {
        return numberOfPeople >= MinPeople && numberOfPeople <= MaxPeople;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return MaxPeople;
        yield return MinPeople;
    }
}

