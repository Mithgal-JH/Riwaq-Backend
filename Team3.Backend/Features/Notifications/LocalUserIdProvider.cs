using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Team3.Backend.Features.Notifications;

public sealed class LocalUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        var value = connection.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(value, out var userId) && userId != Guid.Empty
            ? userId.ToString()
            : null;
    }
}
