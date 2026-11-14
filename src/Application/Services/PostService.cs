using SecureNoteApp.Application.Common.Exceptions;
using SecureNoteApp.Application.Common.Interfaces;
using SecureNoteApp.Application.Common.Models;
using SecureNoteApp.Application.DTOs;
using SecureNoteApp.Domain.Entities;

namespace SecureNoteApp.Application.Services;

public interface IPostService
{
    Task<PostDto> CreatePostAsync(CreatePostRequest request);
    Task<PagedResult<PostDto>> GetPostsPagedAsync(PaginationParams pagination);
}

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly ICurrentUserService _currentUserService;

    public PostService(IPostRepository postRepository, ICurrentUserService currentUserService)
    {
        _postRepository = postRepository;
        _currentUserService = currentUserService;
    }

    public async Task<PostDto> CreatePostAsync(CreatePostRequest request)
    {
        var currentUserId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(currentUserId))
            throw new UnauthorizedException();

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new BadRequestException("Post title cannot be empty.");

        var post = new Post
        {
            UserId = currentUserId,
            Title = request.Title.Trim(),
            Content = request.Content?.Trim() ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        await _postRepository.CreateAsync(post);
        return new PostDto(post.Id, post.UserId, post.Title, post.Content, post.CreatedAt, post.UpdatedAt);
    }

    public async Task<PagedResult<PostDto>> GetPostsPagedAsync(PaginationParams pagination)
    {
        var pagedPosts = await _postRepository.GetPagedAsync(pagination);
        var dtos = pagedPosts.Items
            .Select(p => new PostDto(p.Id, p.UserId, p.Title, p.Content, p.CreatedAt, p.UpdatedAt))
            .ToList();

        return new PagedResult<PostDto>(dtos, pagedPosts.TotalCount, pagedPosts.PageNumber, pagedPosts.PageSize);
    }
}
