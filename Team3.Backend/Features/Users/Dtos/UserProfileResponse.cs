namespace Team3.Backend.Features.Users.Dtos;

public class UserProfileResponse
{
    public Guid UserId { get; set; }

    public string FirebaseUid { get; set; } = string.Empty;

    public int Points { get; set; }

    public Guid? LearningDirectionId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Bio { get; set; }

    public string? University { get; set; }
}