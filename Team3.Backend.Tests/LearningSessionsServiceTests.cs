using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Team3.Backend.Data;
using Team3.Backend.Features.LearningSessions;
using Team3.Backend.Features.LearningSessions.Dtos;
using Team3.Backend.Models;

namespace Team3.Backend.Tests;

public sealed class LearningSessionsServiceTests
{
    [Fact]
    public async Task CreateAsyncCreatesScheduledSessionForConnectionParticipant()
    {
        await using var context = CreateContext();
        var (service, firstUser, _, connection) = await CreateServiceWithConnectionAsync(context);

        var response = await service.CreateAsync(firstUser.Id, new CreateLearningSessionRequest
        {
            ConnectionId = connection.Id,
            Title = "  C# Basics ",
            Description = "Introduction",
            ScheduledAt = DateTime.UtcNow.AddDays(1),
            MeetingUrl = "https://example.test/meeting"
        });

        response.ConnectionId.Should().Be(connection.Id);
        response.Title.Should().Be("C# Basics");
        response.Status.Should().Be("Scheduled");
        context.LearningSessions.Should().ContainSingle(session =>
            session.ConnectionId == connection.Id
            && session.Status == "Scheduled");
    }

    [Fact]
    public async Task CreateAsyncRejectsNonexistentConnection()
    {
        await using var context = CreateContext();
        var service = new LearningSessionsService(new LearningSessionsRepository(context));
        var user = ServiceTestData.User(Guid.NewGuid());
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var action = () => service.CreateAsync(user.Id, ValidCreateRequest(Guid.NewGuid()));

        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Connection not found.");
    }

    [Fact]
    public async Task CreateAsyncRejectsNonParticipant()
    {
        await using var context = CreateContext();
        var (service, _, _, connection) = await CreateServiceWithConnectionAsync(context);
        var outsider = ServiceTestData.User(Guid.NewGuid());
        context.Users.Add(outsider);
        await context.SaveChangesAsync();

        var action = () => service.CreateAsync(
            outsider.Id,
            ValidCreateRequest(connection.Id));

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Only connection participants can create learning sessions.");
    }

