using SecureNoteApp.Application.Common.Exceptions;
using SecureNoteApp.Application.Common.Interfaces;
using SecureNoteApp.Application.DTOs;

namespace SecureNoteApp.Application.Services;

public interface IAggregationService
{
    // Scenario 1: Group by Interests
    Task<List<InterestsGroupDto>> GetUsersGroupedByInterestsAsync();

    // Scenario 2: User Posts with $lookup
    Task<UserWithPostsDto> GetUserPostsLookupAsync(string userId);
}

public class AggregationService : IAggregationService
{
    private readonly IUserRepository _userRepository;
    private readonly IPostRepository _postRepository;

    public AggregationService(IUserRepository userRepository, IPostRepository postRepository)
    {
        _userRepository = userRepository;
        _postRepository = postRepository;
    }

    public async Task<List<InterestsGroupDto>> GetUsersGroupedByInterestsAsync()
    {
        return await _userRepository.GetUsersGroupedByInterestsAsync();
    }

    public async Task<UserWithPostsDto> GetUserPostsLookupAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new BadRequestException("User ID is required.");

        var result = await _postRepository.GetUserPostsWithLookupAsync(userId);
        if (result == null)
            throw new NotFoundException("User", userId);

        return result;
    }
}
