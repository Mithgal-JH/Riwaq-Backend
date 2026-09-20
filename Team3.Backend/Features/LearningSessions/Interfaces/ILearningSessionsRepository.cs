using Team3.Backend.Models;

namespace Team3.Backend.Features.LearningSessions.Interfaces;

public interface ILearningSessionsRepository
{
    Task<Connection?> GetConnectionAsync(Guid connectionId);

    Task<List<LearningSession>> GetByUserIdAsync(Guid userId);

    Task<LearningSession?> GetByIdForUserAsync(
        Guid sessionId,
        Guid userId);

    Task<LearningSession?> GetTrackedByIdForUserAsync(
        Guid sessionId,
        Guid userId);

    void Add(LearningSession session);

    Task SaveChangesAsync();
}