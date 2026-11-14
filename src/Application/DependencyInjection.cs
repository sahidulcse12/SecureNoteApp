using Microsoft.Extensions.DependencyInjection;
using SecureNoteApp.Application.Services;

namespace SecureNoteApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<INoteService, NoteService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<IAggregationService, AggregationService>();

        return services;
    }
}
