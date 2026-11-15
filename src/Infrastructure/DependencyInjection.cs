using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SecureNoteApp.Application.Common.Interfaces;
using SecureNoteApp.Infrastructure.Configuration;
using SecureNoteApp.Infrastructure.Persistence;
using SecureNoteApp.Infrastructure.Security;

namespace SecureNoteApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // MongoDB Settings & Context
        services.Configure<MongoDbSettings>(options =>
        {
            configuration.GetSection(MongoDbSettings.SectionName).Bind(options);

            var envConn = Environment.GetEnvironmentVariable("MONGODB_URI")
                       ?? Environment.GetEnvironmentVariable("MONGO_URL")
                       ?? Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");
            if (!string.IsNullOrWhiteSpace(envConn))
            {
                options.ConnectionString = envConn;
            }

            var envDb = Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME")
                     ?? Environment.GetEnvironmentVariable("MONGO_DATABASE");
            if (!string.IsNullOrWhiteSpace(envDb))
            {
                options.DatabaseName = envDb;
            }
        });

        services.AddSingleton<IMongoDbContext, MongoDbContext>();
        services.AddSingleton<MongoIndexConfigurator>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<INoteRepository, NoteRepository>();
        services.AddScoped<IPostRepository, PostRepository>();

        // Security
        services.Configure<JwtSettings>(options =>
        {
            configuration.GetSection(JwtSettings.SectionName).Bind(options);

            var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
                         ?? Environment.GetEnvironmentVariable("JWT_SECRET");
            if (!string.IsNullOrWhiteSpace(jwtSecret))
            {
                options.SecretKey = jwtSecret;
            }
        });
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}
