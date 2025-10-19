using BuildingBlocks.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;
using User.Application.DTOs;
using User.Domain.Entities;

namespace User.Application.Queries.Handlers;

/// <summary>
/// Handler for GetUserByIdQuery.
/// </summary>
public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GetUserByIdQueryHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());

        if (user == null)
        {
            return Result.Failure<UserDto>(new Error(
                "User.NotFound",
                $"User with ID {request.UserId} not found"));
        }

        var roles = await _userManager.GetRolesAsync(user);

        var dto = new UserDto(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            user.FullName,
            roles,
            user.CreatedAt,
            user.LastLoginAt,
            user.IsActive
        );

        return Result.Success(dto);
    }
}

