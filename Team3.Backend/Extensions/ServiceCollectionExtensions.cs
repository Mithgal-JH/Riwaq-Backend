using Microsoft.AspNetCore.Identity;
using Team3.Backend.Data;
using Team3.Backend.Features.Authentication;
using Team3.Backend.Features.Authentication.Interfaces;
using Team3.Backend.Features.LearningDirections;
using Team3.Backend.Features.LearningDirections.Interfaces;
using Team3.Backend.Features.Skills;
using Team3.Backend.Features.Skills.Interfaces;
using Team3.Backend.Features.Users;
using Team3.Backend.Features.Users.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services
            .AddIdentityCore<User>(options =>
            {
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddScoped<IAuthenticationService, AuthenticationService>();

        services.AddScoped<FirebaseAuthenticationService>();

        services.AddScoped<IUsersRepository, UsersRepository>();

        services.AddScoped<IUsersService, UsersService>();

        services.AddScoped<ISkillsRepository, SkillsRepository>();

        services.AddScoped<ISkillsService, SkillsService>();

        services.AddScoped<ILearningDirectionsRepository, LearningDirectionsRepository>();

        services.AddScoped<ILearningDirectionsService, LearningDirectionsService>();

        return services;
    }
}