using FluentAssertions;
using Moq;
using Team3.Backend.Features.ConnectionRequests;
using Team3.Backend.Features.ConnectionRequests.Dtos;
using Team3.Backend.Features.ConnectionRequests.Interfaces;
using Team3.Backend.Features.Notifications;
using Team3.Backend.Features.Notifications.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Tests;

public class ConnectionRequestsServiceTests
{
    private readonly Mock<IConnectionRequestsRepository> _repository = new();
    private readonly Mock<INotificationsService> _notificationsService = new();
    private readonly ConnectionRequestsService _service;

    public ConnectionRequestsServiceTests()
    {
        _service = new ConnectionRequestsService(
            _repository.Object,
            _notificationsService.Object);
    }

    [Fact]
    public async Task SendAsync_ShouldCreatePendingRequestAndNotification_WhenDataIsValid()
    {
        var sender = ServiceTestData.User(Guid.NewGuid());
        var receiver = ServiceTestData.User(Guid.NewGuid());

        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("sender-firebase-uid"))
            .ReturnsAsync(sender);

        _repository
            .Setup(x => x.GetUserByIdAsync(receiver.Id))
            .ReturnsAsync(receiver);

        _repository
            .Setup(x => x.AreUsersConnectedAsync(sender.Id, receiver.Id))
            .ReturnsAsync(false);

        _repository
            .Setup(x => x.GetActiveRequestAsync(sender.Id, receiver.Id))
            .ReturnsAsync((ConnectionRequest?)null);

        var request = new SendConnectionRequestRequest
        {
            ReceiverUserId = receiver.Id
        };

        var response = await _service.SendAsync(
            "sender-firebase-uid",
            request);

        response.Status.Should().Be("Pending");
        response.Sender.UserId.Should().Be(sender.Id);
        response.Receiver.UserId.Should().Be(receiver.Id);

        _repository.Verify(
            x => x.Add(It.Is<ConnectionRequest>(connectionRequest =>
                connectionRequest.SenderUserId == sender.Id &&
                connectionRequest.ReceiverUserId == receiver.Id &&
                connectionRequest.Status == "Pending")),
            Times.Once);

