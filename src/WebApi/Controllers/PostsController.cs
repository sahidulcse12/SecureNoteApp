using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureNoteApp.Application.Common.Models;
using SecureNoteApp.Application.DTOs;
using SecureNoteApp.Application.Services;

namespace SecureNoteApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService)
    {
        _postService = postService;
    }

    /// <summary>
    /// Feed of public posts visible to everyone, with pagination.
    /// Supported by index: { createdAt: -1 }
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<PostDto>>> GetPosts([FromQuery] PaginationParams pagination)
    {
        var posts = await _postService.GetPostsPagedAsync(pagination);
        return Ok(posts);
    }

    /// <summary>
    /// Authenticated user: Create a new post.
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<PostDto>> CreatePost([FromBody] CreatePostRequest request)
    {
        var post = await _postService.CreatePostAsync(request);
        return Ok(post);
    }
}
