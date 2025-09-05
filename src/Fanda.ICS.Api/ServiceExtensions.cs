using Fanda.ICS.Api.Services;

namespace Fanda.ICS.Api;

public static class ServiceExtensions
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
    }
}
