using Microsoft.EntityFrameworkCore;
using Team3.Backend.Data;
using Team3.Backend.Features.Connections.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Features.Connections;

public class ConnectionsRepository : IConnectionsRepository
{
    private readonly AppDbContext _context;

    public ConnectionsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _context.Users
            .Include(user => user.Profile)
            .FirstOrDefaultAsync(user => user.Id == userId);
    }

    public async Task<List<Connection>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Connections
            .AsNoTracking()
            .Include(connection => connection.UserA)
                .ThenInclude(user => user.Profile)
            .Include(connection => connection.UserB)
                .ThenInclude(user => user.Profile)
            .Where(connection =>
                connection.UserAId == userId ||
                connection.UserBId == userId)
            .OrderByDescending(connection => connection.CreatedAt)
            .ToListAsync();
    }

    public async Task<Connection?> GetByIdWithUsersAsync(Guid connectionId)
    {
        return await _context.Connections
            .Include(connection => connection.UserA)
                .ThenInclude(user => user.Profile)
            .Include(connection => connection.UserB)
                .ThenInclude(user => user.Profile)
            .FirstOrDefaultAsync(connection => connection.Id == connectionId);
    }

    public void Remove(Connection connection)
    {
        _context.Connections.Remove(connection);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
