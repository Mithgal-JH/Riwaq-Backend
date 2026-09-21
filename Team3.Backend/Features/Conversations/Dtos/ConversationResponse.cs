using Team3.Backend.Features.Users.Dtos;

namespace Team3.Backend.Features.Conversations.Dtos;

public class ConversationResponse
{
    public Guid Id { get; set; }

    public string? Subject { get; set; }

    public IReadOnlyList<PublicUserProfileResponse> Participants { get; set; } =
        new List<PublicUserProfileResponse>();

    public DateTime LastActivityAt { get; set; }
}
