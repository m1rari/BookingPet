namespace User.Application.DTOs;

/// <summary>
/// User data transfer objects.
/// </summary>
public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    IEnumerable<string> Roles,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    bool IsActive
);

public record LoginResponseDto(
    string AccessToken,
    string RefreshToken,
    UserDto User
);

public record RefreshTokenDto(
    string RefreshToken
);

