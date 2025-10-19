namespace Inventory.Domain.Enums;

/// <summary>
/// Overall status of a resource.
/// </summary>
public enum ResourceStatus
{
    Active = 0,
    Inactive = 1,
    UnderMaintenance = 2,
    Decommissioned = 3
}

