using Team3.Backend.Models;

namespace Team3.Backend.Features.LearningDirections.Interfaces;

public interface ILearningDirectionsRepository
{
    Task<List<LearningDirection>> GetAllAsync();

    Task<LearningDirection?> GetByIdWithSkillsAsync(Guid learningDirectionId);
}
