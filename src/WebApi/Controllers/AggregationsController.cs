using Microsoft.AspNetCore.Mvc;
using SecureNoteApp.Application.DTOs;
using SecureNoteApp.Application.Services;

namespace SecureNoteApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AggregationsController : ControllerBase
{
    private readonly IAggregationService _aggregationService;

    public AggregationsController(IAggregationService aggregationService)
    {
        _aggregationService = aggregationService;
    }


    [HttpGet("users-by-interests")]
    public async Task<ActionResult<List<InterestsGroupDto>>> GetUsersByInterests()
    {
        var result = await _aggregationService.GetUsersGroupedByInterestsAsync();
        return Ok(result);
    }


    [HttpGet("users/{userId}/posts")]
    public async Task<ActionResult<UserWithPostsDto>> GetUserPosts(string userId)
    {
        var result = await _aggregationService.GetUserPostsLookupAsync(userId);
        return Ok(result);
    }


    [HttpGet("indexes-info")]
    public IActionResult GetIndexesInfo()
    {
        var indexingStrategy = new
        {
            strategy = "Strictly Required Indexes Only (Zero Unnecessary Indexes)",
            collections = new[]
            {
                new
                {
                    collection = "users",
                    indexes = new[]
                    {
                        new { name = "idx_users_email_unique", keys = "{ email: 1 }", unique = true, purpose = "Read Operation & Auth: Unique lookup by user email" },
                        new { name = "idx_users_username_unique", keys = "{ username: 1 }", unique = true, purpose = "Read Operation & Auth: Unique lookup by username" },
                        new { name = "idx_users_createdAt_desc", keys = "{ createdAt: -1 }", unique = false, purpose = "List Operation: Admin listing all users paginated (sorted newest first)" },
                        new { name = "idx_users_interests_multikey", keys = "{ interests: 1 }", unique = false, purpose = "Scenario 1 Aggregation: Multikey index for $unwind and $group by interests" }
                    }
                },
                new
                {
                    collection = "notes",
                    indexes = new[]
                    {
                        new { name = "idx_notes_userId_createdAt", keys = "{ userId: 1, createdAt: -1 }", unique = false, purpose = "List Operation: User listing their own notes paginated (filtered by userId, sorted newest first)" },
                        new { name = "idx_notes_createdAt_desc", keys = "{ createdAt: -1 }", unique = false, purpose = "List Operation: Admin listing all notes across all users paginated" }
                    }
                },
                new
                {
                    collection = "posts",
                    indexes = new[]
                    {
                        new { name = "idx_posts_userId_createdAt", keys = "{ userId: 1, createdAt: -1 }", unique = false, purpose = "Scenario 2 Aggregation: $lookup stage joining posts by userId and sorting newest first" },
                        new { name = "idx_posts_createdAt_desc", keys = "{ createdAt: -1 }", unique = false, purpose = "List Operation: Public feed of all posts paginated (sorted newest first)" }
                    }
                }
            }
        };

        return Ok(indexingStrategy);
    }
}
