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
        Guid userId)
    {
        await EnsureUserExistsAsync(userId);
        var progress = await _progressRepository.GetByUserIdAsync(userId);

        return progress.Select(MapToResponse).ToList();
    }

    public async Task<ProgressResponse?> GetByIdAsync(
        Guid userId,
        Guid progressId)
    {
        ValidateId(progressId);

        var progress = await _progressRepository.GetByIdForUserAsync(
            progressId,
            userId);

        return progress is null ? null : MapToResponse(progress);
    }

    public async Task<ProgressResponse> CreateAsync(
        Guid userId,
        CreateProgressRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateId(request.LearningDirectionId);
        ValidateLevel(request.Level);

        await EnsureUserExistsAsync(userId);
        var learningDirection = await _progressRepository
            .GetLearningDirectionByIdAsync(request.LearningDirectionId);

        if (learningDirection is null)
        {
            throw new KeyNotFoundException("Learning direction not found.");
        }

        if (await _progressRepository.ExistsForUserAsync(
                userId,
                request.LearningDirectionId))
        {
            throw new InvalidOperationException(
                "Progress already exists for this learning direction.");
        }

        var progress = new ProgressEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
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
        Guid userId,
        Guid progressId,
        UpdateProgressRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateId(progressId);
        ValidateLevel(request.Level);

        var progress = await _progressRepository
            .GetTrackedByIdForUserAsync(progressId, userId);

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
            userId);

        return updatedProgress is null
            ? MapToResponse(progress)
            : MapToResponse(updatedProgress);
    }

    public async Task DeleteAsync(Guid userId, Guid progressId)
    {
        ValidateId(progressId);

        var progress = await _progressRepository
            .GetTrackedByIdForUserAsync(progressId, userId);

        if (progress is null)
        {
            throw new KeyNotFoundException("Progress not found.");
        }

        _progressRepository.Remove(progress);
        await _progressRepository.SaveChangesAsync();
    }

    private async Task EnsureUserExistsAsync(Guid userId)
    {
        var user = await _progressRepository
            .GetUserByIdAsync(userId);

        if (user is null)
        {
            throw new KeyNotFoundException("User not found.");
        }
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
