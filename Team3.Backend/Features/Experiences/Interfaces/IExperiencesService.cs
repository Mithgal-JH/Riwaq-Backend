using Team3.Backend.Features.Experiences.Dtos;

namespace Team3.Backend.Features.Experiences.Interfaces;

public interface IExperiencesService
{
    Task<IReadOnlyList<ExperienceResponse>> GetMyExperiencesAsync(
        string firebaseUid);

    Task<ExperienceResponse> CreateAsync(
        string firebaseUid,
        CreateExperienceRequest request);

    Task<ExperienceResponse?> GetByIdAsync(
        string firebaseUid,
        Guid experienceId);

    Task<ExperienceResponse> UpdateAsync(
        string firebaseUid,
        Guid experienceId,
        UpdateExperienceRequest request);

    Task DeleteAsync(string firebaseUid, Guid experienceId);
}
