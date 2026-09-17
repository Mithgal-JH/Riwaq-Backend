using Team3.Backend.Features.Skills.Dtos;

namespace Team3.Backend.Features.LearningDirections.Dtos;

public class LearningDirectionDetailsResponse : LearningDirectionResponse
{
    public IReadOnlyList<SkillResponse> Skills { get; set; } =
        Array.Empty<SkillResponse>();
}
