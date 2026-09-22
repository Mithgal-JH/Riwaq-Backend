namespace Team3.Backend.Features.Notifications.Dtos;

public sealed class NotificationResponse
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public bool IsRead { get; init; }
    public DateTime CreatedAt { get; init; }
    public Guid? RelatedEntityId { get; init; }
}