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
        string firebaseUid)
    {
        var user = await GetUserAsync(firebaseUid);
        var experiences = await _experiencesRepository
            .GetByUserIdAsync(user.Id);

        return experiences.Select(MapToResponse).ToList();
    }

    public async Task<ExperienceResponse> CreateAsync(
        string firebaseUid,
        CreateExperienceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateTitle(request.Title);

        var user = await GetUserAsync(firebaseUid);
        var experience = new Experience
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Title = request.Title.Trim(),
            Description = request.Description
        };

        _experiencesRepository.Add(experience);
        await _experiencesRepository.SaveChangesAsync();

        return MapToResponse(experience);
    }

    public async Task<ExperienceResponse?> GetByIdAsync(
        string firebaseUid,
        Guid experienceId)
    {
        ValidateId(experienceId);

        var user = await GetUserAsync(firebaseUid);
        var experience = await _experiencesRepository.GetByIdForUserAsync(
            experienceId,
            user.Id);

        return experience is null ? null : MapToResponse(experience);
    }

    public async Task<ExperienceResponse> UpdateAsync(
        string firebaseUid,
        Guid experienceId,
        UpdateExperienceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateId(experienceId);
        ValidateTitle(request.Title);

        var user = await GetUserAsync(firebaseUid);
        var experience = await _experiencesRepository.GetByIdForUserAsync(
            experienceId,
            user.Id);

        if (experience is null)
        {
            throw new KeyNotFoundException("Experience not found.");
        }

        experience.Title = request.Title.Trim();
        experience.Description = request.Description;

        await _experiencesRepository.SaveChangesAsync();
        return MapToResponse(experience);
    }

    public async Task DeleteAsync(string firebaseUid, Guid experienceId)
    {
        ValidateId(experienceId);

        var user = await GetUserAsync(firebaseUid);
        var experience = await _experiencesRepository.GetByIdForUserAsync(
            experienceId,
            user.Id);

        if (experience is null)
        {
            throw new KeyNotFoundException("Experience not found.");
        }

        _experiencesRepository.Remove(experience);
        await _experiencesRepository.SaveChangesAsync();
    }

    private async Task<User> GetUserAsync(string firebaseUid)
    {
        var user = await _experiencesRepository
            .GetUserByFirebaseUidAsync(firebaseUid);

        return user ?? throw new KeyNotFoundException("User not found.");
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
