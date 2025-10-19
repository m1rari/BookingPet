using BuildingBlocks.Authentication;
using BuildingBlocks.Common.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using User.Application.DTOs;
using User.Domain.Entities;

namespace User.Application.Commands.Handlers;

/// <summary>
/// Handler for LoginCommand.
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponseDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenGenerator jwtTokenGenerator,
        ILogger<LoginCommandHandler> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _logger = logger;
    }

    public async Task<Result<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Find user by email
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("Login attempt with non-existent email: {Email}", request.Email);
            return Result.Failure<LoginResponseDto>(new Error(
                "User.InvalidCredentials",
                "Invalid email or password"));
        }

        // Check if user is active
        if (!user.IsActive)
        {
            return Result.Failure<LoginResponseDto>(new Error(
                "User.Inactive",
                "User account is inactive"));
        }

        // Verify password
        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (!signInResult.Succeeded)
        {
            if (signInResult.IsLockedOut)
            {
                _logger.LogWarning("User {UserId} is locked out", user.Id);
                return Result.Failure<LoginResponseDto>(new Error(
                    "User.LockedOut",
                    "Account is locked due to multiple failed login attempts"));
            }

            _logger.LogWarning("Failed login attempt for user {Email}", request.Email);
            return Result.Failure<LoginResponseDto>(new Error(
                "User.InvalidCredentials",
                "Invalid email or password"));
        }

        // Get user roles
        var roles = await _userManager.GetRolesAsync(user);

        // Generate JWT tokens
        var accessToken = _jwtTokenGenerator.GenerateToken(user.Id, user.Email!, roles);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("User {Email} logged in successfully", request.Email);

        // Create response
        var userDto = new UserDto(
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

        var response = new LoginResponseDto(accessToken, refreshToken, userDto);

        return Result.Success(response);
    }
}

