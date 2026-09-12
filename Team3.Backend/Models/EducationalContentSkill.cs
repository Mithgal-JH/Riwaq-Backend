namespace Team3.Backend.Models;

public class EducationalContentSkill
{
    public Guid EducationalContentId { get; set; }

    public Guid SkillId { get; set; }

    public EducationalContent EducationalContent { get; set; } = null!;

    public Skill Skill { get; set; } = null!;
}
