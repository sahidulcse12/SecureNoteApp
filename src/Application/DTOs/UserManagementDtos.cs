using SecureNoteApp.Domain.Enums;

namespace SecureNoteApp.Application.DTOs;

public record CreateUserRequest(
    string Username,
    string Email,
    string Password,
    string FullName,
    UserRole Role,
    List<string>? Interests
);

public record UpdateUserRequest(
    string Username,
    string Email,
    string FullName,
    UserRole Role,
    List<string>? Interests,
    string? Password
);
