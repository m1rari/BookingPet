using BuildingBlocks.Common.Results;
using MediatR;

namespace Inventory.Application.Commands;

/// <summary>
/// Command to create a new resource.
/// </summary>
public record CreateResourceCommand(
    string Name,
    string Description,
    string Type,
    string Address,
    string City,
    string Country,
    string? PostalCode,
    int MaxPeople,
    int MinPeople,
    decimal PricePerHour
) : IRequest<Result<Guid>>;

