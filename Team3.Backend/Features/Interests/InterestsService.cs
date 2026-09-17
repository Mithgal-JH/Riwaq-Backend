using Team3.Backend.Features.Interests.Dtos;
using Team3.Backend.Features.Interests.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Features.Interests;

public class InterestsService : IInterestsService
{
    private readonly IInterestsRepository _interestsRepository;

    public InterestsService(IInterestsRepository interestsRepository)
    {
        _interestsRepository = interestsRepository;
    }

    public async Task<IReadOnlyList<InterestResponse>> GetAllAsync()
    {
        var interests = await _interestsRepository.GetAllAsync();
        return interests.Select(MapToResponse).ToList();
    }

    public async Task<InterestResponse?> GetByIdAsync(Guid interestId)
    {
        var interest = await _interestsRepository.GetByIdAsync(interestId);
        return interest is null ? null : MapToResponse(interest);
    }

    public async Task<IReadOnlyList<InterestResponse>> GetMyInterestsAsync(
        string firebaseUid)
    {
        var user = await GetUserAsync(firebaseUid);
        var interests = user.UserInterests
            .Select(userInterest => userInterest.Interest)
            .OrderBy(interest => interest.Name)
            .ToList();

        return interests.Select(MapToResponse).ToList();
    }

    public async Task<InterestResponse> AddMyInterestAsync(
        string firebaseUid,
        Guid interestId)
    {
        var user = await GetUserAsync(firebaseUid);
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
        return MapToResponse(interest);
    }

    public async Task<bool> RemoveMyInterestAsync(
        string firebaseUid,
        Guid interestId)
    {
        var user = await GetUserAsync(firebaseUid);
        var removed = await _interestsRepository.RemoveUserInterestAsync(
            user.Id,
            interestId);

        if (!removed)
        {
            throw new KeyNotFoundException(
                "The interest is not associated with this profile.");
        }

        await _interestsRepository.SaveChangesAsync();
        return true;
    }

    private async Task<User> GetUserAsync(string firebaseUid)
    {
        var user = await _interestsRepository
            .GetUserByFirebaseUidAsync(firebaseUid);

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
}
