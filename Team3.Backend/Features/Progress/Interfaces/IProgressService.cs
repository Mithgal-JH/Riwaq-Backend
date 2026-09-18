using Team3.Backend.Features.Progress.Dtos;

namespace Team3.Backend.Features.Progress.Interfaces;

public interface IProgressService
{
    Task<IReadOnlyList<ProgressResponse>> GetMyProgressAsync(
        Guid userId);

    Task<ProgressResponse?> GetByIdAsync(
        Guid userId,
        Guid progressId);

    Task<ProgressResponse> CreateAsync(
        Guid userId,
        CreateProgressRequest request);

    Task<ProgressResponse> UpdateAsync(
        Guid userId,
        Guid progressId,
        UpdateProgressRequest request);

    Task DeleteAsync(Guid userId, Guid progressId);
}
