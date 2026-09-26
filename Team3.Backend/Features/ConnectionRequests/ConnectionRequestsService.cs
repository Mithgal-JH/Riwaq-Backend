using Team3.Backend.Features.ConnectionRequests.Dtos;
using Team3.Backend.Features.ConnectionRequests.Interfaces;
using Team3.Backend.Features.Notifications;
using Team3.Backend.Features.Notifications.Interfaces;
using Team3.Backend.Features.Users.Dtos;
using Team3.Backend.Models;

namespace Team3.Backend.Features.ConnectionRequests;

public class ConnectionRequestsService : IConnectionRequestsService
{
    private readonly IConnectionRequestsRepository _repository;
    private readonly INotificationsService _notificationsService;

    public ConnectionRequestsService(
        IConnectionRequestsRepository repository,
        INotificationsService notificationsService)
    {
        _repository = repository;
        _notificationsService = notificationsService;
    }

    public async Task<ConnectionRequestResponse> SendAsync(
        string firebaseUid,
        SendConnectionRequestRequest request)
    {
        if (string.IsNullOrWhiteSpace(firebaseUid))
        {
            throw new ArgumentException("Firebase UID is required.");
        }

        if (request is null)
        {
            throw new ArgumentException("Request payload is required.");
        }

        if (request.ReceiverUserId == Guid.Empty)
        {
            throw new ArgumentException("ReceiverUserId must be a valid Guid.");
        }

        var sender = await _repository.GetUserByFirebaseUidAsync(firebaseUid)
            ?? throw new KeyNotFoundException("User not found.");

        if (request.ReceiverUserId == sender.Id)
        {
            throw new InvalidOperationException(
                "You cannot send a connection request to yourself.");
        }

        var receiver = await _repository.GetUserByIdAsync(request.ReceiverUserId)
            ?? throw new KeyNotFoundException("Receiver user not found.");

        // Do not create another request if the users are already connected.
        var alreadyConnected = await _repository.AreUsersConnectedAsync(
            sender.Id,
            receiver.Id);

        if (alreadyConnected)
        {
            throw new InvalidOperationException(
                "These users are already connected.");
        }

        var duplicateRequest = await _repository.GetActiveRequestAsync(
            sender.Id,
            receiver.Id);

        if (duplicateRequest is not null)
        {
            throw new InvalidOperationException(
                "A pending connection request already exists between these users.");
        }

        var connectionRequest = new ConnectionRequest
        {
            Id = Guid.NewGuid(),
            SenderUserId = sender.Id,
            ReceiverUserId = receiver.Id,
            SenderUser = sender,
            ReceiverUser = receiver,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repository.Add(connectionRequest);
        await _repository.SaveChangesAsync();

        await _notificationsService.CreateAsync(
            receiver.Id,
            NotificationType.ConnectionRequestReceived,
            "You received a new connection request.",
            connectionRequest.Id);

        return MapToResponse(connectionRequest);
    }

    public async Task<List<ConnectionRequestResponse>> GetReceivedAsync(
        string firebaseUid)
    {
        var user = await _repository.GetUserByFirebaseUidAsync(firebaseUid)
            ?? throw new KeyNotFoundException("User not found.");

        var requests = await _repository.GetReceivedAsync(user.Id);

        return requests
            .Select(request => MapToResponse(request))
            .ToList();
    }

    public async Task<List<ConnectionRequestResponse>> GetSentAsync(
        string firebaseUid)
    {
        var user = await _repository.GetUserByFirebaseUidAsync(firebaseUid)
            ?? throw new KeyNotFoundException("User not found.");

        var requests = await _repository.GetSentAsync(user.Id);

        return requests
    .Select(request => MapToResponse(request))
    .ToList();
    }

    public async Task<ConnectionRequestResponse> UpdateStatusAsync(
        string firebaseUid,
        Guid connectionRequestId,
        UpdateConnectionRequestStatusRequest request)
    {
        if (string.IsNullOrWhiteSpace(firebaseUid))
        {
            throw new ArgumentException("Firebase UID is required.");
        }

        if (request is null)
        {
            throw new ArgumentException("Request payload is required.");
        }

        if (connectionRequestId == Guid.Empty)
        {
            throw new ArgumentException(
                "ConnectionRequestId must be a valid Guid.");
        }

        var currentUser = await _repository.GetUserByFirebaseUidAsync(firebaseUid)
            ?? throw new KeyNotFoundException("User not found.");

        var connectionRequest = await _repository.GetByIdAsync(connectionRequestId)
            ?? throw new KeyNotFoundException("Connection request not found.");

        var requestedStatus = NormalizeStatus(request.Status);
        Guid? conversationId = null;
        if (connectionRequest.SenderUserId == currentUser.Id)
        {
            // The sender can only cancel a pending request.
            if (connectionRequest.Status != "Pending")
            {
                throw new InvalidOperationException(
                    "This connection request can no longer be changed.");
            }

            if (requestedStatus != "Cancelled")
            {
                throw new InvalidOperationException(
                    "Only the sender can cancel this request.");
            }

            connectionRequest.Status = "Cancelled";
        }
        else if (connectionRequest.ReceiverUserId == currentUser.Id)
        {
            // The receiver can only accept or reject a pending request.
            if (connectionRequest.Status != "Pending")
            {
                throw new InvalidOperationException(
                    "This connection request has already been resolved.");
            }

            if (requestedStatus != "Accepted" &&
                requestedStatus != "Rejected")
            {
                throw new InvalidOperationException(
                    "The receiver can only accept or reject this request.");
            }

            connectionRequest.Status = requestedStatus;

            if (requestedStatus == "Accepted")
            {
                // Check whether the users are already connected.
                var alreadyConnected =
                    await _repository.AreUsersConnectedAsync(
                        connectionRequest.SenderUserId,
                        connectionRequest.ReceiverUserId);

                if (!alreadyConnected)
                {
                    // Store users in a consistent order.
                    var userAId = connectionRequest.SenderUserId;
                    var userBId = connectionRequest.ReceiverUserId;

                    if (userAId.CompareTo(userBId) > 0)
                    {
                        (userAId, userBId) = (userBId, userAId);
                    }

                    var connection = new Connection
                    {
                        Id = Guid.NewGuid(),
                        UserAId = userAId,
                        UserBId = userBId,
                        CreatedAt = DateTime.UtcNow
                    };

                    // Create the connection when the request is accepted.
                    _repository.AddConnection(connection);

                    var now = DateTime.UtcNow;

                    var conversation = new Conversation
                    {
                        Id = Guid.NewGuid(),
                        ConnectionId = connection.Id,
                        CreatedAt = now,
                        LastActivityAt = now,
                        Connection = connection
                    };

                    // Create the conversation automatically for the new connection.
                    _repository.AddConversation(conversation);

                    conversationId = conversation.Id;
                }
            }
        }
        else
        {
            // Only the sender or receiver can update the request.
            throw new InvalidOperationException(
                "You are not allowed to update this connection request.");
        }

        connectionRequest.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync();

        if (requestedStatus == "Accepted")
        {
            await _notificationsService.CreateAsync(
                connectionRequest.SenderUserId,
                NotificationType.ConnectionRequestAccepted,
                "Your connection request was accepted.",
                connectionRequest.Id);
        }
        else if (requestedStatus == "Rejected")
        {
            await _notificationsService.CreateAsync(
                connectionRequest.SenderUserId,
                NotificationType.ConnectionRequestRejected,
                "Your connection request was rejected.",
                connectionRequest.Id);
        }

        return MapToResponse(connectionRequest, conversationId);
    }

    private static string NormalizeStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            throw new ArgumentException("Status is required.");
        }

