using Team3.Backend.Models;

namespace Team3.Backend.Features.SkillVerificationRequests.Interfaces;

public interface ISkillVerificationRequestsRepository
{
    Task<User?> GetUserByIdAsync(Guid userId);

    Task<Skill?> GetSkillByIdAsync(Guid skillId);

    Task<SkillVerificationRequest?> GetRequestByIdAsync(Guid requestId);

    Task<SkillVerificationRequest?> GetPendingRequestForSkillAsync(
        Guid requesterUserId,
        Guid mentorUserId,
        Guid skillId);

    Task<bool> HasSharedLearningSessionAsync(Guid requesterUserId, Guid mentorUserId);

    Task<List<SkillVerificationRequest>> GetSentAsync(Guid requesterUserId);

    Task<List<SkillVerificationRequest>> GetReceivedAsync(Guid mentorUserId);

    void Add(SkillVerificationRequest request);

    Task SaveChangesAsync();
}
