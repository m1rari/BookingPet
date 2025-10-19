using BuildingBlocks.Common.Results;
using BuildingBlocks.EventBus;
using Inventory.Application.Contracts;
using Inventory.Application.IntegrationEvents;
using MediatR;

namespace Inventory.Application.Commands.Handlers;

/// <summary>
/// Handler for ReserveResourceCommand with distributed locking.
/// </summary>
public class ReserveResourceCommandHandler : IRequestHandler<ReserveResourceCommand, Result<Guid>>
{
    private readonly IResourceRepository _repository;
    private readonly IDistributedLockService _lockService;
    private readonly IEventBus _eventBus;

    public ReserveResourceCommandHandler(
        IResourceRepository repository,
        IDistributedLockService lockService,
        IEventBus eventBus)
    {
        _repository = repository;
        _lockService = lockService;
        _eventBus = eventBus;
    }

    public async Task<Result<Guid>> Handle(ReserveResourceCommand request, CancellationToken cancellationToken)
    {
        // Acquire distributed lock on the resource
        await using var lockHandle = await _lockService.AcquireLockAsync(
            $"resource:{request.ResourceId}",
            TimeSpan.FromSeconds(30));

        if (lockHandle == null)
        {
            return Result.Failure<Guid>(new Error(
                "Resource.LockAcquisitionFailed",
                "Could not acquire lock on resource"));
        }

        // Load resource with pessimistic locking
        var resource = await _repository.GetByIdWithLockAsync(request.ResourceId, cancellationToken);

        if (resource == null)
        {
            return Result.Failure<Guid>(new Error(
                "Resource.NotFound",
                $"Resource with ID {request.ResourceId} not found"));
        }

        // Execute business logic
        var result = resource.ReserveSlot(request.StartTime, request.EndTime);

        if (result.IsFailure)
        {
            return result;
        }

        // Save changes
        await _repository.UnitOfWork.SaveChangesAsync(cancellationToken);

        // Publish integration event
        await _eventBus.PublishAsync(new ResourceReservedIntegrationEvent(
            resource.Id,
            request.StartTime,
            request.EndTime,
            result.Value), cancellationToken);

        return result;
    }
}

