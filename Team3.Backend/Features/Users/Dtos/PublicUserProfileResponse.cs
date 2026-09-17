namespace Team3.Backend.Features.Users.Dtos;

public class PublicUserProfileResponse
{
    public Guid UserId { get; set; }

    public int Points { get; set; }

    public Guid? LearningDirectionId { get; set; }

    public string? LearningDirectionName { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Bio { get; set; }

    public string? University { get; set; }
}
