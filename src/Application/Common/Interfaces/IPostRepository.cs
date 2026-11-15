using SecureNoteApp.Application.Common.Models;
using SecureNoteApp.Application.DTOs;
using SecureNoteApp.Domain.Entities;

namespace SecureNoteApp.Application.Common.Interfaces;

public interface IPostRepository
{
    Task<Post?> GetByIdAsync(string id);
    Task<PagedResult<Post>> GetPagedAsync(PaginationParams pagination);
    Task CreateAsync(Post post);

    Task<UserWithPostsDto?> GetUserPostsWithLookupAsync(string userId);
}
