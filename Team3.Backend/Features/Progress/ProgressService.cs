using Team3.Backend.Features.Progress.Dtos;
using Team3.Backend.Features.Progress.Interfaces;
using Team3.Backend.Models;
using ProgressEntity = Team3.Backend.Models.Progress;

namespace Team3.Backend.Features.Progress;

public class ProgressService : IProgressService
{
    private readonly IProgressRepository _progressRepository;

    public ProgressService(IProgressRepository progressRepository)
    {
        _progressRepository = progressRepository;
    }

    public async Task<IReadOnlyList<ProgressResponse>> GetMyProgressAsync(
        string firebaseUid)
    {
        var user = await GetUserAsync(firebaseUid);
        var progress = await _progressRepository.GetByUserIdAsync(user.Id);

        return progress.Select(MapToResponse).ToList();
    }

    public async Task<ProgressResponse?> GetByIdAsync(
        string firebaseUid,
        Guid progressId)
    {
        ValidateId(progressId);

        var user = await GetUserAsync(firebaseUid);
        var progress = await _progressRepository.GetByIdForUserAsync(
            progressId,
            user.Id);

        return progress is null ? null : MapToResponse(progress);
    }

    public async Task<ProgressResponse> CreateAsync(
        string firebaseUid,
        CreateProgressRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateId(request.LearningDirectionId);
        ValidateLevel(request.Level);

        var user = await GetUserAsync(firebaseUid);
        var learningDirection = await _progressRepository
            .GetLearningDirectionByIdAsync(request.LearningDirectionId);

        if (learningDirection is null)
        {
            throw new KeyNotFoundException("Learning direction not found.");
        }

        if (await _progressRepository.ExistsForUserAsync(
                user.Id,
                request.LearningDirectionId))
        {
            throw new InvalidOperationException(
                "Progress already exists for this learning direction.");
        }

        var progress = new ProgressEntity
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            LearningDirectionId = request.LearningDirectionId,
            Level = request.Level.Trim(),
            StartedAt = request.StartedAt,
            UpdatedAt = DateTime.UtcNow
        };

        _progressRepository.Add(progress);
        await _progressRepository.SaveChangesAsync();

        return MapToResponse(progress, learningDirection.Name);
    }

    public async Task<ProgressResponse> UpdateAsync(
        string firebaseUid,
        Guid progressId,
        UpdateProgressRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateId(progressId);
        ValidateLevel(request.Level);

        var user = await GetUserAsync(firebaseUid);
        var progress = await _progressRepository
            .GetTrackedByIdForUserAsync(progressId, user.Id);

        if (progress is null)
        {
            throw new KeyNotFoundException("Progress not found.");
        }

        progress.Level = request.Level.Trim();
        progress.StartedAt = request.StartedAt;
        progress.UpdatedAt = DateTime.UtcNow;

        await _progressRepository.SaveChangesAsync();

        var updatedProgress = await _progressRepository.GetByIdForUserAsync(
            progressId,
            user.Id);

        return updatedProgress is null
            ? MapToResponse(progress)
            : MapToResponse(updatedProgress);
    }

    public async Task DeleteAsync(string firebaseUid, Guid progressId)
    {
        ValidateId(progressId);

        var user = await GetUserAsync(firebaseUid);
        var progress = await _progressRepository
            .GetTrackedByIdForUserAsync(progressId, user.Id);

        if (progress is null)
        {
            throw new KeyNotFoundException("Progress not found.");
        }

        _progressRepository.Remove(progress);
        await _progressRepository.SaveChangesAsync();
    }

    private async Task<User> GetUserAsync(string firebaseUid)
    {
        var user = await _progressRepository
            .GetUserByFirebaseUidAsync(firebaseUid);

        return user ?? throw new KeyNotFoundException("User not found.");
    }

    private static void ValidateId(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("ID must be a valid Guid.");
        }
    }

    private static void ValidateLevel(string? level)
    {
        if (string.IsNullOrWhiteSpace(level))
        {
            throw new ArgumentException("Level is required.");
        }
    }

    private static ProgressResponse MapToResponse(ProgressEntity progress)
    {
        return new ProgressResponse
        {
            Id = progress.Id,
            LearningDirectionId = progress.LearningDirectionId,
            LearningDirectionName = progress.LearningDirection?.Name,
            Level = progress.Level,
            StartedAt = progress.StartedAt,
            UpdatedAt = progress.UpdatedAt
        };
    }

    private static ProgressResponse MapToResponse(
        ProgressEntity progress,
        string learningDirectionName)
    {
        var response = MapToResponse(progress);
        response.LearningDirectionName = learningDirectionName;
        return response;
    }
}
