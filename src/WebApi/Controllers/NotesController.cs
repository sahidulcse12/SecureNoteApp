using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureNoteApp.Application.Common.Models;
using SecureNoteApp.Application.DTOs;
using SecureNoteApp.Application.Services;

namespace SecureNoteApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotesController : ControllerBase
{
    private readonly INoteService _noteService;

    public NotesController(INoteService noteService)
    {
        _noteService = noteService;
    }

    /// <summary>
    /// User: View a paginated list of their own notes.
    /// Supported by compound index: { userId: 1, createdAt: -1 }
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<NoteDto>>> GetMyNotes([FromQuery] PaginationParams pagination)
    {
        var notes = await _noteService.GetMyNotesAsync(pagination);
        return Ok(notes);
    }

    /// <summary>
    /// Admin: View everyone's notes with pagination.
    /// Supported by index: { createdAt: -1 }
    /// </summary>
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResult<NoteDto>>> GetAllNotes([FromQuery] PaginationParams pagination)
    {
        var notes = await _noteService.GetAllNotesAsync(pagination);
        return Ok(notes);
    }

    /// <summary>
    /// Read Operation: Get a specific note by ID.
    /// User can view own note; Admin can view any note.
    /// Supported by default _id index.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<NoteDto>> GetNoteById(string id)
    {
        var note = await _noteService.GetNoteByIdAsync(id);
        return Ok(note);
    }

    /// <summary>
    /// User: Create a new note.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<NoteDto>> CreateNote([FromBody] CreateNoteRequest request)
    {
        var note = await _noteService.CreateNoteAsync(request);
        return CreatedAtAction(nameof(GetNoteById), new { id = note.Id }, note);
    }

    /// <summary>
    /// User: Update their own note. Admin can also update.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<NoteDto>> UpdateNote(string id, [FromBody] UpdateNoteRequest request)
    {
        var note = await _noteService.UpdateNoteAsync(id, request);
        return Ok(note);
    }

    /// <summary>
    /// User: Delete their own note. Admin can also delete.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNote(string id)
    {
        await _noteService.DeleteNoteAsync(id);
        return NoContent();
    }
}
