using Team3.Backend.Models;

namespace Team3.Backend.Features.Users.Interfaces;

public interface IUsersRepository
{
    Task<User?> GetByIdWithProfileAsync(Guid id);

    Task<User?> GetByIdForAiSyncAsync(Guid id);

    Task<List<User>> GetAllForAiSyncAsync();

    Task<List<User>> GetPublicProfilesByIdsAsync(
        IReadOnlyCollection<Guid> userIds);

    Task<Skill?> GetSkillByIdAsync(Guid skillId);

    void AddProfile(Profile profile);

    Task SaveChangesAsync();
}
