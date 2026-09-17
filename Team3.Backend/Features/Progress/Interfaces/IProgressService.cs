using Team3.Backend.Features.Progress.Dtos;

namespace Team3.Backend.Features.Progress.Interfaces;

public interface IProgressService
{
    Task<IReadOnlyList<ProgressResponse>> GetMyProgressAsync(
        string firebaseUid);

    Task<ProgressResponse?> GetByIdAsync(
        string firebaseUid,
        Guid progressId);

    Task<ProgressResponse> CreateAsync(
        string firebaseUid,
        CreateProgressRequest request);

    Task<ProgressResponse> UpdateAsync(
        string firebaseUid,
        Guid progressId,
        UpdateProgressRequest request);

    Task DeleteAsync(string firebaseUid, Guid progressId);
}
