using Team3.Backend.Features.Notifications;

namespace Team3.Backend.Models;

public class Notification
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public NotificationType Type { get; set; }

    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? RelatedEntityId { get; set; }

    public User User { get; set; } = null!;
}
