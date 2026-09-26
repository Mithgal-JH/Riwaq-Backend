using Team3.Backend.Features.Skills.Dtos;
using Team3.Backend.Features.Users.Dtos;

namespace Team3.Backend.Features.SkillVerificationRequests.Dtos;

public class SkillVerificationRequestResponse
{
    public Guid Id { get; set; }

    public PublicUserProfileResponse Requester { get; set; } = new();

    public PublicUserProfileResponse Mentor { get; set; } = new();

    public SkillResponse Skill { get; set; } = new();

    public string Status { get; set; } = string.Empty;

    public int? Score { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
