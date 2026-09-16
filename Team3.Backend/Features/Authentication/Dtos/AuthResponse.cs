namespace Team3.Backend.Features.Authentication.Dtos;

public class AuthResponse
{
    public Guid UserId { get; set; }

    public string FirebaseUid { get; set; } = string.Empty;

    public int Points { get; set; }

    public Guid? LearningDirectionId { get; set; }

    public bool IsNewUser { get; set; }
}