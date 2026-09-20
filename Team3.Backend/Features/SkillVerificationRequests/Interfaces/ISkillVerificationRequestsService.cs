using Team3.Backend.Features.SkillVerificationRequests.Dtos;

namespace Team3.Backend.Features.SkillVerificationRequests.Interfaces;

public interface ISkillVerificationRequestsService
{
    Task<SkillVerificationRequestResponse> CreateAsync(
        Guid userId,
        CreateSkillVerificationRequest request);

    Task<IReadOnlyList<SkillVerificationRequestResponse>> GetSentAsync(Guid userId);

    Task<IReadOnlyList<SkillVerificationRequestResponse>> GetReceivedAsync(Guid userId);

    Task<SkillVerificationRequestResponse> UpdateAsync(
        Guid userId,
        Guid requestId,
        UpdateSkillVerificationRequest request);
}
