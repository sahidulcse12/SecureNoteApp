using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureNoteApp.Application.Common.Models;
using SecureNoteApp.Application.DTOs;
using SecureNoteApp.Application.Services;

namespace SecureNoteApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }


    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResult<UserDto>>> GetUsers([FromQuery] PaginationParams pagination)
    {
        var users = await _userService.GetUsersPagedAsync(pagination);
        return Ok(users);
    }


    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUserById(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        return Ok(user);
    }


    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserRequest request)
    {
        var user = await _userService.CreateUserAsync(request);
        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
    }


    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserDto>> UpdateUser(string id, [FromBody] UpdateUserRequest request)
    {
        var user = await _userService.UpdateUserAsync(id, request);
        return Ok(user);
    }


    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        await _userService.DeleteUserAsync(id);
        return NoContent();
    }
}
