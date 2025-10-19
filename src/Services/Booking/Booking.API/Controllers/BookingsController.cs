using Booking.Application.Commands;
using Booking.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Booking.API.Controllers;

/// <summary>
/// Controller for managing bookings.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class BookingsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(IMediator mediator, ILogger<BookingsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Create a new booking (initiates Saga).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
    {
        var command = new CreateBookingCommand(
            request.ResourceId,
            request.UserId,
            request.StartTime,
            request.EndTime,
            request.PricePerHour);

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to create booking: {Error}", result.Error.Message);
            return BadRequest(new { error = result.Error.Message });
        }

        return CreatedAtAction(nameof(GetBooking), new { id = result.Value }, new { id = result.Value });
    }

    /// <summary>
    /// Get all bookings.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllBookings()
    {
        var query = new GetAllBookingsQuery();
        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to get all bookings: {Error}", result.Error.Message);
            return BadRequest(new { error = result.Error.Message });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get a booking by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBooking(Guid id)
    {
        var query = new GetBookingByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error.Message });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Cancel a booking.
    /// </summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelBooking(Guid id, [FromBody] CancelBookingRequest request)
    {
        var command = new CancelBookingCommand(id, request.Reason);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to cancel booking {BookingId}: {Error}", id, result.Error.Message);

            return result.Error.Code switch
            {
                "Booking.NotFound" => NotFound(new { error = result.Error.Message }),
                _ => BadRequest(new { error = result.Error.Message })
            };
        }

        return Ok(new { message = "Booking cancelled successfully" });
    }

    /// <summary>
    /// Confirm a booking (internal endpoint, called by event handlers).
    /// </summary>
    [HttpPost("{id}/confirm")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmBooking(Guid id)
    {
        var command = new ConfirmBookingCommand(id);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to confirm booking {BookingId}: {Error}", id, result.Error.Message);

            return result.Error.Code switch
            {
                "Booking.NotFound" => NotFound(new { error = result.Error.Message }),
                _ => BadRequest(new { error = result.Error.Message })
            };
        }

        return Ok(new { message = "Booking confirmed successfully" });
    }
}

// Request DTOs
public record CreateBookingRequest(
    Guid ResourceId,
    Guid UserId,
    DateTime StartTime,
    DateTime EndTime,
    decimal PricePerHour
);

public record CancelBookingRequest(
    string Reason
);


