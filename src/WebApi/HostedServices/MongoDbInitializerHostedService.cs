using MongoDB.Driver;
using SecureNoteApp.Application.Common.Interfaces;
using SecureNoteApp.Domain.Entities;
using SecureNoteApp.Domain.Enums;
using SecureNoteApp.Infrastructure.Persistence;

namespace SecureNoteApp.WebApi.HostedServices;

public class MongoDbInitializerHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MongoDbInitializerHostedService> _logger;

    public MongoDbInitializerHostedService(
        IServiceProvider serviceProvider,
        ILogger<MongoDbInitializerHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IMongoDbContext>();
            var indexConfigurator = scope.ServiceProvider.GetRequiredService<MongoIndexConfigurator>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            _logger.LogInformation("Initializing database indexes...");
            await indexConfigurator.InitializeIndexesAsync(cancellationToken);

            _logger.LogInformation("Checking database seed data...");
            await SeedInitialDataAsync(context, passwordHasher, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during database initialization or index configuration: {Message}", ex.Message);
        }
    }

    private async Task SeedInitialDataAsync(
        IMongoDbContext context,
        IPasswordHasher passwordHasher,
        CancellationToken cancellationToken)
    {
        var userCount = await context.Users.CountDocumentsAsync(Builders<User>.Filter.Empty, cancellationToken: cancellationToken);
        if (userCount > 0)
        {
            _logger.LogInformation("Database already contains {Count} users. Skipping seeding.", userCount);
            return;
        }

        _logger.LogInformation("Seeding initial users, notes, and posts...");

        // 1. Admin User
        var adminUser = new User
        {
            Username = "admin",
            Email = "admin@careguide.bd",
            FullName = "System Administrator",
            Role = UserRole.Admin,
            PasswordHash = passwordHasher.HashPassword("Admin123!"),
            Interests = new List<string> { "management", "security", "technology" },
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };
        await context.Users.InsertOneAsync(adminUser, cancellationToken: cancellationToken);

        // 2. Regular User: Alice (Interests: chess, reading, coding)
        var alice = new User
        {
            Username = "alice",
            Email = "alice@careguide.bd",
            FullName = "Alice Johnson",
            Role = UserRole.User,
            PasswordHash = passwordHasher.HashPassword("User123!"),
            Interests = new List<string> { "chess", "reading", "coding" },
            CreatedAt = DateTime.UtcNow.AddDays(-8)
        };
        await context.Users.InsertOneAsync(alice, cancellationToken: cancellationToken);

        // 3. Regular User: Bob (Interests: chess, photography, travel)
        var bob = new User
        {
            Username = "bob",
            Email = "bob@careguide.bd",
            FullName = "Bob Smith",
            Role = UserRole.User,
            PasswordHash = passwordHasher.HashPassword("User123!"),
            Interests = new List<string> { "chess", "photography", "travel" },
            CreatedAt = DateTime.UtcNow.AddDays(-6)
        };
        await context.Users.InsertOneAsync(bob, cancellationToken: cancellationToken);

        // 4. Regular User: Charlie (Interests: reading, music, travel)
        var charlie = new User
        {
            Username = "charlie",
            Email = "charlie@careguide.bd",
            FullName = "Charlie Davis",
            Role = UserRole.User,
            PasswordHash = passwordHasher.HashPassword("User123!"),
            Interests = new List<string> { "reading", "music", "travel" },
            CreatedAt = DateTime.UtcNow.AddDays(-4)
        };
        await context.Users.InsertOneAsync(charlie, cancellationToken: cancellationToken);

        // Seed Notes for each user
        var notes = new List<Note>
        {
            new()
            {
                UserId = adminUser.Id,
                Title = "System Maintenance Protocol",
                Content = "Weekly MongoDB index inspection and security audit protocols.",
                Tags = new List<string> { "admin", "devops", "security" },
                CreatedAt = DateTime.UtcNow.AddDays(-9)
            },
            new()
            {
                UserId = alice.Id,
                Title = "Clean Architecture In .NET",
                Content = "Domain, Application, Infrastructure, WebApi separation makes testing and scaling effortless.",
                Tags = new List<string> { "dotnet", "architecture", "coding" },
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            },
            new()
            {
                UserId = alice.Id,
                Title = "Sicilian Defense Opening Notes",
                Content = "Key variations in Sicilian Defense for club chess tournament next weekend.",
                Tags = new List<string> { "chess", "strategy" },
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new()
            {
                UserId = bob.Id,
                Title = "Landscape Photography Essentials",
                Content = "Golden hour light, wide angle lens, f/8 to f/11 aperture settings.",
                Tags = new List<string> { "photography", "tips" },
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new()
            {
                UserId = charlie.Id,
                Title = "Book Recommendations: Sci-Fi",
                Content = "Dune, Foundation, Project Hail Mary - must read list.",
                Tags = new List<string> { "books", "reading" },
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            }
        };
        await context.Notes.InsertManyAsync(notes, cancellationToken: cancellationToken);

        // Seed Public Posts for users (for Scenario 2 User Posts with $lookup)
        var posts = new List<Post>
        {
            new()
            {
                UserId = alice.Id,
                Title = "My First Tournament Win!",
                Content = "Excited to share that I won the regional blitz chess championship yesterday!",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new()
            {
                UserId = alice.Id,
                Title = "Why Clean Architecture Matters",
                Content = "Loose coupling and independent business rules make long-term maintenance manageable.",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                UserId = bob.Id,
                Title = "Sunset at Cox's Bazar",
                Content = "Captured an incredible long exposure sunset over the Bay of Bengal.",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new()
            {
                UserId = charlie.Id,
                Title = "Vinyl Record Collection Progress",
                Content = "Added Pink Floyd's The Dark Side of the Moon original pressing today.",
                CreatedAt = DateTime.UtcNow.AddHours(-12)
            }
        };
        await context.Posts.InsertManyAsync(posts, cancellationToken: cancellationToken);

        _logger.LogInformation("Database seeding complete.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
