using BuildingBlocks.Common.Results;
using Booking.Application.Contracts;
using Booking.Application.DTOs;
using MediatR;

namespace Booking.Application.Queries.Handlers;

/// <summary>
/// Handler for GetAllBookingsQuery.
/// </summary>
public class GetAllBookingsQueryHandler : IRequestHandler<GetAllBookingsQuery, Result<IEnumerable<BookingDto>>>
{
    private readonly IBookingRepository _repository;

    public GetAllBookingsQueryHandler(IBookingRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<BookingDto>>> Handle(GetAllBookingsQuery request, CancellationToken cancellationToken)
    {
        var bookings = await _repository.GetAllAsync(cancellationToken);

        var dtos = bookings.Select(booking => new BookingDto(
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
            booking.CancellationReason));

        return Result.Success(dtos);
    }
}
