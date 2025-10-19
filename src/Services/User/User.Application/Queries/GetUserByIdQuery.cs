using BuildingBlocks.Common.Results;
using MediatR;
using User.Application.DTOs;

namespace User.Application.Queries;

/// <summary>
/// Query to get user by ID.
/// </summary>
public record GetUserByIdQuery(Guid UserId) : IRequest<Result<UserDto>>;

