using Team3.Backend.Features.Notifications.Dtos;

namespace Team3.Backend.Features.Notifications.Interfaces;

public interface INotificationsService
{
    Task<IReadOnlyList<NotificationResponse>> GetMineAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task<NotificationResponse> MarkAsReadAsync(Guid userId, Guid notificationId, bool isRead);
    Task MarkAllAsReadAsync(Guid userId);
    Task DeleteAsync(Guid userId, Guid notificationId);
    Task CreateAsync(Guid userId, NotificationType type, string message, Guid? relatedEntityId = null);
}