        _repository.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);

        _notificationsService.Verify(
            x => x.CreateAsync(
                receiver.Id,
                NotificationType.ConnectionRequestReceived,
                "You received a new connection request.",
                It.IsAny<Guid>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_ShouldThrow_WhenFirebaseUidIsEmpty()
    {
        var request = new SendConnectionRequestRequest
        {
            ReceiverUserId = Guid.NewGuid()
        };

        var action = () => _service.SendAsync(string.Empty, request);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Firebase UID is required.");
    }

    [Fact]
    public async Task SendAsync_ShouldThrow_WhenRequestIsNull()
    {
        var action = () => _service.SendAsync(
            "sender-firebase-uid",
            null!);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Request payload is required.");
    }

    [Fact]
    public async Task SendAsync_ShouldThrow_WhenReceiverUserIdIsEmpty()
    {
        var request = new SendConnectionRequestRequest
        {
            ReceiverUserId = Guid.Empty
        };

        var action = () => _service.SendAsync(
            "sender-firebase-uid",
            request);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("ReceiverUserId must be a valid Guid.");
    }

    [Fact]
    public async Task SendAsync_ShouldThrow_WhenSenderDoesNotExist()
    {
        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("sender-firebase-uid"))
            .ReturnsAsync((User?)null);

        var request = new SendConnectionRequestRequest
        {
            ReceiverUserId = Guid.NewGuid()
        };

        var action = () => _service.SendAsync(
            "sender-firebase-uid",
            request);

        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("User not found.");
    }

    [Fact]
    public async Task SendAsync_ShouldThrow_WhenSenderAndReceiverAreTheSame()
    {
        var sender = ServiceTestData.User(Guid.NewGuid());

        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("sender-firebase-uid"))
            .ReturnsAsync(sender);

        var request = new SendConnectionRequestRequest
        {
            ReceiverUserId = sender.Id
        };

        var action = () => _service.SendAsync(
            "sender-firebase-uid",
            request);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("You cannot send a connection request to yourself.");
    }

    [Fact]
    public async Task SendAsync_ShouldThrow_WhenReceiverDoesNotExist()
    {
        var sender = ServiceTestData.User(Guid.NewGuid());
        var receiverId = Guid.NewGuid();

        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("sender-firebase-uid"))
            .ReturnsAsync(sender);

        _repository
            .Setup(x => x.GetUserByIdAsync(receiverId))
            .ReturnsAsync((User?)null);

        var request = new SendConnectionRequestRequest
        {
            ReceiverUserId = receiverId
        };

        var action = () => _service.SendAsync(
            "sender-firebase-uid",
            request);

        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Receiver user not found.");
    }

    [Fact]
    public async Task SendAsync_ShouldThrow_WhenUsersAreAlreadyConnected()
    {
        var sender = ServiceTestData.User(Guid.NewGuid());
        var receiver = ServiceTestData.User(Guid.NewGuid());

        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("sender-firebase-uid"))
            .ReturnsAsync(sender);

        _repository
            .Setup(x => x.GetUserByIdAsync(receiver.Id))
            .ReturnsAsync(receiver);

        _repository
            .Setup(x => x.AreUsersConnectedAsync(sender.Id, receiver.Id))
            .ReturnsAsync(true);

        var request = new SendConnectionRequestRequest
        {
            ReceiverUserId = receiver.Id
        };

        var action = () => _service.SendAsync(
            "sender-firebase-uid",
            request);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("These users are already connected.");
    }

    [Fact]
    public async Task SendAsync_ShouldThrow_WhenActiveRequestAlreadyExists()
    {
        var sender = ServiceTestData.User(Guid.NewGuid());
        var receiver = ServiceTestData.User(Guid.NewGuid());

        var existingRequest = new ConnectionRequest
        {
            Id = Guid.NewGuid(),
            SenderUserId = sender.Id,
            ReceiverUserId = receiver.Id,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("sender-firebase-uid"))
            .ReturnsAsync(sender);

        _repository
            .Setup(x => x.GetUserByIdAsync(receiver.Id))
            .ReturnsAsync(receiver);

        _repository
            .Setup(x => x.AreUsersConnectedAsync(sender.Id, receiver.Id))
            .ReturnsAsync(false);

        _repository
            .Setup(x => x.GetActiveRequestAsync(sender.Id, receiver.Id))
            .ReturnsAsync(existingRequest);

        var request = new SendConnectionRequestRequest
        {
            ReceiverUserId = receiver.Id
        };

        var action = () => _service.SendAsync(
            "sender-firebase-uid",
            request);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage(
                "A pending connection request already exists between these users.");
    }

    [Fact]
    public async Task GetReceivedAsync_ShouldReturnReceivedRequests()
    {
        var user = ServiceTestData.User(Guid.NewGuid());
        var sender = ServiceTestData.User(Guid.NewGuid());

        var connectionRequest = CreateConnectionRequest(
            sender,
            user,
            "Pending");

        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("receiver-firebase-uid"))
            .ReturnsAsync(user);

        _repository
            .Setup(x => x.GetReceivedAsync(user.Id))
            .ReturnsAsync(new List<ConnectionRequest>
            {
                connectionRequest
            });

        var response = await _service.GetReceivedAsync(
            "receiver-firebase-uid");

        response.Should().HaveCount(1);
        response[0].Id.Should().Be(connectionRequest.Id);
        response[0].Sender.UserId.Should().Be(sender.Id);
        response[0].Receiver.UserId.Should().Be(user.Id);
        response[0].Status.Should().Be("Pending");
    }

    [Fact]
    public async Task GetReceivedAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("receiver-firebase-uid"))
            .ReturnsAsync((User?)null);

        var action = () => _service.GetReceivedAsync(
            "receiver-firebase-uid");

        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("User not found.");
    }

    [Fact]
    public async Task GetSentAsync_ShouldReturnSentRequests()
    {
        var sender = ServiceTestData.User(Guid.NewGuid());
        var receiver = ServiceTestData.User(Guid.NewGuid());

        var connectionRequest = CreateConnectionRequest(
            sender,
            receiver,
            "Pending");

        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("sender-firebase-uid"))
            .ReturnsAsync(sender);

        _repository
            .Setup(x => x.GetSentAsync(sender.Id))
            .ReturnsAsync(new List<ConnectionRequest>
            {
                connectionRequest
            });

        var response = await _service.GetSentAsync(
            "sender-firebase-uid");

        response.Should().HaveCount(1);
        response[0].Id.Should().Be(connectionRequest.Id);
        response[0].Sender.UserId.Should().Be(sender.Id);
        response[0].Receiver.UserId.Should().Be(receiver.Id);
        response[0].Status.Should().Be("Pending");
    }

    [Fact]
    public async Task GetSentAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("sender-firebase-uid"))
            .ReturnsAsync((User?)null);

        var action = () => _service.GetSentAsync(
            "sender-firebase-uid");

        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("User not found.");
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldCancelRequest_WhenSenderCancelsPendingRequest()
    {
        var sender = ServiceTestData.User(Guid.NewGuid());
        var receiver = ServiceTestData.User(Guid.NewGuid());

        var connectionRequest = CreateConnectionRequest(
            sender,
            receiver,
            "Pending");

        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("sender-firebase-uid"))
            .ReturnsAsync(sender);

        _repository
            .Setup(x => x.GetByIdAsync(connectionRequest.Id))
            .ReturnsAsync(connectionRequest);

        var request = new UpdateConnectionRequestStatusRequest
        {
            Status = "Cancelled"
        };

        var response = await _service.UpdateStatusAsync(
            "sender-firebase-uid",
            connectionRequest.Id,
            request);

        response.Status.Should().Be("Cancelled");
        connectionRequest.Status.Should().Be("Cancelled");

        _repository.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);

        _notificationsService.Verify(
            x => x.CreateAsync(
                It.IsAny<Guid>(),
                It.IsAny<NotificationType>(),
                It.IsAny<string>(),
                It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldThrow_WhenSenderTriesToAccept()
    {
        var sender = ServiceTestData.User(Guid.NewGuid());
        var receiver = ServiceTestData.User(Guid.NewGuid());

        var connectionRequest = CreateConnectionRequest(
            sender,
            receiver,
            "Pending");

        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("sender-firebase-uid"))
            .ReturnsAsync(sender);

        _repository
            .Setup(x => x.GetByIdAsync(connectionRequest.Id))
            .ReturnsAsync(connectionRequest);

        var request = new UpdateConnectionRequestStatusRequest
        {
            Status = "Accepted"
        };

        var action = () => _service.UpdateStatusAsync(
            "sender-firebase-uid",
            connectionRequest.Id,
            request);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Only the sender can cancel this request.");
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldAcceptRequestAndCreateConnection_WhenReceiverAccepts()
    {
        var sender = ServiceTestData.User(Guid.NewGuid());
        var receiver = ServiceTestData.User(Guid.NewGuid());

        var connectionRequest = CreateConnectionRequest(
            sender,
            receiver,
            "Pending");

        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("receiver-firebase-uid"))
            .ReturnsAsync(receiver);

        _repository
            .Setup(x => x.GetByIdAsync(connectionRequest.Id))
            .ReturnsAsync(connectionRequest);

        _repository
            .Setup(x => x.AreUsersConnectedAsync(
                sender.Id,
                receiver.Id))
            .ReturnsAsync(false);

        var request = new UpdateConnectionRequestStatusRequest
        {
            Status = "Accepted"
        };

        var response = await _service.UpdateStatusAsync(
            "receiver-firebase-uid",
            connectionRequest.Id,
            request);

        response.Status.Should().Be("Accepted");
        connectionRequest.Status.Should().Be("Accepted");

        // The service stores UserA and UserB in a consistent Guid order.
        _repository.Verify(x => x.AddConnection(It.Is<Connection>(connection =>
            (connection.UserAId == sender.Id && connection.UserBId == receiver.Id) ||
            (connection.UserAId == receiver.Id && connection.UserBId == sender.Id))), Times.Once);

        _repository.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);

        _notificationsService.Verify(
            x => x.CreateAsync(
                sender.Id,
                NotificationType.ConnectionRequestAccepted,
                "Your connection request was accepted.",
                connectionRequest.Id),
            Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldNotCreateDuplicateConnection_WhenAlreadyConnected()
    {
        var sender = ServiceTestData.User(Guid.NewGuid());
        var receiver = ServiceTestData.User(Guid.NewGuid());

        var connectionRequest = CreateConnectionRequest(
            sender,
            receiver,
            "Pending");

        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("receiver-firebase-uid"))
            .ReturnsAsync(receiver);

        _repository
            .Setup(x => x.GetByIdAsync(connectionRequest.Id))
            .ReturnsAsync(connectionRequest);

        _repository
            .Setup(x => x.AreUsersConnectedAsync(
                sender.Id,
                receiver.Id))
            .ReturnsAsync(true);

        var request = new UpdateConnectionRequestStatusRequest
        {
            Status = "Accepted"
        };

        var response = await _service.UpdateStatusAsync(
            "receiver-firebase-uid",
            connectionRequest.Id,
            request);

        response.Status.Should().Be("Accepted");

        _repository.Verify(
            x => x.AddConnection(It.IsAny<Connection>()),
            Times.Never);

        _repository.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);

        _notificationsService.Verify(
            x => x.CreateAsync(
                sender.Id,
                NotificationType.ConnectionRequestAccepted,
                "Your connection request was accepted.",
                connectionRequest.Id),
            Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldRejectRequestAndNotifySender_WhenReceiverRejects()
    {
        var sender = ServiceTestData.User(Guid.NewGuid());
        var receiver = ServiceTestData.User(Guid.NewGuid());

        var connectionRequest = CreateConnectionRequest(
            sender,
            receiver,
            "Pending");

        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("receiver-firebase-uid"))
            .ReturnsAsync(receiver);

        _repository
            .Setup(x => x.GetByIdAsync(connectionRequest.Id))
            .ReturnsAsync(connectionRequest);

        var request = new UpdateConnectionRequestStatusRequest
        {
            Status = "Rejected"
        };

        var response = await _service.UpdateStatusAsync(
            "receiver-firebase-uid",
            connectionRequest.Id,
            request);

        response.Status.Should().Be("Rejected");
        connectionRequest.Status.Should().Be("Rejected");

        _repository.Verify(
            x => x.AddConnection(It.IsAny<Connection>()),
            Times.Never);

        _repository.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);

        _notificationsService.Verify(
            x => x.CreateAsync(
                sender.Id,
                NotificationType.ConnectionRequestRejected,
                "Your connection request was rejected.",
                connectionRequest.Id),
            Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldThrow_WhenRequestIsAlreadyResolved()
    {
        var sender = ServiceTestData.User(Guid.NewGuid());
        var receiver = ServiceTestData.User(Guid.NewGuid());

        var connectionRequest = CreateConnectionRequest(
            sender,
            receiver,
            "Rejected");

        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("receiver-firebase-uid"))
            .ReturnsAsync(receiver);

        _repository
            .Setup(x => x.GetByIdAsync(connectionRequest.Id))
            .ReturnsAsync(connectionRequest);

        var request = new UpdateConnectionRequestStatusRequest
        {
            Status = "Accepted"
        };

        var action = () => _service.UpdateStatusAsync(
            "receiver-firebase-uid",
            connectionRequest.Id,
            request);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("This connection request has already been resolved.");
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldThrow_WhenUserIsNotSenderOrReceiver()
    {
        var sender = ServiceTestData.User(Guid.NewGuid());
        var receiver = ServiceTestData.User(Guid.NewGuid());
        var otherUser = ServiceTestData.User(Guid.NewGuid());

        var connectionRequest = CreateConnectionRequest(
            sender,
            receiver,
            "Pending");

        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("other-firebase-uid"))
            .ReturnsAsync(otherUser);

        _repository
            .Setup(x => x.GetByIdAsync(connectionRequest.Id))
            .ReturnsAsync(connectionRequest);

        var request = new UpdateConnectionRequestStatusRequest
        {
            Status = "Accepted"
        };

        var action = () => _service.UpdateStatusAsync(
            "other-firebase-uid",
            connectionRequest.Id,
            request);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage(
                "You are not allowed to update this connection request.");
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldThrow_WhenFirebaseUidIsEmpty()
    {
        var request = new UpdateConnectionRequestStatusRequest
        {
            Status = "Accepted"
        };

        var action = () => _service.UpdateStatusAsync(
            string.Empty,
            Guid.NewGuid(),
            request);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Firebase UID is required.");
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldThrow_WhenRequestIsNull()
    {
        var action = () => _service.UpdateStatusAsync(
            "firebase-uid",
            Guid.NewGuid(),
            null!);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Request payload is required.");
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldThrow_WhenConnectionRequestIdIsEmpty()
    {
        var request = new UpdateConnectionRequestStatusRequest
        {
            Status = "Accepted"
        };

        var action = () => _service.UpdateStatusAsync(
            "firebase-uid",
            Guid.Empty,
            request);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("ConnectionRequestId must be a valid Guid.");
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("firebase-uid"))
            .ReturnsAsync((User?)null);

        var request = new UpdateConnectionRequestStatusRequest
        {
            Status = "Accepted"
        };

        var action = () => _service.UpdateStatusAsync(
            "firebase-uid",
            Guid.NewGuid(),
            request);

        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("User not found.");
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldThrow_WhenConnectionRequestDoesNotExist()
    {
        var user = ServiceTestData.User(Guid.NewGuid());
        var requestId = Guid.NewGuid();

        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("firebase-uid"))
            .ReturnsAsync(user);

        _repository
            .Setup(x => x.GetByIdAsync(requestId))
            .ReturnsAsync((ConnectionRequest?)null);

        var request = new UpdateConnectionRequestStatusRequest
        {
            Status = "Accepted"
        };

        var action = () => _service.UpdateStatusAsync(
            "firebase-uid",
            requestId,
            request);

        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Connection request not found.");
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldThrow_WhenStatusIsEmpty()
    {
        var user = ServiceTestData.User(Guid.NewGuid());
        var connectionRequest = CreateConnectionRequest(
            user,
            ServiceTestData.User(Guid.NewGuid()),
            "Pending");

        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("firebase-uid"))
            .ReturnsAsync(user);

        _repository
            .Setup(x => x.GetByIdAsync(connectionRequest.Id))
            .ReturnsAsync(connectionRequest);

        var request = new UpdateConnectionRequestStatusRequest
        {
            Status = string.Empty
        };

        var action = () => _service.UpdateStatusAsync(
            "firebase-uid",
            connectionRequest.Id,
            request);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Status is required.");
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldThrow_WhenStatusIsInvalid()
    {
        var user = ServiceTestData.User(Guid.NewGuid());
        var connectionRequest = CreateConnectionRequest(
            user,
            ServiceTestData.User(Guid.NewGuid()),
            "Pending");

        _repository
            .Setup(x => x.GetUserByFirebaseUidAsync("firebase-uid"))
            .ReturnsAsync(user);

        _repository
            .Setup(x => x.GetByIdAsync(connectionRequest.Id))
            .ReturnsAsync(connectionRequest);

        var request = new UpdateConnectionRequestStatusRequest
        {
            Status = "Unknown"
        };

        var action = () => _service.UpdateStatusAsync(
            "firebase-uid",
            connectionRequest.Id,
            request);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage(
                "Status must be one of: Pending, Accepted, Rejected, Cancelled.");
    }

    // Create a connection request for the test scenarios.
    private static ConnectionRequest CreateConnectionRequest(
        User sender,
        User receiver,
        string status)
    {
        return new ConnectionRequest
        {
            Id = Guid.NewGuid(),
            SenderUserId = sender.Id,
            ReceiverUserId = receiver.Id,
            SenderUser = sender,
            ReceiverUser = receiver,
            Status = status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}