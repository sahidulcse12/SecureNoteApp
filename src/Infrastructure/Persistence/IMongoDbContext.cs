using MongoDB.Driver;
using SecureNoteApp.Domain.Entities;

namespace SecureNoteApp.Infrastructure.Persistence;

public interface IMongoDbContext
{
    IMongoDatabase Database { get; }
    IMongoCollection<User> Users { get; }
    IMongoCollection<Note> Notes { get; }
    IMongoCollection<Post> Posts { get; }
}
