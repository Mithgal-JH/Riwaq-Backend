using Team3.Backend.Features.Experiences.Dtos;

namespace Team3.Backend.Features.Experiences.Interfaces;

public interface IExperiencesService
{
    Task<IReadOnlyList<ExperienceResponse>> GetMyExperiencesAsync(
        Guid userId);

    Task<ExperienceResponse> CreateAsync(
        Guid userId,
        CreateExperienceRequest request);

    Task<ExperienceResponse?> GetByIdAsync(
        Guid userId,
        Guid experienceId);

    Task<ExperienceResponse> UpdateAsync(
        Guid userId,
        Guid experienceId,
        UpdateExperienceRequest request);

    Task DeleteAsync(Guid userId, Guid experienceId);
}
