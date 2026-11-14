namespace SecureNoteApp.Application.DTOs;

public record NoteDto(
    string Id,
    string UserId,
    string Title,
    string Content,
    List<string> Tags,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateNoteRequest(
    string Title,
    string Content,
    List<string>? Tags
);

public record UpdateNoteRequest(
    string Title,
    string Content,
    List<string>? Tags
);
