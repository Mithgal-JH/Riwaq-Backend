using Team3.Backend.Features.Users.Dtos;

namespace Team3.Backend.Features.Conversations.Dtos;

public class MessageResponse
{
    public Guid Id { get; set; }

    public PublicUserProfileResponse Sender { get; set; } = new();

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
