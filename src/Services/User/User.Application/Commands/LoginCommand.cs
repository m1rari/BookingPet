using BuildingBlocks.Common.Results;
using MediatR;
using User.Application.DTOs;

namespace User.Application.Commands;

/// <summary>
/// Command to login a user.
/// </summary>
public record LoginCommand(string Email, string Password) : IRequest<Result<LoginResponseDto>>;


