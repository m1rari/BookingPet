using BuildingBlocks.Common.Results;
using Booking.Application.Contracts;
using Booking.Application.DTOs;
using MediatR;

namespace Booking.Application.Queries.Handlers;

/// <summary>
/// Handler for GetBookingByIdQuery.
/// </summary>
public class GetBookingByIdQueryHandler : IRequestHandler<GetBookingByIdQuery, Result<BookingDto>>
{
    private readonly IBookingRepository _repository;

    public GetBookingByIdQueryHandler(IBookingRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<BookingDto>> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
    {
        var booking = await _repository.GetByIdAsync(request.BookingId, cancellationToken);

        if (booking == null)
        {
            return Result.Failure<BookingDto>(new Error(
                "Booking.NotFound",
                $"Booking with ID {request.BookingId} not found"));
        }

        var dto = new BookingDto(
            booking.Id,
            booking.ResourceId,
            booking.UserId,
            booking.Period.StartTime,
            booking.Period.EndTime,
            booking.TotalPrice.Amount,
            booking.TotalPrice.Currency,
            booking.Status.ToString(),
            booking.CreatedAt,
            booking.ConfirmedAt,
            booking.CancelledAt,
            booking.CancellationReason);

        return Result.Success(dto);
    }
}


