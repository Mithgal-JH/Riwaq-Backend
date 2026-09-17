using Team3.Backend.Features.LearningDirections.Dtos;
using Team3.Backend.Features.LearningDirections.Interfaces;
using Team3.Backend.Features.Skills.Dtos;
using Team3.Backend.Models;

namespace Team3.Backend.Features.LearningDirections;

public class LearningDirectionsService : ILearningDirectionsService
{
    private readonly ILearningDirectionsRepository _learningDirectionsRepository;

    public LearningDirectionsService(
        ILearningDirectionsRepository learningDirectionsRepository)
    {
        _learningDirectionsRepository = learningDirectionsRepository;
    }

    public async Task<IReadOnlyList<LearningDirectionResponse>> GetAllAsync()
    {
        var learningDirections = await _learningDirectionsRepository.GetAllAsync();
        return learningDirections.Select(MapToResponse).ToList();
    }

    public async Task<LearningDirectionDetailsResponse?> GetByIdAsync(
        Guid learningDirectionId)
    {
        var learningDirection = await _learningDirectionsRepository
            .GetByIdWithSkillsAsync(learningDirectionId);

        return learningDirection is null
            ? null
            : MapToDetailsResponse(learningDirection);
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
