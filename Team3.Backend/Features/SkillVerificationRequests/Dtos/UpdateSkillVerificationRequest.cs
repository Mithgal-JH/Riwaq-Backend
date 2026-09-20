namespace Team3.Backend.Features.SkillVerificationRequests.Dtos;

public class UpdateSkillVerificationRequest
{
    public string Status { get; set; } = string.Empty;

    public string? Note { get; set; }
}
