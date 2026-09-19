using Team3.Backend.Features.Notifications.Dtos;

namespace Team3.Backend.Features.Notifications.Interfaces;

public interface INotificationRealtimePublisher
{
    Task PublishAsync(Guid recipientUserId, NotificationResponse notification);
}
