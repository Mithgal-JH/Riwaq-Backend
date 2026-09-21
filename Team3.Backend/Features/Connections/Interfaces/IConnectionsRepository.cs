using Team3.Backend.Models;

namespace Team3.Backend.Features.Connections.Interfaces;

public interface IConnectionsRepository
{
    Task<User?> GetUserByIdAsync(Guid userId);

    Task<List<Connection>> GetByUserIdAsync(Guid userId);

    Task<Connection?> GetByIdWithUsersAsync(Guid connectionId);

    void Remove(Connection connection);

    Task SaveChangesAsync();
}
