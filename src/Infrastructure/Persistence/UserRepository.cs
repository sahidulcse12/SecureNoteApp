using MongoDB.Bson;
using MongoDB.Driver;
using SecureNoteApp.Application.Common.Interfaces;
using SecureNoteApp.Application.Common.Models;
using SecureNoteApp.Application.DTOs;
using SecureNoteApp.Domain.Entities;

namespace SecureNoteApp.Infrastructure.Persistence;

public class UserRepository : IUserRepository
{
    private readonly IMongoDbContext _context;

    public UserRepository(IMongoDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        return await _context.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.Find(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users.Find(u => u.Username == username).FirstOrDefaultAsync();
    }

    public async Task<PagedResult<User>> GetPagedAsync(PaginationParams pagination)
    {
        var filter = Builders<User>.Filter.Empty;
        var totalCount = await _context.Users.CountDocumentsAsync(filter);

        var items = await _context.Users.Find(filter)
            .SortByDescending(u => u.CreatedAt)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Limit(pagination.PageSize)
            .ToListAsync();

        return new PagedResult<User>(items, totalCount, pagination.PageNumber, pagination.PageSize);
    }

    public async Task CreateAsync(User user)
    {
        await _context.Users.InsertOneAsync(user);
    }

    public async Task UpdateAsync(User user)
    {
        await _context.Users.ReplaceOneAsync(u => u.Id == user.Id, user);
    }

    public async Task DeleteAsync(string id)
    {
        await _context.Users.DeleteOneAsync(u => u.Id == id);
    }

    public async Task<bool> ExistsByEmailAsync(string email, string? excludeUserId = null)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Email, email);
        if (!string.IsNullOrEmpty(excludeUserId))
        {
            filter &= Builders<User>.Filter.Ne(u => u.Id, excludeUserId);
        }

        return await _context.Users.Find(filter).AnyAsync();
    }

    public async Task<bool> ExistsByUsernameAsync(string username, string? excludeUserId = null)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Username, username);
        if (!string.IsNullOrEmpty(excludeUserId))
        {
            filter &= Builders<User>.Filter.Ne(u => u.Id, excludeUserId);
        }

        return await _context.Users.Find(filter).AnyAsync();
    }

    /// <summary>
    /// Scenario 1: Group by Interests
    /// Constraint: You must use exactly one collection.aggregate() call. Do not use any other methods.
    /// Uses multikey index on "interests".
    /// </summary>
    public async Task<List<InterestsGroupDto>> GetUsersGroupedByInterestsAsync()
    {
        var aggregate = _context.Users.Aggregate()
            .Unwind("interests")
            .Group(new BsonDocument
            {
                { "_id", "$interests" },
                { "userCount", new BsonDocument("$sum", 1) },
                { "users", new BsonDocument("$push", new BsonDocument
                    {
                        { "id", new BsonDocument("$toString", "$_id") },
                        { "username", "$username" },
                        { "email", "$email" },
                        { "fullName", "$fullName" }
                    })
                }
            })
            .Sort(new BsonDocument("_id", 1));

        var bsonResults = await aggregate.ToListAsync();

        var result = bsonResults.Select(doc =>
        {
            var interest = doc["_id"].AsString;
            var userCount = doc["userCount"].AsInt32;
            var usersList = doc["users"].AsBsonArray.Select(u => new UserSummaryDto(
                u["id"].AsString,
                u["username"].AsString,
                u["email"].AsString,
                u.AsBsonDocument.Contains("fullName") ? u["fullName"].AsString : string.Empty
            )).ToList();

            return new InterestsGroupDto(interest, userCount, usersList);
        }).ToList();

        return result;
    }
}
