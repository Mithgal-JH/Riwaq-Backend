namespace Team3.Backend.Models;

public class SkillVerificationRequest
{
    public Guid Id { get; set; }

    public Guid RequesterUserId { get; set; }

    public Guid MentorUserId { get; set; }

    public Guid SkillId { get; set; }

    public string Status { get; set; } = string.Empty;

    public int? Score { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public User RequesterUser { get; set; } = null!;

    public User MentorUser { get; set; } = null!;

    public Skill Skill { get; set; } = null!;
}
