using BuildingBlocks.Common.Results;
using Booking.Application.DTOs;
using MediatR;

namespace Booking.Application.Queries;

/// <summary>
/// Query to get a booking by ID.
/// </summary>
public record GetBookingByIdQuery(Guid BookingId) : IRequest<Result<BookingDto>>;


