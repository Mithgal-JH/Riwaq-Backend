using Team3.Backend.Models;

namespace Team3.Backend.Features.Skills.Interfaces;

public interface ISkillsRepository
{
    Task<List<Skill>> GetAllAsync();

    Task<Skill?> GetByIdAsync(Guid skillId);

    Task<User?> GetUserByFirebaseUidAsync(string firebaseUid);

    Task<bool> UserHasSkillAsync(Guid userId, Guid skillId);

    Task<bool> RemoveUserSkillAsync(Guid userId, Guid skillId);

    void AddUserSkill(UserSkill userSkill);

    Task SaveChangesAsync();
}
