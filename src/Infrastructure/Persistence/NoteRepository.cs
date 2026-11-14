using MongoDB.Driver;
using SecureNoteApp.Application.Common.Interfaces;
using SecureNoteApp.Application.Common.Models;
using SecureNoteApp.Domain.Entities;

namespace SecureNoteApp.Infrastructure.Persistence;

public class NoteRepository : INoteRepository
{
    private readonly IMongoDbContext _context;

    public NoteRepository(IMongoDbContext context)
    {
        _context = context;
    }

    public async Task<Note?> GetByIdAsync(string id)
    {
        return await _context.Notes.Find(n => n.Id == id).FirstOrDefaultAsync();
    }

    /// <summary>
    /// User listing their notes paginated.
    /// Supported by compound index: { userId: 1, createdAt: -1 }
    /// </summary>
    public async Task<PagedResult<Note>> GetPagedByUserAsync(string userId, PaginationParams pagination)
    {
        var filter = Builders<Note>.Filter.Eq(n => n.UserId, userId);
        var totalCount = await _context.Notes.CountDocumentsAsync(filter);

        var items = await _context.Notes.Find(filter)
            .SortByDescending(n => n.CreatedAt)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Limit(pagination.PageSize)
            .ToListAsync();

        return new PagedResult<Note>(items, totalCount, pagination.PageNumber, pagination.PageSize);
    }

    /// <summary>
    /// Admin listing all notes across all users paginated.
    /// Supported by index: { createdAt: -1 }
    /// </summary>
    public async Task<PagedResult<Note>> GetAllPagedAsync(PaginationParams pagination)
    {
        var filter = Builders<Note>.Filter.Empty;
        var totalCount = await _context.Notes.CountDocumentsAsync(filter);

        var items = await _context.Notes.Find(filter)
            .SortByDescending(n => n.CreatedAt)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Limit(pagination.PageSize)
            .ToListAsync();

        return new PagedResult<Note>(items, totalCount, pagination.PageNumber, pagination.PageSize);
    }

    public async Task CreateAsync(Note note)
    {
        await _context.Notes.InsertOneAsync(note);
    }

    public async Task UpdateAsync(Note note)
    {
        await _context.Notes.ReplaceOneAsync(n => n.Id == note.Id, note);
    }

    public async Task DeleteAsync(string id)
    {
        await _context.Notes.DeleteOneAsync(n => n.Id == id);
    }
}
