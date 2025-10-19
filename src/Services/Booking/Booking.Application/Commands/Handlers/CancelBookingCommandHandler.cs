using BuildingBlocks.Common.Results;
using BuildingBlocks.EventBus;
using Booking.Application.Contracts;
using Booking.Application.IntegrationEvents;
using MediatR;

namespace Booking.Application.Commands.Handlers;

/// <summary>
/// Handler for CancelBookingCommand.
/// </summary>
public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, Result>
{
    private readonly IBookingRepository _repository;
    private readonly IEventBus _eventBus;

    public CancelBookingCommandHandler(IBookingRepository repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _repository.GetByIdAsync(request.BookingId, cancellationToken);

        if (booking == null)
        {
            return Result.Failure(new Error(
                "Booking.NotFound",
                $"Booking with ID {request.BookingId} not found"));
        }

        var result = booking.Cancel(request.Reason);

        if (result.IsFailure)
        {
            return result;
        }

        await _repository.UnitOfWork.SaveChangesAsync(cancellationToken);

        // Publish integration event
        await _eventBus.PublishAsync(new BookingCancelledIntegrationEvent(
            booking.Id,
            booking.ResourceId,
            request.Reason
        ), cancellationToken);

        return Result.Success();
    }
}


