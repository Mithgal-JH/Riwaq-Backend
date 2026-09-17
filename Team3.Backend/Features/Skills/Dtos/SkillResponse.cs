namespace Team3.Backend.Features.Skills.Dtos;

public class SkillResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
