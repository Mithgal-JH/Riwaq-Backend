
using FluentAssertions;
using Moq;
using Team3.Backend.Features.Connections;
using Team3.Backend.Features.Connections.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Tests;

public class ConnectionsServiceTests
{
    // Mock the repository so the tests do not use the real database.
    private readonly Mock<IConnectionsRepository> _repository = new();

    // Service under test.
    private readonly ConnectionsService _service;

    public ConnectionsServiceTests()
    {
        // Create the service using the mocked repository.
        _service = new ConnectionsService(_repository.Object);
    }

    [Fact]
    public async Task GetMyConnectionsAsync_ShouldReturnUserConnections()
    {
        // Create the current user and the other user.
        var user = ServiceTestData.User(Guid.NewGuid());
        var otherUser = ServiceTestData.User(Guid.NewGuid());

        // Create a connection between the two users.
        var connection = CreateConnection(
            user.Id,
            otherUser.Id,
            user,
            otherUser);

        // Return the current user from the repository.
        _repository
            .Setup(x => x.GetUserByIdAsync(user.Id))
            .ReturnsAsync(user);

        // Return the user's connections.
        _repository
            .Setup(x => x.GetByUserIdAsync(user.Id))
            .ReturnsAsync(new List<Connection> { connection });

        // Call the service method.
        var response = await _service.GetMyConnectionsAsync(user.Id);

        // Verify that the connection was returned.
        response.Should().HaveCount(1);
        response[0].Id.Should().Be(connection.Id);
        response[0].UserAId.Should().Be(user.Id);
        response[0].UserBId.Should().Be(otherUser.Id);
    }

    [Fact]
    public async Task GetMyConnectionsAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();

        // Return null when the user does not exist.
        _repository
            .Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Call the service method.
        var action = () =>
            _service.GetMyConnectionsAsync(userId);

