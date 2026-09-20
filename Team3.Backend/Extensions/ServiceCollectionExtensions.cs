using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Team3.Backend.Data;
using Team3.Backend.Features.Authentication;
using Team3.Backend.Features.Authentication.Interfaces;
using Team3.Backend.Features.ConnectionRequests;
using Team3.Backend.Features.ConnectionRequests.Interfaces;
using Team3.Backend.Features.EducationalContent;
using Team3.Backend.Features.EducationalContent.Interfaces;
using Team3.Backend.Features.Experiences;
using Team3.Backend.Features.Experiences.Interfaces;
using Team3.Backend.Features.Interests;
using Team3.Backend.Features.Interests.Interfaces;
using Team3.Backend.Features.LearningDirections;
using Team3.Backend.Features.LearningDirections.Interfaces;
using Team3.Backend.Features.LearningSessions;
using Team3.Backend.Features.LearningSessions.Interfaces;
using Team3.Backend.Features.Progress;
using Team3.Backend.Features.Progress.Interfaces;
using Team3.Backend.Features.Points;
using Team3.Backend.Features.Skills;
using Team3.Backend.Features.Skills.Interfaces;
using Team3.Backend.Features.Users;
using Team3.Backend.Features.Users.Interfaces;
using Team3.Backend.Models;
using Team3.Backend.Features.Comments;
using Team3.Backend.Features.Comments.Interfaces;
using Team3.Backend.Features.Notifications;
using Team3.Backend.Features.Notifications.Interfaces;

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

        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped<IUserProvisioningService, UserProvisioningService>();

        services.AddScoped<IUsersRepository, UsersRepository>();

        services.AddScoped<IUsersService, UsersService>();

        services.AddScoped<ISkillsRepository, SkillsRepository>();

        services.AddScoped<ISkillsService, SkillsService>();

        services.AddScoped<IInterestsRepository, InterestsRepository>();

        services.AddScoped<IInterestsService, InterestsService>();

        services.AddScoped<IExperiencesRepository, ExperiencesRepository>();

        services.AddScoped<IExperiencesService, ExperiencesService>();

        services.AddScoped<IProgressRepository, ProgressRepository>();

        services.AddScoped<IProgressService, ProgressService>();

        services.AddScoped<PointsService>();

        services.AddScoped<ILearningDirectionsRepository, LearningDirectionsRepository>();

        services.AddScoped<ILearningDirectionsService, LearningDirectionsService>();

        services.AddScoped<ILearningSessionsRepository, LearningSessionsRepository>();
        services.AddScoped<ILearningSessionsService, LearningSessionsService>();

        // Register Educational Content services.
        services.AddScoped<
            IEducationalContentRepository,
            EducationalContentRepository>();

        services.AddScoped<
            IEducationalContentService,
            EducationalContentService>();

        // Register Educational Content interaction services.
        services.AddScoped<
            IEducationalContentInteractionsRepository,
            EducationalContentInteractionsRepository>();

        services.AddScoped<
            IEducationalContentInteractionsService,
            EducationalContentInteractionsService>();



        // Register Comment services.
        services.AddScoped<
            ICommentsRepository,
            CommentsRepository>();

        services.AddScoped<
            ICommentsService,
            CommentsService>();

        services.AddScoped<
            IConnectionRequestsRepository,
            ConnectionRequestsRepository>();

        services.AddScoped<
            IConnectionRequestsService,
            ConnectionRequestsService>();

        services.AddScoped<INotificationsService, NotificationsService>();
        services.AddScoped<INotificationRealtimePublisher, SignalRNotificationRealtimePublisher>();
        services.AddSingleton<IUserIdProvider, LocalUserIdProvider>();

        return services;
    }
}
