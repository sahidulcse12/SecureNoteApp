using SecureNoteApp.Application.Common.Exceptions;
using SecureNoteApp.Application.Common.Interfaces;
using SecureNoteApp.Application.Common.Models;
using SecureNoteApp.Application.DTOs;
using SecureNoteApp.Domain.Entities;
using SecureNoteApp.Domain.Enums;

namespace SecureNoteApp.Application.Services;

public interface IUserService
{
    Task<PagedResult<UserDto>> GetUsersPagedAsync(PaginationParams pagination);
    Task<UserDto> GetUserByIdAsync(string id);
    Task<UserDto> CreateUserAsync(CreateUserRequest request);
    Task<UserDto> UpdateUserAsync(string id, UpdateUserRequest request);
    Task DeleteUserAsync(string id);
}

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUserService;

    public UserService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<UserDto>> GetUsersPagedAsync(PaginationParams pagination)
    {
        EnsureAdmin();

        var pagedUsers = await _userRepository.GetPagedAsync(pagination);
        var dtos = pagedUsers.Items.Select(MapToDto).ToList();

        return new PagedResult<UserDto>(dtos, pagedUsers.TotalCount, pagedUsers.PageNumber, pagedUsers.PageSize);
    }

    public async Task<UserDto> GetUserByIdAsync(string id)
    {
        // Admin or the user themselves can view user details
        var currentUserId = _currentUserService.UserId;
        if (!_currentUserService.IsAdmin && currentUserId != id)
            throw new ForbiddenException("You cannot view this user profile.");

        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new NotFoundException("User", id);

        return MapToDto(user);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
    {
        EnsureAdmin();

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new BadRequestException("Email is required.");
        if (string.IsNullOrWhiteSpace(request.Username))
            throw new BadRequestException("Username is required.");
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            throw new BadRequestException("Password must be at least 6 characters.");

        var cleanEmail = request.Email.Trim().ToLowerInvariant();
        var cleanUsername = request.Username.Trim();

        if (await _userRepository.ExistsByEmailAsync(cleanEmail))
            throw new ConflictException("A user with this email already exists.");

        if (await _userRepository.ExistsByUsernameAsync(cleanUsername))
            throw new ConflictException("A user with this username already exists.");

        var user = new User
        {
            Username = cleanUsername,
            Email = cleanEmail,
            FullName = string.IsNullOrWhiteSpace(request.FullName) ? cleanUsername : request.FullName.Trim(),
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Role = request.Role,
            Interests = request.Interests?.Where(i => !string.IsNullOrWhiteSpace(i)).Select(i => i.Trim().ToLowerInvariant()).Distinct().ToList() ?? new List<string>(),
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);
        return MapToDto(user);
    }

    public async Task<UserDto> UpdateUserAsync(string id, UpdateUserRequest request)
    {
        EnsureAdmin();

        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new NotFoundException("User", id);

        var cleanEmail = request.Email.Trim().ToLowerInvariant();
        var cleanUsername = request.Username.Trim();

        if (await _userRepository.ExistsByEmailAsync(cleanEmail, id))
            throw new ConflictException("A user with this email already exists.");

        if (await _userRepository.ExistsByUsernameAsync(cleanUsername, id))
            throw new ConflictException("A user with this username already exists.");

        user.Username = cleanUsername;
        user.Email = cleanEmail;
        user.FullName = string.IsNullOrWhiteSpace(request.FullName) ? cleanUsername : request.FullName.Trim();
        user.Role = request.Role;
        user.Interests = request.Interests?.Where(i => !string.IsNullOrWhiteSpace(i)).Select(i => i.Trim().ToLowerInvariant()).Distinct().ToList() ?? new List<string>();
        user.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            if (request.Password.Length < 6)
                throw new BadRequestException("Password must be at least 6 characters.");
            user.PasswordHash = _passwordHasher.HashPassword(request.Password);
        }

        await _userRepository.UpdateAsync(user);
        return MapToDto(user);
    }

    public async Task DeleteUserAsync(string id)
    {
        EnsureAdmin();

        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new NotFoundException("User", id);

        // Prevent admin from deleting themselves
        if (_currentUserService.UserId == id)
            throw new BadRequestException("You cannot delete your own account.");

        await _userRepository.DeleteAsync(id);
    }

    private void EnsureAdmin()
    {
        if (!_currentUserService.IsAdmin)
            throw new ForbiddenException("Admin privilege required for user management.");
    }

    private static UserDto MapToDto(User user) =>
        new(user.Id, user.Username, user.Email, user.FullName, user.Role, user.Interests, user.CreatedAt);
}
