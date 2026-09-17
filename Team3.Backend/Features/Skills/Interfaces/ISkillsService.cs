using Team3.Backend.Features.Skills.Dtos;

namespace Team3.Backend.Features.Skills.Interfaces;

public interface ISkillsService
{
    Task<IReadOnlyList<SkillResponse>> GetAllAsync();

    Task<SkillResponse?> GetByIdAsync(Guid skillId);

    Task<IReadOnlyList<SkillResponse>> GetMySkillsAsync(string firebaseUid);

    Task<SkillResponse> AddMySkillAsync(string firebaseUid, Guid skillId);

    Task<bool> RemoveMySkillAsync(string firebaseUid, Guid skillId);
}
