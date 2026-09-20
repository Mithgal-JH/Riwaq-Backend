using Team3.Backend.Features.Connections.Dtos;
using Team3.Backend.Features.Connections.Interfaces;
using Team3.Backend.Features.Users.Dtos;
using Team3.Backend.Models;

namespace Team3.Backend.Features.Connections;

public class ConnectionsService : IConnectionsService
{
    private readonly IConnectionsRepository _connectionsRepository;

    public ConnectionsService(IConnectionsRepository connectionsRepository)
    {
        _connectionsRepository = connectionsRepository;
    }

    public async Task<IReadOnlyList<ConnectionResponse>> GetMyConnectionsAsync(
        Guid userId)
    {
        var user = await _connectionsRepository.GetUserByIdAsync(userId);

        if (user is null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        var connections = await _connectionsRepository.GetByUserIdAsync(user.Id);

        return connections
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<ConnectionResponse?> GetByIdAsync(
        Guid userId,
        Guid connectionId)
    {
        if (connectionId == Guid.Empty)
        {
            throw new ArgumentException("Connection id is required.");
        }

        var user = await _connectionsRepository.GetUserByIdAsync(userId);

        if (user is null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        var connection = await _connectionsRepository.GetByIdWithUsersAsync(
            connectionId);

        if (connection is null)
        {
            return null;
        }

        if (connection.UserAId != user.Id && connection.UserBId != user.Id)
        {
            throw new InvalidOperationException(
                "You do not have access to this connection.");
        }

        return MapToResponse(connection);
    }

    public async Task DeleteAsync(
        Guid userId,
        Guid connectionId)
    {
        if (connectionId == Guid.Empty)
        {
            throw new ArgumentException("Connection id is required.");
        }

        var user = await _connectionsRepository.GetUserByIdAsync(userId);

        if (user is null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        var connection = await _connectionsRepository.GetByIdWithUsersAsync(
            connectionId);

        if (connection is null)
        {
            throw new KeyNotFoundException("Connection not found.");
        }

        if (connection.UserAId != user.Id && connection.UserBId != user.Id)
        {
            throw new InvalidOperationException(
                "You do not have access to this connection.");
        }

        _connectionsRepository.Remove(connection);
        await _connectionsRepository.SaveChangesAsync();
    }

    private static ConnectionResponse MapToResponse(Connection connection)
    {
        return new ConnectionResponse
        {
            Id = connection.Id,
            UserAId = connection.UserAId,
            UserBId = connection.UserBId,
            CreatedAt = connection.CreatedAt,
            UserA = MapUser(connection.UserA),
            UserB = MapUser(connection.UserB)
        };
    }

    private static PublicUserProfileResponse MapUser(User user)
    {
        return new PublicUserProfileResponse
        {
            UserId = user.Id,
            Points = user.Points,
            LearningDirectionId = user.LearningDirectionId,
            LearningDirectionName = user.SelectedSkill?.Name,
            FirstName = user.Profile?.FirstName,
            LastName = user.Profile?.LastName,
            Bio = user.Profile?.Bio,
            University = user.Profile?.University
        };
    }
}
