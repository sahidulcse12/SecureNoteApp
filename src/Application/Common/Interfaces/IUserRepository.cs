using SecureNoteApp.Application.Common.Models;
using SecureNoteApp.Application.DTOs;
using SecureNoteApp.Domain.Entities;

namespace SecureNoteApp.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<PagedResult<User>> GetPagedAsync(PaginationParams pagination);
    Task CreateAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(string id);
    Task<bool> ExistsByEmailAsync(string email, string? excludeUserId = null);
    Task<bool> ExistsByUsernameAsync(string username, string? excludeUserId = null);
    
    // Scenario 1: Group by Interests (using single collection.aggregate() call)
    Task<List<InterestsGroupDto>> GetUsersGroupedByInterestsAsync();
}