        var trimmedStatus = status.Trim();

        if (trimmedStatus.Equals(
                "pending",
                StringComparison.OrdinalIgnoreCase))
        {
            return "Pending";
        }

        if (trimmedStatus.Equals(
                "accepted",
                StringComparison.OrdinalIgnoreCase))
        {
            return "Accepted";
        }

        if (trimmedStatus.Equals(
                "rejected",
                StringComparison.OrdinalIgnoreCase))
        {
            return "Rejected";
        }

        if (trimmedStatus.Equals(
                "cancelled",
                StringComparison.OrdinalIgnoreCase))
        {
            return "Cancelled";
        }

        throw new ArgumentException(
            "Status must be one of: Pending, Accepted, Rejected, Cancelled.");
    }

    private static ConnectionRequestResponse MapToResponse(
      ConnectionRequest connectionRequest,
      Guid? conversationId = null)
    {
        return new ConnectionRequestResponse
        {
            Id = connectionRequest.Id,
            Sender = MapUserProfile(connectionRequest.SenderUser),
            Receiver = MapUserProfile(connectionRequest.ReceiverUser),
            Status = connectionRequest.Status,
            ConversationId = conversationId,
            CreatedAt = connectionRequest.CreatedAt,
            UpdatedAt = connectionRequest.UpdatedAt
        };
    }

    private static PublicUserProfileResponse MapUserProfile(User user)
    {
        return new PublicUserProfileResponse
        {
            UserId = user.Id,
            Points = user.Points,
            LearningDirectionId = user.LearningDirectionId,
            FirstName = user.Profile?.FirstName,
            LastName = user.Profile?.LastName,
            Bio = user.Profile?.Bio,
            University = user.Profile?.University
        };
    }
}