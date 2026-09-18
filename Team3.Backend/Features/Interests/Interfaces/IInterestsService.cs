using Team3.Backend.Features.Interests.Dtos;

namespace Team3.Backend.Features.Interests.Interfaces;

public interface IInterestsService
{
    Task<IReadOnlyList<InterestResponse>> GetAllAsync();

    Task<InterestResponse?> GetByIdAsync(Guid interestId);

    Task<IReadOnlyList<InterestResponse>> GetMyInterestsAsync(
        Guid userId);

    Task<InterestResponse> AddMyInterestAsync(
        Guid userId,
        Guid interestId);

    Task<bool> RemoveMyInterestAsync(
        Guid userId,
        Guid interestId);
}
