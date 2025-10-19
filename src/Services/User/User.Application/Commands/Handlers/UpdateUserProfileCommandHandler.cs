using BuildingBlocks.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;
using User.Domain.Entities;

namespace User.Application.Commands.Handlers;

/// <summary>
/// Handler for UpdateUserProfileCommand.
/// </summary>
public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UpdateUserProfileCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());

        if (user == null)
        {
            return Result.Failure(new Error(
                "User.NotFound",
                $"User with ID {request.UserId} not found"));
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure(new Error(
                "User.UpdateFailed",
                $"Failed to update user: {errors}"));
        }

        return Result.Success();
    }
}

