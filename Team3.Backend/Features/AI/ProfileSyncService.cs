using Team3.Backend.Features.AI.Dtos;
using Team3.Backend.Features.AI.Interfaces;
using Team3.Backend.Features.Users.Interfaces;

namespace Team3.Backend.Features.AI;

public sealed class ProfileSyncService : IProfileSyncService
{
    private readonly IUsersRepository _usersRepository;
    private readonly IProfileSyncClient _profileSyncClient;
    private readonly ILogger<ProfileSyncService> _logger;

    public ProfileSyncService(
        IUsersRepository usersRepository,
        IProfileSyncClient profileSyncClient,
        ILogger<ProfileSyncService> logger)
    {
        _usersRepository = usersRepository;
        _profileSyncClient = profileSyncClient;
        _logger = logger;
    }

    public async Task UpsertProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _usersRepository.GetByIdForAiSyncAsync(userId);

        if (user is null)
        {
            _logger.LogWarning(
                "Could not synchronize profile {ProfileId} because it was not found.",
                userId);
            return;
        }

        var request = new ProfileSyncRequest
        {
            SyncType = "upsert",
            Profiles = [new ProfileSyncItem
            {
                ProfileId = user.Id.ToString(),
                Skills = user.UserSkills
                    .Select(userSkill => userSkill.Skill.Name)
                    .OrderBy(name => name)
                    .ToList(),
                Interests = user.UserInterests
                    .Select(userInterest => userInterest.Interest.Name)
                    .OrderBy(name => name)
                    .ToList(),
                LearningDirection = user.SelectedSkill?.Name,
                Bio = user.Profile?.Bio
            }]
        };

        await TrySyncAsync(request, userId, cancellationToken);
    }

    public async Task DeleteProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var request = new ProfileSyncRequest
        {
            SyncType = "delete",
            ProfileIds = [userId.ToString()]
        };

        await TrySyncAsync(request, userId, cancellationToken);
    }

    private async Task TrySyncAsync(
        ProfileSyncRequest request,
        Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _profileSyncClient.SyncAsync(request, cancellationToken);
        }
        catch (OperationCanceledException) when (
            !cancellationToken.IsCancellationRequested)
        {
            LogFailure(userId, "timed out");
        }
        catch (Exception exception) when (
            exception is AiServiceException
                or HttpRequestException
                or InvalidOperationException)
        {
            _logger.LogWarning(
                exception,
                "AI profile synchronization failed for profile {ProfileId}.",
                userId);
        }
    }

    private void LogFailure(Guid userId, string reason)
    {
        _logger.LogWarning(
            "AI profile synchronization {Reason} for profile {ProfileId}.",
            reason,
            userId);
    }
}
