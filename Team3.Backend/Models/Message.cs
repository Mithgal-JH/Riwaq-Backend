namespace Team3.Backend.Models;

public class Message
{
    public Guid Id { get; set; }

    public Guid ConversationId { get; set; }

    public Guid SenderUserId { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Conversation Conversation { get; set; } = null!;

    public User SenderUser { get; set; } = null!;
}
