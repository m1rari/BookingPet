using BuildingBlocks.Common.Results;
using MediatR;

namespace Inventory.Application.Commands;

/// <summary>
/// Command to reserve a resource time slot.
/// </summary>
public record ReserveResourceCommand(
    Guid ResourceId,
    DateTime StartTime,
    DateTime EndTime
) : IRequest<Result<Guid>>;

