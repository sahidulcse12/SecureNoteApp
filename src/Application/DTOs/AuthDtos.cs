using SecureNoteApp.Domain.Enums;

namespace SecureNoteApp.Application.DTOs;

public record UserDto(
    string Id,
    string Username,
    string Email,
    string FullName,
    UserRole Role,
    List<string> Interests,
    DateTime CreatedAt
);

public record LoginRequest(
    string EmailOrUsername,
    string Password
);

public record RegisterRequest(
    string Username,
    string Email,
    string Password,
    string FullName,
    List<string>? Interests,
    UserRole? Role
);

public record AuthResponse(
    string Token,
    UserDto User
);
