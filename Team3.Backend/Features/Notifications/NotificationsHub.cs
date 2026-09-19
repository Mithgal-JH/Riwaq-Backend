using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Team3.Backend.Features.Notifications;

[Authorize]
public sealed class NotificationsHub : Hub
{
}
