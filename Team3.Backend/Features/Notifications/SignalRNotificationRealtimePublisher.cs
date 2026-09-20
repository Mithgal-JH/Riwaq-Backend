using Microsoft.AspNetCore.SignalR;
using Team3.Backend.Features.Notifications.Dtos;
using Team3.Backend.Features.Notifications.Interfaces;

namespace Team3.Backend.Features.Notifications;

public sealed class SignalRNotificationRealtimePublisher : INotificationRealtimePublisher
{
    private readonly IHubContext<NotificationsHub> _hubContext;
    private readonly ILogger<SignalRNotificationRealtimePublisher> _logger;

    public SignalRNotificationRealtimePublisher(
        IHubContext<NotificationsHub> hubContext,
        ILogger<SignalRNotificationRealtimePublisher> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task PublishAsync(
        Guid recipientUserId,
        NotificationResponse notification)
    {
        try
        {
            await _hubContext.Clients
                .User(recipientUserId.ToString())
                .SendAsync("NotificationReceived", notification);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Realtime notification delivery failed for local user {UserId} and notification {NotificationId}.",
                recipientUserId,
                notification.Id);
        }
    }
}
