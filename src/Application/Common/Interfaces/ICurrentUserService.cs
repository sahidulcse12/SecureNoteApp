namespace SecureNoteApp.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Email { get; }
    string? Role { get; }
    bool IsAdmin { get; }
    bool IsAuthenticated { get; }
}
