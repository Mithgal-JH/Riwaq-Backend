using Team3.Backend.Models;

namespace Team3.Backend.Features.Experiences.Interfaces;

public interface IExperiencesRepository
{
    Task<User?> GetUserByIdAsync(Guid userId);

    Task<List<Experience>> GetByUserIdAsync(Guid userId);

    Task<Experience?> GetByIdForUserAsync(
        Guid experienceId,
        Guid userId);

    Task<Experience?> GetByIdAsync(Guid experienceId);

    void Add(Experience experience);

    void Remove(Experience experience);

    Task SaveChangesAsync();
}
