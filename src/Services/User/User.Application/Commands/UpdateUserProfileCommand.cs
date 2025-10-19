using BuildingBlocks.Common.Results;
using MediatR;

namespace User.Application.Commands;

/// <summary>
/// Command to update user profile.
/// </summary>
public record UpdateUserProfileCommand(
    Guid UserId,
    string FirstName,
    string LastName
) : IRequest<Result>;

