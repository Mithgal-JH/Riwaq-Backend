using Team3.Backend.Features.EducationalContent.Dtos;

namespace Team3.Backend.Features.EducationalContent.Interfaces;

public interface IEducationalContentService
{
    Task<IReadOnlyList<EducationalContentResponse>> GetAllAsync();

    Task<EducationalContentResponse?> GetByIdAsync(
        Guid contentId);

    Task<EducationalContentResponse> CreateAsync(
        string firebaseUid,
        CreateEducationalContentRequest request);

    Task<EducationalContentResponse> UpdateAsync(
        string firebaseUid,
        Guid contentId,
        UpdateEducationalContentRequest request);

    Task DeleteAsync(
        string firebaseUid,
        Guid contentId);
}