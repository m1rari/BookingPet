using BuildingBlocks.Common.Results;
using Booking.Application.Sagas;
using MediatR;

namespace Booking.Application.Commands.Handlers;

/// <summary>
/// Handler for CreateBookingCommand - initiates the booking Saga.
/// </summary>
public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, Result<Guid>>
{
    private readonly CreateBookingSaga _saga;

    public CreateBookingCommandHandler(CreateBookingSaga saga)
    {
        _saga = saga;
    }

    public async Task<Result<Guid>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        return await _saga.ExecuteAsync(
            request.ResourceId,
            request.UserId,
            request.StartTime,
            request.EndTime,
            request.PricePerHour,
            cancellationToken);
    }
}


