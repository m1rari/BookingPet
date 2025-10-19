using BuildingBlocks.Common.Results;
using Inventory.Application.Contracts;
using Inventory.Application.DTOs;
using MediatR;

namespace Inventory.Application.Queries.Handlers;

/// <summary>
/// Handler for GetResourceByIdQuery.
/// </summary>
public class GetResourceByIdQueryHandler : IRequestHandler<GetResourceByIdQuery, Result<ResourceDto>>
{
    private readonly IResourceRepository _repository;

    public GetResourceByIdQueryHandler(IResourceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ResourceDto>> Handle(GetResourceByIdQuery request, CancellationToken cancellationToken)
    {
        var resource = await _repository.GetByIdAsync(request.ResourceId, cancellationToken);

        if (resource == null)
        {
            return Result.Failure<ResourceDto>(new Error(
                "Resource.NotFound",
                $"Resource with ID {request.ResourceId} not found"));
        }

        var dto = new ResourceDto(
            resource.Id,
            resource.Name,
            resource.Description,
            resource.Type.ToString(),
            new LocationDto(
                resource.Location.Address,
                resource.Location.City,
                resource.Location.Country,
                resource.Location.PostalCode,
                resource.Location.Latitude,
                resource.Location.Longitude),
            new CapacityDto(
                resource.Capacity.MaxPeople,
                resource.Capacity.MinPeople),
            resource.PricePerHour,
            resource.Status.ToString());

        return Result.Success(dto);
    }
}

