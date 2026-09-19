using FluentAssertions;
using Moq;
using Team3.Backend.Features.Comments;
using Team3.Backend.Features.Comments.DTOs;
using Team3.Backend.Features.Comments.Interfaces;
using Team3.Backend.Features.ConnectionRequests;
using Team3.Backend.Features.ConnectionRequests.Dtos;
using Team3.Backend.Features.ConnectionRequests.Interfaces;
using Team3.Backend.Features.Notifications;
using Team3.Backend.Features.Notifications.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Tests;

public sealed class NotificationProducerTests
{
    [Fact]
    public async Task SendingConnectionRequestNotifiesReceiver()
    {
        var repository = new Mock<IConnectionRequestsRepository>();
        var notifications = new Mock<INotificationsService>();
        var sender = ServiceTestData.User(Guid.NewGuid());
        var receiver = ServiceTestData.User(Guid.NewGuid());
        var request = new SendConnectionRequestRequest
        {
            ReceiverUserId = receiver.Id
        };

        repository.Setup(item => item.GetUserByFirebaseUidAsync(sender.FirebaseUid))
            .ReturnsAsync(sender);
        repository.Setup(item => item.GetUserByIdAsync(receiver.Id))
            .ReturnsAsync(receiver);
        repository.Setup(item => item.AreUsersConnectedAsync(sender.Id, receiver.Id))
            .ReturnsAsync(false);
        repository.Setup(item => item.GetActiveRequestAsync(sender.Id, receiver.Id))
            .ReturnsAsync((ConnectionRequest?)null);

        var service = new ConnectionRequestsService(
            repository.Object,
            notifications.Object);

        await service.SendAsync(sender.FirebaseUid, request);

        notifications.Verify(item => item.CreateAsync(
            receiver.Id,
            NotificationType.ConnectionRequestReceived,
            It.IsAny<string>(),
            It.IsAny<Guid?>()), Times.Once);
    }

    [Fact]
    public async Task AcceptingConnectionRequestNotifiesSender()
    {
        var repository = new Mock<IConnectionRequestsRepository>();
        var notifications = new Mock<INotificationsService>();
        var sender = ServiceTestData.User(Guid.NewGuid());
        var receiver = ServiceTestData.User(Guid.NewGuid());
        var connectionRequest = new ConnectionRequest
        {
            Id = Guid.NewGuid(),
            SenderUserId = sender.Id,
            ReceiverUserId = receiver.Id,
            SenderUser = sender,
            ReceiverUser = receiver,
            Status = "Pending"
        };

        repository.Setup(item => item.GetUserByFirebaseUidAsync(receiver.FirebaseUid))
            .ReturnsAsync(receiver);
        repository.Setup(item => item.GetByIdAsync(connectionRequest.Id))
            .ReturnsAsync(connectionRequest);
        repository.Setup(item => item.AreUsersConnectedAsync(sender.Id, receiver.Id))
            .ReturnsAsync(false);

        var service = new ConnectionRequestsService(
            repository.Object,
            notifications.Object);

        await service.UpdateStatusAsync(
            receiver.FirebaseUid,
            connectionRequest.Id,
            new UpdateConnectionRequestStatusRequest { Status = "Accepted" });

        notifications.Verify(item => item.CreateAsync(
            sender.Id,
            NotificationType.ConnectionRequestAccepted,
            It.IsAny<string>(),
            connectionRequest.Id), Times.Once);
    }

    [Fact]
    public async Task CommentingOnContentNotifiesContentOwner()
    {
        var repository = new Mock<ICommentsRepository>();
        var notifications = new Mock<INotificationsService>();
        var commenter = ServiceTestData.User(Guid.NewGuid());
        var owner = ServiceTestData.User(Guid.NewGuid());
        var contentId = Guid.NewGuid();

        repository.Setup(item => item.EducationalContentExistsAsync(contentId))
            .ReturnsAsync(true);
        repository.Setup(item => item.GetUserByFirebaseUidAsync(commenter.FirebaseUid))
            .ReturnsAsync(commenter);
        repository.Setup(item => item.GetEducationalContentOwnerIdAsync(contentId))
            .ReturnsAsync(owner.Id);

        var service = new CommentsService(
            repository.Object,
            notifications.Object);

        await service.CreateAsync(
            commenter.FirebaseUid,
            contentId,
            new CreateCommentRequest { Content = "Useful explanation." });

        notifications.Verify(item => item.CreateAsync(
            owner.Id,
            NotificationType.CommentReceived,
            It.IsAny<string>(),
            It.IsAny<Guid?>()), Times.Once);
    }
}