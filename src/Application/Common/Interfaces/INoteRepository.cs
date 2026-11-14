using SecureNoteApp.Application.Common.Models;
using SecureNoteApp.Domain.Entities;

namespace SecureNoteApp.Application.Common.Interfaces;

public interface INoteRepository
{
    Task<Note?> GetByIdAsync(string id);
    Task<PagedResult<Note>> GetPagedByUserAsync(string userId, PaginationParams pagination);
    Task<PagedResult<Note>> GetAllPagedAsync(PaginationParams pagination);
    Task CreateAsync(Note note);
    Task UpdateAsync(Note note);
    Task DeleteAsync(string id);
}
