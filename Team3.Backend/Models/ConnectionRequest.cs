namespace Team3.Backend.Models;

public class ConnectionRequest
{
    public Guid Id { get; set; }

    public Guid SenderUserId { get; set; }

    public Guid ReceiverUserId { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public User SenderUser { get; set; } = null!;

    public User ReceiverUser { get; set; } = null!;
}
