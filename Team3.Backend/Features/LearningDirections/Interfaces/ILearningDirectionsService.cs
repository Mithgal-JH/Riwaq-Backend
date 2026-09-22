using Team3.Backend.Features.LearningDirections.Dtos;

namespace Team3.Backend.Features.LearningDirections.Interfaces;

public interface ILearningDirectionsService
{
    Task<IReadOnlyList<LearningDirectionResponse>> GetAllAsync();

    Task<LearningDirectionDetailsResponse?> GetByIdAsync(
        Guid learningDirectionId);
}
