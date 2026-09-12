namespace Team3.Backend.Models;

public class Skill
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ICollection<UserSkill> UserSkills { get; set; } =
        new List<UserSkill>();

    public ICollection<LearningDirectionSkill> LearningDirectionSkills { get; set; } =
        new List<LearningDirectionSkill>();

    public ICollection<EducationalContentSkill> EducationalContentSkills { get; set; } =
        new List<EducationalContentSkill>();
}
