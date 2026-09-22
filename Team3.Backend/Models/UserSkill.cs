namespace Team3.Backend.Models;

public class UserSkill
{
    public Guid UserId { get; set; }

    public Guid SkillId { get; set; }

    public User User { get; set; } = null!;

    public Skill Skill { get; set; } = null!;
}
