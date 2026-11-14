using MongoDB.Bson;
using MongoDB.Driver;
using SecureNoteApp.Application.Common.Interfaces;
using SecureNoteApp.Application.Common.Models;
using SecureNoteApp.Application.DTOs;
using SecureNoteApp.Domain.Entities;

namespace SecureNoteApp.Infrastructure.Persistence;

public class PostRepository : IPostRepository
{
    private readonly IMongoDbContext _context;

    public PostRepository(IMongoDbContext context)
    {
        _context = context;
    }

    public async Task<Post?> GetByIdAsync(string id)
    {
        return await _context.Posts.Find(p => p.Id == id).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Listing public posts feed paginated.
    /// Supported by index: { createdAt: -1 }
    /// </summary>
    public async Task<PagedResult<Post>> GetPagedAsync(PaginationParams pagination)
    {
        var filter = Builders<Post>.Filter.Empty;
        var totalCount = await _context.Posts.CountDocumentsAsync(filter);

        var items = await _context.Posts.Find(filter)
            .SortByDescending(p => p.CreatedAt)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Limit(pagination.PageSize)
            .ToListAsync();

        return new PagedResult<Post>(items, totalCount, pagination.PageNumber, pagination.PageSize);
    }

    public async Task CreateAsync(Post post)
    {
        await _context.Posts.InsertOneAsync(post);
    }

    /// <summary>
    /// Scenario 2: User Posts ($lookup)
    /// Task: Retrieve all posts belonging to a particular user.
    /// Constraint: Use a single aggregation pipeline with a $lookup stage.
    /// Supported by index on posts: { userId: 1, createdAt: -1 }
    /// </summary>
    public async Task<UserWithPostsDto?> GetUserPostsWithLookupAsync(string userId)
    {
        ObjectId parsedObjectId;
        bool isObjectId = ObjectId.TryParse(userId, out parsedObjectId);

        var matchFilter = isObjectId
            ? Builders<BsonDocument>.Filter.Eq("_id", parsedObjectId)
            : Builders<BsonDocument>.Filter.Eq("_id", userId);

        var lookupPipeline = new BsonDocument[]
        {
            new BsonDocument("$match", new BsonDocument("$expr",
                new BsonDocument("$or", new BsonArray
                {
                    new BsonDocument("$eq", new BsonArray { "$userId", "$$uIdStr" }),
                    new BsonDocument("$eq", new BsonArray { "$userId", "$$rawId" })
                })
            )),
            new BsonDocument("$sort", new BsonDocument("createdAt", -1))
        };

        var aggregate = _context.Database.GetCollection<BsonDocument>("users").Aggregate()
            .Match(matchFilter)
            .AppendStage<BsonDocument>(new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "posts" },
                { "let", new BsonDocument
                    {
                        { "uIdStr", new BsonDocument("$toString", "$_id") },
                        { "rawId", "$_id" }
                    }
                },
                { "pipeline", new BsonArray(lookupPipeline) },
                { "as", "posts" }
            }))
            .Project(new BsonDocument
            {
                { "userId", new BsonDocument("$toString", "$_id") },
                { "username", "$username" },
                { "email", "$email" },
                { "fullName", "$fullName" },
                { "posts", "$posts" }
            });

        var userDoc = await aggregate.FirstOrDefaultAsync();
        if (userDoc == null)
            return null;

        var postsList = new List<PostDto>();
        if (userDoc.Contains("posts") && userDoc["posts"].IsBsonArray)
        {
            foreach (var item in userDoc["posts"].AsBsonArray)
            {
                var doc = item.AsBsonDocument;
                var postId = doc.Contains("_id") ? doc["_id"].ToString() ?? string.Empty : string.Empty;
                var postUserId = doc.Contains("userId") ? doc["userId"].AsString : userId;
                var title = doc.Contains("title") ? doc["title"].AsString : string.Empty;
                var content = doc.Contains("content") ? doc["content"].AsString : string.Empty;
                var createdAt = doc.Contains("createdAt") && doc["createdAt"].IsValidDateTime
                    ? doc["createdAt"].ToUniversalTime()
                    : DateTime.UtcNow;
                DateTime? updatedAt = doc.Contains("updatedAt") && !doc["updatedAt"].IsBsonNull && doc["updatedAt"].IsValidDateTime
                    ? doc["updatedAt"].ToUniversalTime()
                    : null;

                postsList.Add(new PostDto(postId, postUserId, title, content, createdAt, updatedAt));
            }
        }

        return new UserWithPostsDto(
            userDoc["userId"].AsString,
            userDoc.Contains("username") ? userDoc["username"].AsString : string.Empty,
            userDoc.Contains("email") ? userDoc["email"].AsString : string.Empty,
            userDoc.Contains("fullName") ? userDoc["fullName"].AsString : string.Empty,
            postsList
        );
    }
}
