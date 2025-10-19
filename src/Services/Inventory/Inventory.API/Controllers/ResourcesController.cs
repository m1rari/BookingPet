using Inventory.Application.Commands;
using Inventory.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers;

/// <summary>
/// Controller for managing resources.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class ResourcesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ResourcesController> _logger;

    public ResourcesController(IMediator mediator, ILogger<ResourcesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Create a new resource.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateResource([FromBody] CreateResourceRequest request)
    {
        var command = new CreateResourceCommand(
            request.Name,
            request.Description,
            request.Type,
            request.Address,
            request.City,
            request.Country,
            request.PostalCode,
            request.MaxPeople,
            request.MinPeople,
            request.PricePerHour);

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to create resource: {Error}", result.Error.Message);
            return BadRequest(new { error = result.Error.Message });
        }

        return CreatedAtAction(nameof(GetResource), new { id = result.Value }, new { id = result.Value });
    }

    /// <summary>
    /// Get a resource by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetResource(Guid id)
    {
        var query = new GetResourceByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error.Message });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Reserve a resource time slot.
    /// </summary>
    [HttpPost("{id}/reserve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ReserveResource(Guid id, [FromBody] ReserveResourceRequest request)
    {
        var command = new ReserveResourceCommand(id, request.StartTime, request.EndTime);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger.LogWarning("Failed to reserve resource {ResourceId}: {Error}", id, result.Error.Message);

            return result.Error.Code switch
            {
                "Resource.NotFound" => NotFound(new { error = result.Error.Message }),
                "Resource.SlotConflict" => Conflict(new { error = result.Error.Message }),
                _ => BadRequest(new { error = result.Error.Message })
            };
        }

        return Ok(new { reservationId = result.Value });
    }
}

// Request DTOs
public record CreateResourceRequest(
    string Name,
    string Description,
    string Type,
    string Address,
    string City,
    string Country,
    string? PostalCode,
    int MaxPeople,
    int MinPeople,
    decimal PricePerHour
);

public record ReserveResourceRequest(
    DateTime StartTime,
    DateTime EndTime
);

