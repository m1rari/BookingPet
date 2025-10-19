using BuildingBlocks.Common.Results;
using Booking.Application.DTOs;
using MediatR;

namespace Booking.Application.Queries;

/// <summary>
/// Query to get all bookings.
/// </summary>
public record GetAllBookingsQuery : IRequest<Result<IEnumerable<BookingDto>>>;
