namespace Team3.Backend.Models;

public class Conversation
{
    public Guid Id { get; set; }

    public Guid ConnectionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Connection Connection { get; set; } = null!;

    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