    [Fact]
    public async Task GetMySessionsAsyncReturnsOnlySessionsForUsersConnections()
    {
        await using var context = CreateContext();
        var (service, firstUser, _, connection) = await CreateServiceWithConnectionAsync(context);
        var unrelatedUser = ServiceTestData.User(Guid.NewGuid());
        var unrelatedConnection = new Connection
        {
            Id = Guid.NewGuid(),
            UserAId = unrelatedUser.Id,
            UserBId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(unrelatedUser);
        context.Connections.Add(unrelatedConnection);
        context.LearningSessions.AddRange(
            Session(connection.Id, "Mine"),
            Session(unrelatedConnection.Id, "Not mine"));
        await context.SaveChangesAsync();

        var result = await service.GetMySessionsAsync(firstUser.Id);

        result.Should().ContainSingle();
        result[0].Title.Should().Be("Mine");
    }

    [Fact]
    public async Task GetByIdAsyncReturnsNullForUnauthorizedUser()
    {
        await using var context = CreateContext();
        var (service, _, _, connection) = await CreateServiceWithConnectionAsync(context);
        var session = Session(connection.Id, "Private");
        context.LearningSessions.Add(session);
        await context.SaveChangesAsync();

        var response = await service.GetByIdAsync(Guid.NewGuid(), session.Id);

        response.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsyncReturnsSessionForConnectionParticipant()
    {
        await using var context = CreateContext();
        var (service, firstUser, _, connection) = await CreateServiceWithConnectionAsync(context);
        var session = Session(connection.Id, "Available");
        context.LearningSessions.Add(session);
        await context.SaveChangesAsync();

        var response = await service.GetByIdAsync(firstUser.Id, session.Id);

        response.Should().NotBeNull();
        response!.Title.Should().Be("Available");
    }

    [Fact]
    public async Task UpdateAsyncUpdatesOwnedSessionFields()
    {
        await using var context = CreateContext();
        var (service, firstUser, _, connection) = await CreateServiceWithConnectionAsync(context);
        var session = Session(connection.Id, "Original");
        context.LearningSessions.Add(session);
        await context.SaveChangesAsync();

        var response = await service.UpdateAsync(firstUser.Id, session.Id,
            new UpdateLearningSessionRequest
            {
                Title = "Updated",
                Description = "New description",
                ScheduledAt = DateTime.UtcNow.AddDays(2),
                MeetingUrl = "https://example.test/updated"
            });

        response.Title.Should().Be("Updated");
        response.Description.Should().Be("New description");
        response.MeetingUrl.Should().Be("https://example.test/updated");
    }

    [Fact]
    public async Task UpdateAsyncRejectsUserOutsideSessionConnection()
    {
        await using var context = CreateContext();
        var (service, _, _, connection) = await CreateServiceWithConnectionAsync(context);
        var session = Session(connection.Id, "Private");
        var outsider = ServiceTestData.User(Guid.NewGuid());
        context.Users.Add(outsider);
        context.LearningSessions.Add(session);
        await context.SaveChangesAsync();

        var action = () => service.UpdateAsync(
            outsider.Id,
            session.Id,
            new UpdateLearningSessionRequest { Title = "Should fail" });

        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Learning session not found.");
        context.LearningSessions.Single().Title.Should().Be("Private");
    }

    [Fact]
    public async Task UpdateAsyncRejectsUnsupportedStatus()
    {
        await using var context = CreateContext();
        var (service, firstUser, _, connection) = await CreateServiceWithConnectionAsync(context);
        var session = Session(connection.Id, "Status test");
        context.LearningSessions.Add(session);
        await context.SaveChangesAsync();

        var action = () => service.UpdateAsync(firstUser.Id, session.Id,
            new UpdateLearningSessionRequest { Status = "InProgress" });

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Status must be one of: Scheduled, Completed, Cancelled.");
    }

    [Fact]
    public async Task UpdateAsyncSupportsCancellationAsStatusUpdate()
    {
        await using var context = CreateContext();
        var (service, firstUser, _, connection) = await CreateServiceWithConnectionAsync(context);
        var session = Session(connection.Id, "Cancel me");
        context.LearningSessions.Add(session);
        await context.SaveChangesAsync();

        var response = await service.UpdateAsync(firstUser.Id, session.Id,
            new UpdateLearningSessionRequest { Status = "Cancelled" });

        response.Status.Should().Be("Cancelled");
        context.LearningSessions.Should().ContainSingle(item =>
            item.Id == session.Id && item.Status == "Cancelled");
    }

    private static async Task<(
        LearningSessionsService Service,
        User FirstUser,
        User SecondUser,
        Connection Connection)> CreateServiceWithConnectionAsync(
        AppDbContext context)
    {
        var firstUser = ServiceTestData.User(Guid.NewGuid());
        var secondUser = ServiceTestData.User(Guid.NewGuid());
        var connection = new Connection
        {
            Id = Guid.NewGuid(),
            UserAId = firstUser.Id,
            UserBId = secondUser.Id,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.AddRange(firstUser, secondUser);
        context.Connections.Add(connection);
        await context.SaveChangesAsync();

        return (
            new LearningSessionsService(new LearningSessionsRepository(context)),
            firstUser,
            secondUser,
            connection);
    }

    private static CreateLearningSessionRequest ValidCreateRequest(Guid connectionId) => new()
    {
        ConnectionId = connectionId,
        Title = "Learning session",
        ScheduledAt = DateTime.UtcNow.AddDays(1)
    };

    private static LearningSession Session(Guid connectionId, string title) => new()
    {
        Id = Guid.NewGuid(),
        ConnectionId = connectionId,
        Title = title,
        ScheduledAt = DateTime.UtcNow.AddDays(1),
        Status = "Scheduled",
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
