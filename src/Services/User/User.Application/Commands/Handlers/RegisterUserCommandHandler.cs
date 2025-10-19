using BuildingBlocks.Common.Results;
using BuildingBlocks.EventBus;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using User.Application.IntegrationEvents;
using User.Domain.Entities;

namespace User.Application.Commands.Handlers;

/// <summary>
/// Handler for RegisterUserCommand.
/// </summary>
public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<Guid>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEventBus _eventBus;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(
        UserManager<ApplicationUser> userManager,
        IEventBus eventBus,
        ILogger<RegisterUserCommandHandler> logger)
    {
        _userManager = userManager;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return Result.Failure<Guid>(new Error(
                "User.AlreadyExists",
                $"User with email {request.Email} already exists"));
        }

        // Create new user
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            EmailConfirmed = true // For simplicity, auto-confirm emails
        };

        // Create user with password
        var createResult = await _userManager.CreateAsync(user, request.Password);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            _logger.LogWarning("Failed to create user {Email}: {Errors}", request.Email, errors);
            
            return Result.Failure<Guid>(new Error(
                "User.CreationFailed",
                $"Failed to create user: {errors}"));
        }

        // Assign role
        var role = request.Role ?? "Customer";
        var roleResult = await _userManager.AddToRoleAsync(user, role);

        if (!roleResult.Succeeded)
        {
            _logger.LogWarning("Failed to assign role {Role} to user {UserId}", role, user.Id);
        }

        _logger.LogInformation("User {Email} registered successfully with ID {UserId}", request.Email, user.Id);

        // Publish integration event
        await _eventBus.PublishAsync(new UserRegisteredIntegrationEvent(
            user.Id,
            user.Email!,
            user.FullName
        ), cancellationToken);

        return Result.Success(user.Id);
    }
}

