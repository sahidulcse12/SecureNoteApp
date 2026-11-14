using System.Security.Claims;
using SecureNoteApp.Application.Common.Interfaces;

namespace SecureNoteApp.WebApi.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public string? UserId =>
        User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User?.FindFirstValue("sub");

    public string? Email =>
        User?.FindFirstValue(ClaimTypes.Email)
        ?? User?.FindFirstValue("email");

    public string? Role =>
        User?.FindFirstValue(ClaimTypes.Role)
        ?? User?.FindFirstValue("role");

    public bool IsAdmin =>
        string.Equals(Role, "Admin", StringComparison.OrdinalIgnoreCase)
        || (User?.IsInRole("Admin") ?? false);

    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated ?? false;
}
