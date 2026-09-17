using Team3.Backend.Features.Interests.Dtos;

namespace Team3.Backend.Features.Interests.Interfaces;

public interface IInterestsService
{
    Task<IReadOnlyList<InterestResponse>> GetAllAsync();

    Task<InterestResponse?> GetByIdAsync(Guid interestId);

    Task<IReadOnlyList<InterestResponse>> GetMyInterestsAsync(
        string firebaseUid);

    Task<InterestResponse> AddMyInterestAsync(
        string firebaseUid,
        Guid interestId);

    Task<bool> RemoveMyInterestAsync(
        string firebaseUid,
        Guid interestId);
}
