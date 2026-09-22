using Team3.Backend.Features.LearningSessions.Dtos;

namespace Team3.Backend.Features.LearningSessions.Interfaces;

public interface ILearningSessionsService
{
    Task<IReadOnlyList<LearningSessionResponse>> GetMySessionsAsync(Guid userId);

    Task<LearningSessionResponse?> GetByIdAsync(
        Guid userId,
        Guid sessionId);

    Task<LearningSessionResponse> CreateAsync(
        Guid userId,
        CreateLearningSessionRequest request);

    Task<LearningSessionResponse> UpdateAsync(
        Guid userId,
        Guid sessionId,
        UpdateLearningSessionRequest request);
}