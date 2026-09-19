using Team3.Backend.Features.Connections.Dtos;

namespace Team3.Backend.Features.Connections.Interfaces;

public interface IConnectionsService
{
    Task<IReadOnlyList<ConnectionResponse>> GetMyConnectionsAsync(
        Guid userId);

    Task<ConnectionResponse?> GetByIdAsync(
        Guid userId,
        Guid connectionId);

    Task DeleteAsync(
        Guid userId,
        Guid connectionId);
}
