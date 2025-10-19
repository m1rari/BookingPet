using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using User.Application.Commands;
using User.Application.Queries;

namespace User.API.Controllers;

/// <summary>
/// Controller for authentication operations.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var command = new RegisterUserCommand(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.Role);

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger.LogWarning("Registration failed for {Email}: {Error}", request.Email, result.Error.Message);
            return BadRequest(new { error = result.Error.Message });
        }

        return CreatedAtAction(
            nameof(UsersController.GetUser),
            "Users",
            new { id = result.Value },
            new { userId = result.Value, message = "User registered successfully" });
    }

    /// <summary>
    /// Login user and get JWT token.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger.LogWarning("Login failed for {Email}: {Error}", request.Email, result.Error.Message);
            return Unauthorized(new { error = result.Error.Message });
        }

        _logger.LogInformation("User {Email} logged in successfully", request.Email);

        return Ok(new
        {
            accessToken = result.Value.AccessToken,
            refreshToken = result.Value.RefreshToken,
            user = result.Value.User
        });
    }

    /// <summary>
    /// Get current authenticated user info.
    /// </summary>
    [HttpGet("me")]
    [Authorize(Policy = "UserAuth")] // Require User JWT token
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst("sub")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Invalid token" });
        }

        var query = new GetUserByIdQuery(userId);
        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error.Message });
        }

        return Ok(result.Value);
    }
}

// Request DTOs
public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? Role = "Customer"
);

public record LoginRequest(
    string Email,
    string Password
);

