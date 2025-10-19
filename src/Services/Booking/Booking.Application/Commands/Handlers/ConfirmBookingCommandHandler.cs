using BuildingBlocks.Common.Results;
using BuildingBlocks.EventBus;
using Booking.Application.Contracts;
using Booking.Application.IntegrationEvents;
using MediatR;

namespace Booking.Application.Commands.Handlers;

/// <summary>
/// Handler for ConfirmBookingCommand.
/// </summary>
public class ConfirmBookingCommandHandler : IRequestHandler<ConfirmBookingCommand, Result>
{
    private readonly IBookingRepository _repository;
    private readonly IEventBus _eventBus;

    public ConfirmBookingCommandHandler(IBookingRepository repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async Task<Result> Handle(ConfirmBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _repository.GetByIdAsync(request.BookingId, cancellationToken);

        if (booking == null)
        {
            return Result.Failure(new Error(
                "Booking.NotFound",
                $"Booking with ID {request.BookingId} not found"));
        }

        var result = booking.Confirm();

        if (result.IsFailure)
        {
            return result;
        }

        await _repository.UnitOfWork.SaveChangesAsync(cancellationToken);

        // Publish integration event
        await _eventBus.PublishAsync(new BookingConfirmedIntegrationEvent(
            booking.Id,
            booking.ResourceId,
            booking.UserId
        ), cancellationToken);

        return Result.Success();
    }
}


