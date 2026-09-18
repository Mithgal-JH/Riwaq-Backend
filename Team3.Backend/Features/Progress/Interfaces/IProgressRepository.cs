using Team3.Backend.Models;
using ProgressEntity = Team3.Backend.Models.Progress;

namespace Team3.Backend.Features.Progress.Interfaces;

public interface IProgressRepository
{
    Task<User?> GetUserByIdAsync(Guid userId);

    Task<List<ProgressEntity>> GetByUserIdAsync(Guid userId);

    Task<ProgressEntity?> GetByIdForUserAsync(
        Guid progressId,
        Guid userId);

    Task<ProgressEntity?> GetTrackedByIdForUserAsync(
        Guid progressId,
        Guid userId);

    Task<LearningDirection?> GetLearningDirectionByIdAsync(
        Guid learningDirectionId);

    Task<bool> ExistsForUserAsync(
        Guid userId,
        Guid learningDirectionId);

    void Add(ProgressEntity progress);

    void Remove(ProgressEntity progress);

    Task SaveChangesAsync();
}
