using Team3.Backend.Features.Experiences.Dtos;
using Team3.Backend.Features.Experiences.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Features.Experiences;

public class ExperiencesService : IExperiencesService
{
    private readonly IExperiencesRepository _experiencesRepository;

    public ExperiencesService(IExperiencesRepository experiencesRepository)
    {
        _experiencesRepository = experiencesRepository;
    }

    public async Task<IReadOnlyList<ExperienceResponse>> GetMyExperiencesAsync(
        Guid userId)
    {
        await EnsureUserExistsAsync(userId);
        var experiences = await _experiencesRepository.GetByUserIdAsync(userId);

        return experiences.Select(MapToResponse).ToList();
    }

    public async Task<ExperienceResponse> CreateAsync(
        Guid userId,
        CreateExperienceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateTitle(request.Title);

        await EnsureUserExistsAsync(userId);
        var experience = new Experience
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = request.Title.Trim(),
            Description = request.Description
        };

        _experiencesRepository.Add(experience);
        await _experiencesRepository.SaveChangesAsync();

        return MapToResponse(experience);
    }

    public async Task<ExperienceResponse?> GetByIdAsync(
        Guid userId,
        Guid experienceId)
    {
        ValidateId(experienceId);

        var experience = await _experiencesRepository.GetByIdForUserAsync(
            experienceId,
            userId);

        return experience is null ? null : MapToResponse(experience);
    }

    public async Task<ExperienceResponse> UpdateAsync(
        Guid userId,
        Guid experienceId,
        UpdateExperienceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateId(experienceId);
        ValidateTitle(request.Title);

        var experience = await _experiencesRepository.GetByIdForUserAsync(
            experienceId,
            userId);

        if (experience is null)
        {
            throw new KeyNotFoundException("Experience not found.");
        }

        experience.Title = request.Title.Trim();
        experience.Description = request.Description;

        await _experiencesRepository.SaveChangesAsync();
        return MapToResponse(experience);
    }

    public async Task DeleteAsync(Guid userId, Guid experienceId)
    {
        ValidateId(experienceId);

        var experience = await _experiencesRepository.GetByIdForUserAsync(
            experienceId,
            userId);

        if (experience is null)
        {
            throw new KeyNotFoundException("Experience not found.");
        }

        _experiencesRepository.Remove(experience);
        await _experiencesRepository.SaveChangesAsync();
    }

    private async Task EnsureUserExistsAsync(Guid userId)
    {
        var user = await _experiencesRepository.GetUserByIdAsync(userId);

        if (user is null)
        {
            throw new KeyNotFoundException("User not found.");
        }
    }

    private static void ValidateTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.");
        }
    }

    private static void ValidateId(Guid experienceId)
    {
        if (experienceId == Guid.Empty)
        {
            throw new ArgumentException(
                "Experience ID must be a valid Guid.");
        }
    }

    private static ExperienceResponse MapToResponse(Experience experience)
    {
        return new ExperienceResponse
        {
            Id = experience.Id,
            UserId = experience.UserId,
            Title = experience.Title,
            Description = experience.Description
        };
    }
}
