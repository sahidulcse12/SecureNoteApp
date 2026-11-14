namespace SecureNoteApp.Application.DTOs;

public record PostDto(
    string Id,
    string UserId,
    string Title,
    string Content,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreatePostRequest(
    string Title,
    string Content
);