        // Verify that the service throws when the user is not found.
        await action.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("User not found.");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnConnection_WhenUserBelongsToConnection()
    {
        // Create the current user and the other user.
        var user = ServiceTestData.User(Guid.NewGuid());
        var otherUser = ServiceTestData.User(Guid.NewGuid());

        // Create a connection between the users.
        var connection = CreateConnection(
            user.Id,
            otherUser.Id,
            user,
            otherUser);

        // Return the current user.
        _repository
            .Setup(x => x.GetUserByIdAsync(user.Id))
            .ReturnsAsync(user);

        // Return the requested connection.
        _repository
            .Setup(x => x.GetByIdWithUsersAsync(connection.Id))
            .ReturnsAsync(connection);

        // Get the connection for the current user.
        var response = await _service.GetByIdAsync(
            user.Id,
            connection.Id);

        // Verify that the correct connection was returned.
        response.Should().NotBeNull();
        response!.Id.Should().Be(connection.Id);
        response.UserAId.Should().Be(user.Id);
        response.UserBId.Should().Be(otherUser.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrow_WhenConnectionIdIsEmpty()
    {
        var userId = Guid.NewGuid();

        // Call the service with an empty connection ID.
        var action = () =>
            _service.GetByIdAsync(userId, Guid.Empty);

        // Verify that the ID validation works.
        await action.Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("Connection id is required.");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();
        var connectionId = Guid.NewGuid();

        // Return null because the user does not exist.
        _repository
            .Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Call the service method.
        var action = () =>
            _service.GetByIdAsync(userId, connectionId);

        // Verify that the service throws when the user is not found.
        await action.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("User not found.");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenConnectionDoesNotExist()
    {
        var user = ServiceTestData.User(Guid.NewGuid());
        var connectionId = Guid.NewGuid();

        // Return the current user.
        _repository
            .Setup(x => x.GetUserByIdAsync(user.Id))
            .ReturnsAsync(user);

        // Return null because the connection does not exist.
        _repository
            .Setup(x => x.GetByIdWithUsersAsync(connectionId))
            .ReturnsAsync((Connection?)null);

        // Call the service method.
        var response = await _service.GetByIdAsync(
            user.Id,
            connectionId);

        // Verify that no connection was returned.
        response.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrow_WhenUserDoesNotBelongToConnection()
    {
        // Create two users who belong to the connection.
        var user = ServiceTestData.User(Guid.NewGuid());
        var otherUser = ServiceTestData.User(Guid.NewGuid());

        // Create a third user who does not belong to the connection.
        var unauthorizedUser = ServiceTestData.User(Guid.NewGuid());

        // Create a connection between the first two users.
        var connection = CreateConnection(
            user.Id,
            otherUser.Id,
            user,
            otherUser);

        // Return the unauthorized user.
        _repository
            .Setup(x => x.GetUserByIdAsync(unauthorizedUser.Id))
            .ReturnsAsync(unauthorizedUser);

        // Return the existing connection.
        _repository
            .Setup(x => x.GetByIdWithUsersAsync(connection.Id))
            .ReturnsAsync(connection);

        // Try to access the connection as a user who is not a participant.
        var action = () =>
            _service.GetByIdAsync(
                unauthorizedUser.Id,
                connection.Id);

        // Verify that access is denied.
        await action.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("You do not have access to this connection.");
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteConnection_WhenUserBelongsToConnection()
    {
        // Create the current user and the other user.
        var user = ServiceTestData.User(Guid.NewGuid());
        var otherUser = ServiceTestData.User(Guid.NewGuid());

        // Create their connection.
        var connection = CreateConnection(
            user.Id,
            otherUser.Id,
            user,
            otherUser);

        // Return the current user.
        _repository
            .Setup(x => x.GetUserByIdAsync(user.Id))
            .ReturnsAsync(user);

        // Return the connection to be deleted.
        _repository
            .Setup(x => x.GetByIdWithUsersAsync(connection.Id))
            .ReturnsAsync(connection);

        // Delete the connection.
        var action = () =>
            _service.DeleteAsync(
                user.Id,
                connection.Id);

        // Verify that the operation completed successfully.
        await action.Should().NotThrowAsync();

        // Verify that the connection was removed.
        _repository.Verify(
            x => x.Remove(connection),
            Times.Once);

        // Verify that the changes were saved.
        _repository.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenConnectionIdIsEmpty()
    {
        var userId = Guid.NewGuid();

        // Try to delete a connection using an empty ID.
        var action = () =>
            _service.DeleteAsync(
                userId,
                Guid.Empty);

        // Verify that the ID validation works.
        await action.Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("Connection id is required.");
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();
        var connectionId = Guid.NewGuid();

        // Return null because the user does not exist.
        _repository
            .Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Try to delete the connection.
        var action = () =>
            _service.DeleteAsync(
                userId,
                connectionId);

        // Verify that the service throws when the user is not found.
        await action.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("User not found.");
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenConnectionDoesNotExist()
    {
        var user = ServiceTestData.User(Guid.NewGuid());
        var connectionId = Guid.NewGuid();

        // Return the current user.
        _repository
            .Setup(x => x.GetUserByIdAsync(user.Id))
            .ReturnsAsync(user);

        // Return null because the connection does not exist.
        _repository
            .Setup(x => x.GetByIdWithUsersAsync(connectionId))
            .ReturnsAsync((Connection?)null);

        // Try to delete the connection.
        var action = () =>
            _service.DeleteAsync(
                user.Id,
                connectionId);

        // Verify that the service reports the missing connection.
        await action.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Connection not found.");
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenUserDoesNotBelongToConnection()
    {
        // Create two users who belong to the connection.
        var user = ServiceTestData.User(Guid.NewGuid());
        var otherUser = ServiceTestData.User(Guid.NewGuid());

        // Create a third user who does not belong to the connection.
        var unauthorizedUser = ServiceTestData.User(Guid.NewGuid());

        // Create the connection between the first two users.
        var connection = CreateConnection(
            user.Id,
            otherUser.Id,
            user,
            otherUser);

        // Return the unauthorized user.
        _repository
            .Setup(x => x.GetUserByIdAsync(unauthorizedUser.Id))
            .ReturnsAsync(unauthorizedUser);

        // Return the existing connection.
        _repository
            .Setup(x => x.GetByIdWithUsersAsync(connection.Id))
            .ReturnsAsync(connection);

        // Try to delete the connection as an unauthorized user.
        var action = () =>
            _service.DeleteAsync(
                unauthorizedUser.Id,
                connection.Id);

        // Verify that access is denied.
        await action.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("You do not have access to this connection.");

        // Make sure the connection was not deleted.
        _repository.Verify(
            x => x.Remove(It.IsAny<Connection>()),
            Times.Never);

        // Make sure no changes were saved.
        _repository.Verify(
            x => x.SaveChangesAsync(),
            Times.Never);
    }

    // Create a connection object for the tests.
    private static Connection CreateConnection(
        Guid userAId,
        Guid userBId,
        User userA,
        User userB)
    {
        return new Connection
        {
            Id = Guid.NewGuid(),
            UserAId = userAId,
            UserBId = userBId,
            UserA = userA,
            UserB = userB,
            CreatedAt = DateTime.UtcNow
        };
    }
}

