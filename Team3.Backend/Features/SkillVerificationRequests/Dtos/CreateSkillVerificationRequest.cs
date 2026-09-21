namespace Team3.Backend.Features.SkillVerificationRequests.Dtos;

public class CreateSkillVerificationRequest
{
    public Guid MentorUserId { get; set; }

    public Guid SkillId { get; set; }
}
