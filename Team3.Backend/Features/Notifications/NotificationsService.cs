using Microsoft.EntityFrameworkCore;
using Team3.Backend.Data;
using Team3.Backend.Features.Notifications.Dtos;
using Team3.Backend.Features.Notifications.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Features.Notifications;

public sealed class NotificationsService : INotificationsService
{
    private readonly AppDbContext _context;

    public NotificationsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<NotificationResponse>> GetMineAsync(Guid userId)
    {
        EnsureUserId(userId);

        return await _context.Notifications
            .AsNoTracking()
            .Where(notification => notification.UserId == userId)
            .OrderByDescending(notification => notification.CreatedAt)
            .Select(notification => new NotificationResponse
            {
                Id = notification.Id,
                Type = notification.Type.ToString(),
                Message = notification.Message,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                RelatedEntityId = notification.RelatedEntityId
            })
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        EnsureUserId(userId);
        return await _context.Notifications.CountAsync(notification =>
            notification.UserId == userId && !notification.IsRead);
    }

    public async Task<NotificationResponse> MarkAsReadAsync(
        Guid userId,
        Guid notificationId,
        bool isRead)
    {
        var notification = await GetForUserAsync(userId, notificationId);
        notification.IsRead = isRead;
        await _context.SaveChangesAsync();
        return Map(notification);
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        EnsureUserId(userId);
        var notifications = await _context.Notifications
            .Where(notification => notification.UserId == userId && !notification.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid userId, Guid notificationId)
    {
        var notification = await GetForUserAsync(userId, notificationId);
        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();
    }

    public async Task CreateAsync(
        Guid userId,
        NotificationType type,
        string message,
        Guid? relatedEntityId = null)
    {
        EnsureUserId(userId);

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Notification message is required.");
        }

        var exists = await _context.Notifications.AnyAsync(notification =>
            notification.UserId == userId
            && notification.Type == type
            && notification.RelatedEntityId == relatedEntityId);

        if (exists)
        {
            return;
        }

        _context.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Message = message.Trim(),
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            RelatedEntityId = relatedEntityId
        });

        await _context.SaveChangesAsync();
    }

    private async Task<Notification> GetForUserAsync(Guid userId, Guid notificationId)
    {
        EnsureUserId(userId);

        if (notificationId == Guid.Empty)
        {
            throw new ArgumentException("Notification ID is required.");
        }

        return await _context.Notifications.FirstOrDefaultAsync(notification =>
                   notification.Id == notificationId && notification.UserId == userId)
               ?? throw new KeyNotFoundException("Notification not found.");
    }

    private static NotificationResponse Map(Notification notification) => new()
    {
        Id = notification.Id,
        Type = notification.Type.ToString(),
        Message = notification.Message,
        IsRead = notification.IsRead,
        CreatedAt = notification.CreatedAt,
        RelatedEntityId = notification.RelatedEntityId
    };

    private static void EnsureUserId(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User ID is required.");
        }
    }
}