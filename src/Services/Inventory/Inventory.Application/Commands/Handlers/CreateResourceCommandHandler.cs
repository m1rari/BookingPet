using BuildingBlocks.Common.Results;
using Inventory.Application.Contracts;
using Inventory.Domain.Aggregates;
using Inventory.Domain.Enums;
using Inventory.Domain.ValueObjects;
using MediatR;

namespace Inventory.Application.Commands.Handlers;

/// <summary>
/// Handler for CreateResourceCommand.
/// </summary>
public class CreateResourceCommandHandler : IRequestHandler<CreateResourceCommand, Result<Guid>>
{
    private readonly IResourceRepository _repository;

    public CreateResourceCommandHandler(IResourceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> Handle(CreateResourceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Parse resource type
            if (!Enum.TryParse<ResourceType>(request.Type, true, out var resourceType))
            {
                return Result.Failure<Guid>(new Error(
                    "Resource.InvalidType",
                    $"Invalid resource type: {request.Type}"));
            }

            // Create value objects
            var location = Location.Create(
                request.Address,
                request.City,
                request.Country,
                request.PostalCode);

            var capacity = Capacity.Create(request.MaxPeople, request.MinPeople);

            // Create resource aggregate
            var resource = Resource.Create(
                request.Name,
                request.Description,
                resourceType,
                location,
                capacity,
                request.PricePerHour);

            // Save to repository
            await _repository.AddAsync(resource, cancellationToken);
            await _repository.UnitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(resource.Id);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<Guid>(new Error("Resource.ValidationError", ex.Message));
        }
    }
}

