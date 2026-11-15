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


    [HttpGet]
    public async Task<ActionResult<PagedResult<PostDto>>> GetPosts([FromQuery] PaginationParams pagination)
    {
        var posts = await _postService.GetPostsPagedAsync(pagination);
        return Ok(posts);
    }


    [HttpPost]
    [Authorize]
    public async Task<ActionResult<PostDto>> CreatePost([FromBody] CreatePostRequest request)
    {
        var post = await _postService.CreatePostAsync(request);
        return Ok(post);
    }
}
