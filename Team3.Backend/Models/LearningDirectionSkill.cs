namespace Team3.Backend.Models;

public class LearningDirectionSkill
{
    public Guid LearningDirectionId { get; set; }

    public Guid SkillId { get; set; }

    public LearningDirection LearningDirection { get; set; } = null!;

    public Skill Skill { get; set; } = null!;
}
