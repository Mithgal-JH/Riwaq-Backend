using Microsoft.EntityFrameworkCore;
using Team3.Backend.Data;
using Team3.Backend.Features.ConnectionRequests.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Features.ConnectionRequests;

public class ConnectionRequestsRepository : IConnectionRequestsRepository
{
    private readonly AppDbContext _context;

    public ConnectionRequestsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByFirebaseUidAsync(string firebaseUid)
    {
        // Load the user's profile with the user.
        return await _context.Users
            .Include(user => user.Profile)
            .FirstOrDefaultAsync(user => user.FirebaseUid == firebaseUid);
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        // Load the user's profile with the user.
        return await _context.Users
            .Include(user => user.Profile)
            .FirstOrDefaultAsync(user => user.Id == userId);
    }

    public async Task<ConnectionRequest?> GetByIdAsync(Guid connectionRequestId)
    {
        // Load the sender and receiver with their profiles.
        return await _context.ConnectionRequests
            .Include(request => request.SenderUser)
                .ThenInclude(user => user.Profile)
            .Include(request => request.ReceiverUser)
                .ThenInclude(user => user.Profile)
            .FirstOrDefaultAsync(request => request.Id == connectionRequestId);
    }

    public async Task<ConnectionRequest?> GetActiveRequestAsync(
        Guid senderUserId,
        Guid receiverUserId)
    {
        return await _context.ConnectionRequests
            .FirstOrDefaultAsync(request =>
                request.Status == "Pending" &&
                (
                    (request.SenderUserId == senderUserId &&
                     request.ReceiverUserId == receiverUserId)
                    ||
                    (request.SenderUserId == receiverUserId &&
                     request.ReceiverUserId == senderUserId)
                ));
    }

    public async Task<bool> AreUsersConnectedAsync(
        Guid userAId,
        Guid userBId)
    {
        // Check both possible user directions.
        return await _context.Connections.AnyAsync(connection =>
            (connection.UserAId == userAId && connection.UserBId == userBId) ||
            (connection.UserAId == userBId && connection.UserBId == userAId));
    }

    public async Task<List<ConnectionRequest>> GetReceivedAsync(
        Guid receiverUserId)
    {
        // Load received requests with sender and receiver profiles.
        return await _context.ConnectionRequests
            .AsNoTracking()
            .Include(request => request.SenderUser)
                .ThenInclude(user => user.Profile)
            .Include(request => request.ReceiverUser)
                .ThenInclude(user => user.Profile)
            .Where(request => request.ReceiverUserId == receiverUserId)
            .OrderByDescending(request => request.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<ConnectionRequest>> GetSentAsync(
        Guid senderUserId)
    {
        // Load sent requests with sender and receiver profiles.
        return await _context.ConnectionRequests
            .AsNoTracking()
            .Include(request => request.SenderUser)
                .ThenInclude(user => user.Profile)
            .Include(request => request.ReceiverUser)
                .ThenInclude(user => user.Profile)
            .Where(request => request.SenderUserId == senderUserId)
            .OrderByDescending(request => request.CreatedAt)
            .ToListAsync();
    }

    public void Add(ConnectionRequest connectionRequest)
    {
        _context.ConnectionRequests.Add(connectionRequest);
    }

    public void AddConnection(Connection connection)
    {
        // Add the new connection to the database context.
        _context.Connections.Add(connection);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}