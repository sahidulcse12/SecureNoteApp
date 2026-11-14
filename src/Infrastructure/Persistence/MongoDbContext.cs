using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using SecureNoteApp.Domain.Entities;
using SecureNoteApp.Infrastructure.Configuration;

namespace SecureNoteApp.Infrastructure.Persistence;

public class MongoDbContext : IMongoDbContext
{
    private readonly IMongoDatabase _database;

    static MongoDbContext()
    {
        // Register class mappings for BaseEntity and its derived types
        if (!BsonClassMap.IsClassMapRegistered(typeof(BaseEntity)))
        {
            BsonClassMap.RegisterClassMap<BaseEntity>(cm =>
            {
                cm.AutoMap();
                cm.MapIdProperty(c => c.Id)
                  .SetIdGenerator(StringObjectIdGenerator.Instance)
                  .SetSerializer(new StringSerializer(BsonType.ObjectId));
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(User)))
        {
            BsonClassMap.RegisterClassMap<User>(cm =>
            {
                cm.AutoMap();
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(Note)))
        {
            BsonClassMap.RegisterClassMap<Note>(cm =>
            {
                cm.AutoMap();
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(Post)))
        {
            BsonClassMap.RegisterClassMap<Post>(cm =>
            {
                cm.AutoMap();
            });
        }
    }

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }

    public IMongoDatabase Database => _database;
    public IMongoCollection<User> Users => _database.GetCollection<User>("users");
    public IMongoCollection<Note> Notes => _database.GetCollection<Note>("notes");
    public IMongoCollection<Post> Posts => _database.GetCollection<Post>("posts");
}
