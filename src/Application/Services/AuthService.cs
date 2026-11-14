using SecureNoteApp.Application.Common.Exceptions;
using SecureNoteApp.Application.Common.Interfaces;
using SecureNoteApp.Application.DTOs;
using SecureNoteApp.Domain.Entities;
using SecureNoteApp.Domain.Enums;

namespace SecureNoteApp.Application.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<UserDto> GetCurrentUserProfileAsync();
}

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ICurrentUserService _currentUserService;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _currentUserService = currentUserService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
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
            Role = request.Role ?? UserRole.User,
            Interests = request.Interests?.Where(i => !string.IsNullOrWhiteSpace(i)).Select(i => i.Trim().ToLowerInvariant()).Distinct().ToList() ?? new List<string>()
        };

        await _userRepository.CreateAsync(user);

        var token = _jwtTokenGenerator.GenerateToken(user);
        var userDto = new UserDto(user.Id, user.Username, user.Email, user.FullName, user.Role, user.Interests, user.CreatedAt);

        return new AuthResponse(token, userDto);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.EmailOrUsername) || string.IsNullOrWhiteSpace(request.Password))
            throw new BadRequestException("Email/Username and password are required.");

        var cleanInput = request.EmailOrUsername.Trim();
        var user = await _userRepository.GetByEmailAsync(cleanInput.ToLowerInvariant())
                   ?? await _userRepository.GetByUsernameAsync(cleanInput);

        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid credentials.");

        var token = _jwtTokenGenerator.GenerateToken(user);
        var userDto = new UserDto(user.Id, user.Username, user.Email, user.FullName, user.Role, user.Interests, user.CreatedAt);

        return new AuthResponse(token, userDto);
    }

    public async Task<UserDto> GetCurrentUserProfileAsync()
    {
        var currentUserId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(currentUserId))
            throw new UnauthorizedException();

        var user = await _userRepository.GetByIdAsync(currentUserId);
        if (user == null)
            throw new NotFoundException("User not found.");

        return new UserDto(user.Id, user.Username, user.Email, user.FullName, user.Role, user.Interests, user.CreatedAt);
    }
}
