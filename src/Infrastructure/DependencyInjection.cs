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
        services.Configure<MongoDbSettings>(
            configuration.GetSection(MongoDbSettings.SectionName));

        services.AddSingleton<IMongoDbContext, MongoDbContext>();
        services.AddSingleton<MongoIndexConfigurator>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<INoteRepository, NoteRepository>();
        services.AddScoped<IPostRepository, PostRepository>();

        // Security
        services.Configure<JwtSettings>(
            configuration.GetSection(JwtSettings.SectionName));
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}
