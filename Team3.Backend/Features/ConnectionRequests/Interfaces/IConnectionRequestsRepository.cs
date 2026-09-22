using Team3.Backend.Models;

namespace Team3.Backend.Features.ConnectionRequests.Interfaces;

public interface IConnectionRequestsRepository
{
    Task<User?> GetUserByFirebaseUidAsync(string firebaseUid);

    Task<User?> GetUserByIdAsync(Guid userId);

    Task<ConnectionRequest?> GetByIdAsync(Guid connectionRequestId);

    Task<ConnectionRequest?> GetActiveRequestAsync(
        Guid senderUserId,
        Guid receiverUserId);

    Task<List<ConnectionRequest>> GetReceivedAsync(Guid receiverUserId);

    Task<List<ConnectionRequest>> GetSentAsync(Guid senderUserId);

    // Check whether two users are already connected.
    Task<bool> AreUsersConnectedAsync(
        Guid userAId,
        Guid userBId);

    // Add a new connection request.
    void Add(ConnectionRequest connectionRequest);

    // Add a new connection after a request is accepted.
    void AddConnection(Connection connection);

    Task SaveChangesAsync();
}