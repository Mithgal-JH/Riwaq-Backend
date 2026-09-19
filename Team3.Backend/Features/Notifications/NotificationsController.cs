using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Team3.Backend.Features.Authentication.Interfaces;
using Team3.Backend.Features.Notifications.Dtos;
using Team3.Backend.Features.Notifications.Interfaces;

namespace Team3.Backend.Features.Notifications;

[ApiController]
[Authorize]
[Route("api/notifications")]
public sealed class NotificationsController : ControllerBase
{
    private readonly INotificationsService _service;
    private readonly ICurrentUserService _currentUserService;

    public NotificationsController(
        INotificationsService service,
        ICurrentUserService currentUserService)
    {
        _service = service;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NotificationResponse>>> GetMine()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        return Ok(await _service.GetMineAsync(userId));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        return Ok(await _service.GetUnreadCountAsync(userId));
    }

    [HttpPatch("{notificationId:guid}")]
    public async Task<ActionResult<NotificationResponse>> Update(
        Guid notificationId,
        UpdateNotificationRequest request)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            return Ok(await _service.MarkAsReadAsync(
                userId,
                notificationId,
                request.IsRead));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpPost("mark-all-as-read")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        await _service.MarkAllAsReadAsync(userId);
        return NoContent();
    }

    [HttpDelete("{notificationId:guid}")]
    public async Task<IActionResult> Delete(Guid notificationId)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            await _service.DeleteAsync(userId, notificationId);
            return NoContent();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    private bool TryGetUserId(out Guid userId)
    {
        userId = _currentUserService.UserId ?? Guid.Empty;
        return userId != Guid.Empty;
    }
}