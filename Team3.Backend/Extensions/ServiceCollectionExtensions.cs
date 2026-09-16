using Team3.Backend.Features.Authentication;
using Team3.Backend.Features.Authentication.Interfaces;

namespace Team3.Backend.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        // Register AuthenticationService through its interface.
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        return services;
    }
}