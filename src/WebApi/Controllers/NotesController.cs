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


    [HttpGet]
    public async Task<ActionResult<PagedResult<NoteDto>>> GetMyNotes([FromQuery] PaginationParams pagination)
    {
        var notes = await _noteService.GetMyNotesAsync(pagination);
        return Ok(notes);
    }


    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResult<NoteDto>>> GetAllNotes([FromQuery] PaginationParams pagination)
    {
        var notes = await _noteService.GetAllNotesAsync(pagination);
        return Ok(notes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<NoteDto>> GetNoteById(string id)
    {
        var note = await _noteService.GetNoteByIdAsync(id);
        return Ok(note);
    }


    [HttpPost]
    public async Task<ActionResult<NoteDto>> CreateNote([FromBody] CreateNoteRequest request)
    {
        var note = await _noteService.CreateNoteAsync(request);
        return CreatedAtAction(nameof(GetNoteById), new { id = note.Id }, note);
    }


    [HttpPut("{id}")]
    public async Task<ActionResult<NoteDto>> UpdateNote(string id, [FromBody] UpdateNoteRequest request)
    {
        var note = await _noteService.UpdateNoteAsync(id, request);
        return Ok(note);
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNote(string id)
    {
        await _noteService.DeleteNoteAsync(id);
        return NoContent();
    }
}
