using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Team3.Backend.Features.Notifications;

/// <summary>
/// Delivers persisted notification events to authenticated users.
/// Connect at <c>/hubs/notifications</c> and listen for
/// <c>NotificationReceived</c>.
/// </summary>
[Authorize]
public sealed class NotificationsHub : Hub
{
}
