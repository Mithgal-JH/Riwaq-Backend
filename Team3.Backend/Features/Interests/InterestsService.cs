using Team3.Backend.Features.AI.Interfaces;
using Team3.Backend.Features.Interests.Dtos;
using Team3.Backend.Features.Interests.Interfaces;
using Team3.Backend.Models;
using Team3.Backend.Services.Caching;

namespace Team3.Backend.Features.Interests;

public class InterestsService : IInterestsService
{
    private readonly IInterestsRepository _interestsRepository;
    private readonly IProfileSyncService? _profileSyncService;
    private readonly ICacheService _cacheService;

    public InterestsService(
        IInterestsRepository interestsRepository,
        ICacheService cacheService,
        IProfileSyncService? profileSyncService = null)
    {
        _interestsRepository = interestsRepository;
        _profileSyncService = profileSyncService;
        _cacheService = cacheService;
    }

    public async Task<IReadOnlyList<InterestResponse>> GetAllAsync()
    {
        const string cacheKey = "interests:all";

        var cached = await _cacheService.GetAsync<List<InterestResponse>>(cacheKey);

        if (cached is not null)
            return cached;

        var interests = await _interestsRepository.GetAllAsync();
        var result = interests.Select(MapToResponse).ToList();

        await _cacheService.SetAsync(
            cacheKey,
            result,
            TimeSpan.FromMinutes(30));

        return result;
    }

    public async Task<InterestResponse?> GetByIdAsync(Guid interestId)
    {
        var cacheKey = $"interest:{interestId}";

        var cached = await _cacheService.GetAsync<InterestResponse>(cacheKey);

        if (cached is not null)
            return cached;

        var interest = await _interestsRepository.GetByIdAsync(interestId);

        if (interest is null)
            return null;

        var result = MapToResponse(interest);

        await _cacheService.SetAsync(
            cacheKey,
            result,
            TimeSpan.FromMinutes(30));

        return result;
    }

    public async Task<IReadOnlyList<InterestResponse>> GetMyInterestsAsync(
        Guid userId)
    {
        var user = await GetUserAsync(userId);
        var interests = user.UserInterests
            .Select(userInterest => userInterest.Interest)
            .OrderBy(interest => interest.Name)
            .ToList();

        return interests.Select(MapToResponse).ToList();
    }

    public async Task<InterestResponse> AddMyInterestAsync(
        Guid userId,
        Guid interestId)
    {
        var user = await GetUserAsync(userId);
        var interest = await _interestsRepository.GetByIdAsync(interestId);

        if (interest is null)
        {
            throw new KeyNotFoundException("Interest not found.");
        }

        if (await _interestsRepository.UserHasInterestAsync(
                user.Id,
                interestId))
        {
            throw new InvalidOperationException(
                "The interest is already associated with this profile.");
        }

        _interestsRepository.AddUserInterest(new UserInterest
        {
            UserId = user.Id,
            InterestId = interest.Id
        });

        await _interestsRepository.SaveChangesAsync();
        await SyncProfileAsync(userId);
        return MapToResponse(interest);
    }

    public async Task<bool> RemoveMyInterestAsync(
        Guid userId,
        Guid interestId)
    {
        var user = await GetUserAsync(userId);
        var removed = await _interestsRepository.RemoveUserInterestAsync(
            user.Id,
            interestId);

        if (!removed)
        {
            throw new KeyNotFoundException(
                "The interest is not associated with this profile.");
        }

        await _interestsRepository.SaveChangesAsync();
        await SyncProfileAsync(userId);
        return true;
    }

    private async Task<User> GetUserAsync(Guid userId)
    {
        var user = await _interestsRepository
            .GetUserByIdAsync(userId);

        return user ?? throw new KeyNotFoundException("User not found.");
    }

    private static InterestResponse MapToResponse(Interest interest)
    {
        return new InterestResponse
        {
            Id = interest.Id,
            Name = interest.Name,
            Description = interest.Description
        };
    }

    private async Task SyncProfileAsync(Guid userId)
    {
        if (_profileSyncService is not null)
        {
            await _profileSyncService.UpsertProfileAsync(userId);
        }
    }
}
