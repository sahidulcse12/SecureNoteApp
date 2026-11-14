using SecureNoteApp.Domain.Entities;

namespace SecureNoteApp.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
