using Team3.Backend.Features.Skills.Dtos;

namespace Team3.Backend.Features.Skills.Interfaces;

public interface ISkillsService
{
    Task<IReadOnlyList<SkillResponse>> GetAllAsync();

    Task<SkillResponse?> GetByIdAsync(Guid skillId);

    Task<IReadOnlyList<SkillResponse>> GetMySkillsAsync(Guid userId);

    Task<SkillResponse> AddMySkillAsync(Guid userId, Guid skillId);

    Task<bool> RemoveMySkillAsync(Guid userId, Guid skillId);
}
