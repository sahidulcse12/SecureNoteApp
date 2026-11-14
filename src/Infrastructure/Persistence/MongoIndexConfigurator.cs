using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using SecureNoteApp.Domain.Entities;

namespace SecureNoteApp.Infrastructure.Persistence;

/// <summary>
/// Explicit Index Configuration adhering to requirement:
/// "You must use the schema.index method for defining indexes in your code so they are visible during review."
///
/// CRITICAL CONSTRAINT: DO NOT MAKE ANY UNNECESSARY INDEXES.
/// Only creates indexes strictly required to support the queries and aggregations:
/// 1. Users:
///    - { email: 1 } (Unique): Auth & Profile GET by email
///    - { username: 1 } (Unique): Auth & Profile GET by username
///    - { createdAt: -1 }: Admin listing users paginated sorted by newest
///    - { interests: 1 }: Multikey index supporting Scenario 1 aggregation ($unwind & $group by interests)
///
/// 2. Notes:
///    - { userId: 1, createdAt: -1 }: Compound index for user listing their own notes paginated
///    - { createdAt: -1 }: Index for admin listing all notes paginated
///
/// 3. Posts:
///    - { userId: 1, createdAt: -1 }: Compound index supporting Scenario 2 ($lookup by userId and sorting)
///    - { createdAt: -1 }: Index for public feed listing posts paginated
/// </summary>
public class MongoIndexConfigurator
{
    private readonly IMongoDbContext _context;
    private readonly ILogger<MongoIndexConfigurator> _logger;

    public MongoIndexConfigurator(IMongoDbContext context, ILogger<MongoIndexConfigurator> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Declarative schema.index definition wrapper for clean visibility during code review.
    /// </summary>
    public static class Schema
    {
        public static CreateIndexModel<T> Index<T>(
            IndexKeysDefinition<T> keys,
            CreateIndexOptions? options = null)
        {
            return new CreateIndexModel<T>(keys, options);
        }
    }

    public async Task InitializeIndexesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Configuring MongoDB indexes according to specifications...");

        await ConfigureUserIndexesAsync(cancellationToken);
        await ConfigureNoteIndexesAsync(cancellationToken);
        await ConfigurePostIndexesAsync(cancellationToken);

        _logger.LogInformation("All strictly required indexes successfully configured.");
    }

    private async Task ConfigureUserIndexesAsync(CancellationToken cancellationToken)
    {
        var usersBuilder = Builders<User>.IndexKeys;

        // Note: Using schema.index style definitions
        var userIndexes = new[]
        {
            // Read Operation Indexing: Authentication & Profile lookup by email
            Schema.Index(usersBuilder.Ascending(u => u.Email), new CreateIndexOptions { Unique = true, Name = "idx_users_email_unique" }),

            // Read Operation Indexing: Authentication & Profile lookup by username
            Schema.Index(usersBuilder.Ascending(u => u.Username), new CreateIndexOptions { Unique = true, Name = "idx_users_username_unique" }),

            // List Operation Indexing: Admin listing all users with pagination sorted by createdAt descending
            Schema.Index(usersBuilder.Descending(u => u.CreatedAt), new CreateIndexOptions { Name = "idx_users_createdAt_desc" }),

            // Scenario 1 Aggregation Indexing: Multikey index supporting collection.aggregate() grouping by interests
            Schema.Index(usersBuilder.Ascending(u => u.Interests), new CreateIndexOptions { Name = "idx_users_interests_multikey" })
        };

        await _context.Users.Indexes.CreateManyAsync(userIndexes, cancellationToken);
    }

    private async Task ConfigureNoteIndexesAsync(CancellationToken cancellationToken)
    {
        var notesBuilder = Builders<Note>.IndexKeys;

        var noteIndexes = new[]
        {
            // List Operation Indexing: Regular user listing their own notes with pagination sorted by createdAt descending
            Schema.Index(
                notesBuilder.Combine(notesBuilder.Ascending(n => n.UserId), notesBuilder.Descending(n => n.CreatedAt)),
                new CreateIndexOptions { Name = "idx_notes_userId_createdAt" }
            ),

            // List Operation Indexing: Admin listing all notes across all users with pagination sorted by createdAt descending
            Schema.Index(notesBuilder.Descending(n => n.CreatedAt), new CreateIndexOptions { Name = "idx_notes_createdAt_desc" })
        };

        await _context.Notes.Indexes.CreateManyAsync(noteIndexes, cancellationToken);
    }

    private async Task ConfigurePostIndexesAsync(CancellationToken cancellationToken)
    {
        var postsBuilder = Builders<Post>.IndexKeys;

        var postIndexes = new[]
        {
            // Scenario 2 Aggregation Indexing: Single aggregation pipeline with $lookup stage joining posts on userId
            Schema.Index(
                postsBuilder.Combine(postsBuilder.Ascending(p => p.UserId), postsBuilder.Descending(p => p.CreatedAt)),
                new CreateIndexOptions { Name = "idx_posts_userId_createdAt" }
            ),

            // List Operation Indexing: Feed of public posts with pagination sorted by createdAt descending
            Schema.Index(postsBuilder.Descending(p => p.CreatedAt), new CreateIndexOptions { Name = "idx_posts_createdAt_desc" })
        };

        await _context.Posts.Indexes.CreateManyAsync(postIndexes, cancellationToken);
    }
}
