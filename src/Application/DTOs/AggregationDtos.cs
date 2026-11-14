namespace SecureNoteApp.Application.DTOs;

public record UserSummaryDto(
    string Id,
    string Username,
    string Email,
    string FullName
);

public record InterestsGroupDto(
    string Interest,
    int UserCount,
    List<UserSummaryDto> Users
);

public record UserWithPostsDto(
    string UserId,
    string Username,
    string Email,
    string FullName,
    List<PostDto> Posts
);
