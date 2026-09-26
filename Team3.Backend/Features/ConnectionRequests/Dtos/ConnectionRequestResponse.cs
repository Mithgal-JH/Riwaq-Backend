using Team3.Backend.Features.Users.Dtos;

namespace Team3.Backend.Features.ConnectionRequests.Dtos;

public class ConnectionRequestResponse
{
    public Guid Id { get; set; }

    public PublicUserProfileResponse Sender { get; set; } = null!;

    public PublicUserProfileResponse Receiver { get; set; } = null!;

    public string Status { get; set; } = string.Empty;

    public Guid? ConversationId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}