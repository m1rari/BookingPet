using BuildingBlocks.Common.Results;
using Inventory.Application.DTOs;
using MediatR;

namespace Inventory.Application.Queries;

/// <summary>
/// Query to get a resource by ID.
/// </summary>
public record GetResourceByIdQuery(Guid ResourceId) : IRequest<Result<ResourceDto>>;

