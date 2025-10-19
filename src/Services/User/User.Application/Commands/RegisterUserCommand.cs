using BuildingBlocks.Common.Results;
using MediatR;

namespace User.Application.Commands;

/// <summary>
/// Command to register a new user.
/// </summary>
public record RegisterUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? Role = "Customer"
) : IRequest<Result<Guid>>;


