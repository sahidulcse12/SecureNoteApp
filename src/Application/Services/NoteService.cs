using SecureNoteApp.Application.Common.Exceptions;
using SecureNoteApp.Application.Common.Interfaces;
using SecureNoteApp.Application.Common.Models;
using SecureNoteApp.Application.DTOs;
using SecureNoteApp.Domain.Entities;

namespace SecureNoteApp.Application.Services;

public interface INoteService
{
    Task<NoteDto> CreateNoteAsync(CreateNoteRequest request);
    Task<NoteDto> GetNoteByIdAsync(string id);
    Task<PagedResult<NoteDto>> GetMyNotesAsync(PaginationParams pagination);
    Task<PagedResult<NoteDto>> GetAllNotesAsync(PaginationParams pagination);
    Task<NoteDto> UpdateNoteAsync(string id, UpdateNoteRequest request);
    Task DeleteNoteAsync(string id);
}

public class NoteService : INoteService
{
    private readonly INoteRepository _noteRepository;
    private readonly ICurrentUserService _currentUserService;

    public NoteService(INoteRepository noteRepository, ICurrentUserService currentUserService)
    {
        _noteRepository = noteRepository;
        _currentUserService = currentUserService;
    }

    public async Task<NoteDto> CreateNoteAsync(CreateNoteRequest request)
    {
        var currentUserId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(currentUserId))
            throw new UnauthorizedException();

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new BadRequestException("Note title cannot be empty.");

        var note = new Note
        {
            UserId = currentUserId,
            Title = request.Title.Trim(),
            Content = request.Content?.Trim() ?? string.Empty,
            Tags = request.Tags?.Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.Trim().ToLowerInvariant()).Distinct().ToList() ?? new List<string>(),
            CreatedAt = DateTime.UtcNow
        };

        await _noteRepository.CreateAsync(note);
        return MapToDto(note);
    }

    public async Task<NoteDto> GetNoteByIdAsync(string id)
    {
        var currentUserId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(currentUserId))
            throw new UnauthorizedException();

        var note = await _noteRepository.GetByIdAsync(id);
        if (note == null)
            throw new NotFoundException("Note", id);

        // RBAC: Admins can view any note; regular users can only view their own notes
        if (!_currentUserService.IsAdmin && note.UserId != currentUserId)
            throw new ForbiddenException("You do not have access to view this note.");

        return MapToDto(note);
    }

    public async Task<PagedResult<NoteDto>> GetMyNotesAsync(PaginationParams pagination)
    {
        var currentUserId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(currentUserId))
            throw new UnauthorizedException();

        var pagedNotes = await _noteRepository.GetPagedByUserAsync(currentUserId, pagination);
        var dtos = pagedNotes.Items.Select(MapToDto).ToList();

        return new PagedResult<NoteDto>(dtos, pagedNotes.TotalCount, pagedNotes.PageNumber, pagedNotes.PageSize);
    }

    public async Task<PagedResult<NoteDto>> GetAllNotesAsync(PaginationParams pagination)
    {
        // RBAC: Admin only capability to view everyone's notes
        if (!_currentUserService.IsAdmin)
            throw new ForbiddenException("Only admins can view all users' notes.");

        var pagedNotes = await _noteRepository.GetAllPagedAsync(pagination);
        var dtos = pagedNotes.Items.Select(MapToDto).ToList();

        return new PagedResult<NoteDto>(dtos, pagedNotes.TotalCount, pagedNotes.PageNumber, pagedNotes.PageSize);
    }

    public async Task<NoteDto> UpdateNoteAsync(string id, UpdateNoteRequest request)
    {
        var currentUserId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(currentUserId))
            throw new UnauthorizedException();

        var note = await _noteRepository.GetByIdAsync(id);
        if (note == null)
            throw new NotFoundException("Note", id);

        // Owner or Admin can update
        if (!_currentUserService.IsAdmin && note.UserId != currentUserId)
            throw new ForbiddenException("You can only edit your own notes.");

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new BadRequestException("Note title cannot be empty.");

        note.Title = request.Title.Trim();
        note.Content = request.Content?.Trim() ?? string.Empty;
        note.Tags = request.Tags?.Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.Trim().ToLowerInvariant()).Distinct().ToList() ?? new List<string>();
        note.UpdatedAt = DateTime.UtcNow;

        await _noteRepository.UpdateAsync(note);
        return MapToDto(note);
    }

    public async Task DeleteNoteAsync(string id)
    {
        var currentUserId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(currentUserId))
            throw new UnauthorizedException();

        var note = await _noteRepository.GetByIdAsync(id);
        if (note == null)
            throw new NotFoundException("Note", id);

        // Owner or Admin can delete
        if (!_currentUserService.IsAdmin && note.UserId != currentUserId)
            throw new ForbiddenException("You can only delete your own notes.");

        await _noteRepository.DeleteAsync(id);
    }

    private static NoteDto MapToDto(Note note) =>
        new(note.Id, note.UserId, note.Title, note.Content, note.Tags, note.CreatedAt, note.UpdatedAt);
}
