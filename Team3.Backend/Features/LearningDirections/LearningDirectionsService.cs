using Team3.Backend.Features.LearningDirections.Dtos;
using Team3.Backend.Features.LearningDirections.Interfaces;
using Team3.Backend.Features.Skills.Dtos;
using Team3.Backend.Models;
using Team3.Backend.Services.Caching;
namespace Team3.Backend.Features.LearningDirections;

public class LearningDirectionsService : ILearningDirectionsService
{
    private readonly ILearningDirectionsRepository _learningDirectionsRepository;
    private readonly ICacheService _cacheService;

    public LearningDirectionsService(
        ILearningDirectionsRepository learningDirectionsRepository,
        ICacheService cacheService)
    {
        _learningDirectionsRepository = learningDirectionsRepository;
        _cacheService = cacheService;
    }


    public async Task<IReadOnlyList<LearningDirectionResponse>> GetAllAsync()
    {
        const string cacheKey = "learning-directions:all";

        var cached = await _cacheService
            .GetAsync<List<LearningDirectionResponse>>(cacheKey);

        if (cached is not null)
            return cached;

        var learningDirections =
            await _learningDirectionsRepository.GetAllAsync();

        var result = learningDirections
            .Select(MapToResponse)
            .ToList();

        await _cacheService.SetAsync(
            cacheKey,
            result,
            TimeSpan.FromMinutes(30));

        return result;
    }
    public async Task<LearningDirectionDetailsResponse?> GetByIdAsync(
       Guid learningDirectionId)
    {
        var cacheKey = $"learning-direction:{learningDirectionId}";

        var cached = await _cacheService
            .GetAsync<LearningDirectionDetailsResponse>(cacheKey);

        if (cached is not null)
            return cached;

        var learningDirection = await _learningDirectionsRepository
            .GetByIdWithSkillsAsync(learningDirectionId);

        if (learningDirection is null)
            return null;

        var result = MapToDetailsResponse(learningDirection);

        await _cacheService.SetAsync(
            cacheKey,
            result,
            TimeSpan.FromMinutes(30));

        return result;
    }

    private static LearningDirectionResponse MapToResponse(
        LearningDirection learningDirection)
    {
        return new LearningDirectionResponse
        {
            Id = learningDirection.Id,
            Name = learningDirection.Name,
            Description = learningDirection.Description
        };
    }

    private static LearningDirectionDetailsResponse MapToDetailsResponse(
        LearningDirection learningDirection)
    {
        return new LearningDirectionDetailsResponse
        {
            Id = learningDirection.Id,
            Name = learningDirection.Name,
            Description = learningDirection.Description,
            Skills = learningDirection.LearningDirectionSkills
                .Select(directionSkill => MapToSkillResponse(directionSkill.Skill))
                .OrderBy(skill => skill.Name)
                .ToList()
        };
    }

    private static SkillResponse MapToSkillResponse(Skill skill)
    {
        return new SkillResponse
        {
            Id = skill.Id,
            Name = skill.Name,
            Description = skill.Description
        };
    }
}
