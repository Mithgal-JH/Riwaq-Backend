namespace Team3.Backend.Features.Conversations.Dtos;

public class SendMessageRequest
{
    public Guid? ConnectionId { get; set; }

    public string Content { get; set; } = string.Empty;

    public string? Subject { get; set; }
}
