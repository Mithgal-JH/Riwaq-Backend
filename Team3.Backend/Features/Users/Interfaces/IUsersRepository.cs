using Team3.Backend.Models;

namespace Team3.Backend.Features.Users.Interfaces;

public interface IUsersRepository
{
    Task<User?> GetByIdWithProfileAsync(Guid id);

    Task<Skill?> GetSkillByIdAsync(Guid skillId);

    void AddProfile(Profile profile);

    Task SaveChangesAsync();
}
