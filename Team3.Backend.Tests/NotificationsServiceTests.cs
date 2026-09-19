using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Team3.Backend.Data;
using Team3.Backend.Features.Notifications;
using Team3.Backend.Models;

namespace Team3.Backend.Tests;

public sealed class NotificationsServiceTests
{
    [Fact]
    public async Task CreateAsyncStoresNotificationAndPreventsDuplicateEvent()
    {
        await using var context = CreateContext();
        var service = new NotificationsService(context);
        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        await service.CreateAsync(
            userId,
            NotificationType.ConnectionRequestReceived,
            "A request arrived.",
            eventId);
        await service.CreateAsync(
            userId,
            NotificationType.ConnectionRequestReceived,
            "A request arrived again.",
            eventId);

        context.Notifications.Should().ContainSingle();
        context.Notifications.Single().Message.Should().Be("A request arrived.");
    }

    [Fact]
    public async Task GetMineAsyncDoesNotReturnAnotherUsersNotifications()
    {
        await using var context = CreateContext();
        var service = new NotificationsService(context);
        var userId = Guid.NewGuid();

        await service.CreateAsync(userId, NotificationType.CommentReceived, "Mine.", Guid.NewGuid());
        await service.CreateAsync(Guid.NewGuid(), NotificationType.CommentReceived, "Not mine.", Guid.NewGuid());

        var notifications = await service.GetMineAsync(userId);

        notifications.Should().ContainSingle();
        notifications[0].Message.Should().Be("Mine.");
    }

    [Fact]
    public async Task MarkAsReadRejectsNotificationOwnedByAnotherUser()
    {
        await using var context = CreateContext();
        var service = new NotificationsService(context);
        var notificationId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        context.Notifications.Add(new Notification
        {
            Id = notificationId,
            UserId = ownerId,
            Type = NotificationType.CommentReceived,
            Message = "Private.",
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var act = () => service.MarkAsReadAsync(
            Guid.NewGuid(),
            notificationId,
            true);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        context.Notifications.Single().IsRead.Should().BeFalse();
    }

    [Fact]
    public async Task MarkAllAsReadOnlyUpdatesCurrentUsersNotifications()
    {
        await using var context = CreateContext();
        var service = new NotificationsService(context);
        var userId = Guid.NewGuid();

        await service.CreateAsync(userId, NotificationType.CommentReceived, "One.", Guid.NewGuid());
        await service.CreateAsync(userId, NotificationType.ConnectionRequestReceived, "Two.", Guid.NewGuid());
        await service.CreateAsync(Guid.NewGuid(), NotificationType.CommentReceived, "Other.", Guid.NewGuid());

        await service.MarkAllAsReadAsync(userId);

        context.Notifications.Where(notification => notification.UserId == userId)
            .Should().OnlyContain(notification => notification.IsRead);
        context.Notifications.Where(notification => notification.UserId != userId)
            .Should().OnlyContain(notification => !notification.IsRead);
    }

    [Fact]
    public async Task DeleteOnlyDeletesCurrentUsersNotification()
    {
        await using var context = CreateContext();
        var service = new NotificationsService(context);
        var ownerId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();

        context.Notifications.Add(new Notification
        {
            Id = notificationId,
            UserId = ownerId,
            Type = NotificationType.ConnectionRequestAccepted,
            Message = "Accepted.",
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var act = () => service.DeleteAsync(Guid.NewGuid(), notificationId);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        context.Notifications.Should().ContainSingle();
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}