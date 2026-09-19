using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Team3.Backend.Data;
using Team3.Backend.Features.Points;
using Team3.Backend.Features.Points.Dtos;
using Team3.Backend.Models;

namespace Team3.Backend.Tests;

public class PointsServiceTests
{
    [Fact]
    public async Task GetBalanceAsync_ShouldReturnCurrentUserBalance()
    {
        await using var context = CreateContext();
        var user = AddUser(context, points: 35);
        await context.SaveChangesAsync();
        var service = new PointsService(context);

        var result = await service.GetBalanceAsync(user.Id);

        result.Points.Should().Be(35);
    }

    [Fact]
    public async Task PurchaseAsync_ShouldUseBackendPackageAndCreatePurchaseAndTransaction()
    {
        await using var context = CreateContext();
        var user = AddUser(context, points: 5);
        await context.SaveChangesAsync();
        var service = new PointsService(context);

        var result = await service.PurchaseAsync(user.Id, "starter", "purchase-1");

        result.Points.Should().Be(100);
        result.Price.Should().Be(5m);
        result.PaymentMethod.Should().Be("Simulated");
        result.Status.Should().Be("Completed");
        context.Users.Single().Points.Should().Be(105);
        context.PointsPurchases.Should().ContainSingle(purchase =>
            purchase.UserId == user.Id
            && purchase.Points == 100
            && purchase.Price == 5m
            && purchase.PaymentMethod == "Simulated"
            && purchase.Status == "Completed");
        context.PointsTransactions.Should().ContainSingle(transaction =>
            transaction.UserId == user.Id
            && transaction.Amount == 100
            && transaction.TransactionType == PointsTransactionType.Purchase);
    }

    [Fact]
    public async Task PurchaseAsync_ShouldRejectUnknownPackageWithoutChangingBalance()
    {
        await using var context = CreateContext();
        var user = AddUser(context, points: 5);
        await context.SaveChangesAsync();
        var service = new PointsService(context);

        var action = () => service.PurchaseAsync(
            user.Id,
            "not-a-package",
            "purchase-invalid");

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Invalid Points package.");
        context.Users.Single().Points.Should().Be(5);
        context.PointsPurchases.Should().BeEmpty();
        context.PointsTransactions.Should().BeEmpty();
    }

    [Fact]
    public async Task PurchaseAsync_ShouldRequireIdempotencyKey()
    {
        await using var context = CreateContext();
        var user = AddUser(context, points: 5);
        await context.SaveChangesAsync();
        var service = new PointsService(context);

        var action = () => service.PurchaseAsync(user.Id, "starter", " ");

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Idempotency key is required.");
        context.Users.Single().Points.Should().Be(5);
    }

    [Fact]
    public async Task PurchaseAsync_ShouldReturnExistingPurchaseForSameUserAndKey()
    {
        await using var context = CreateContext();
        var user = AddUser(context, points: 5);
        await context.SaveChangesAsync();
        var service = new PointsService(context);

        var first = await service.PurchaseAsync(user.Id, "starter", "retry-1");
        var second = await service.PurchaseAsync(user.Id, "starter", "retry-1");

        second.Id.Should().Be(first.Id);
        context.Users.Single().Points.Should().Be(105);
        context.PointsPurchases.Should().ContainSingle();
        context.PointsTransactions.Should().ContainSingle();
    }

    [Fact]
    public async Task PurchaseAsync_ShouldScopeSameKeyToEachUser()
    {
        await using var context = CreateContext();
        var firstUser = AddUser(context, points: 5);
        var secondUser = AddUser(context, points: 5);
        await context.SaveChangesAsync();
        var service = new PointsService(context);

        var first = await service.PurchaseAsync(firstUser.Id, "starter", "shared-key");
        var second = await service.PurchaseAsync(secondUser.Id, "starter", "shared-key");

        second.Id.Should().NotBe(first.Id);
        context.PointsPurchases.Should().HaveCount(2);
        context.Users.Single(user => user.Id == firstUser.Id).Points.Should().Be(105);
        context.Users.Single(user => user.Id == secondUser.Id).Points.Should().Be(105);
    }

    [Fact]
    public async Task PurchaseAsync_ShouldAllowTwoIntentionalPurchasesWithDifferentKeys()
    {
        await using var context = CreateContext();
        var user = AddUser(context, points: 5);
        await context.SaveChangesAsync();
        var service = new PointsService(context);

        await service.PurchaseAsync(user.Id, "starter", "purchase-a");
        await service.PurchaseAsync(user.Id, "starter", "purchase-b");

        context.Users.Single().Points.Should().Be(205);
        context.PointsPurchases.Should().HaveCount(2);
        context.PointsTransactions.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetTransactionsAsync_ShouldReturnOnlyCurrentUsersTransactions()
    {
        await using var context = CreateContext();
        var user = AddUser(context, points: 50);
        var otherUser = AddUser(context, points: 50);
        context.PointsTransactions.AddRange(
            Transaction(user.Id, 15, PointsTransactionType.Purchase),
            Transaction(otherUser.Id, 100, PointsTransactionType.Purchase));
        await context.SaveChangesAsync();
        var service = new PointsService(context);

        var result = await service.GetTransactionsAsync(user.Id);

        result.Should().ContainSingle();
        result[0].Amount.Should().Be(15);
        result[0].RelatedUserId.Should().BeNull();
    }

    [Fact]
    public async Task GetPurchasesAsync_ShouldReturnOnlyCurrentUsersPurchases()
    {
        await using var context = CreateContext();
        var user = AddUser(context, points: 50);
        var otherUser = AddUser(context, points: 50);
        context.PointsPurchases.AddRange(
            new PointsPurchase
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                PackageId = "starter",
                Points = 100,
                Price = 5m,
                Reference = "user-purchase",
                CreatedAt = DateTime.UtcNow
            },
            new PointsPurchase
            {
                Id = Guid.NewGuid(),
                UserId = otherUser.Id,
                PackageId = "premium",
                Points = 500,
                Price = 18m,
                Reference = "other-purchase",
                CreatedAt = DateTime.UtcNow
            });
        await context.SaveChangesAsync();
        var service = new PointsService(context);

        var result = await service.GetPurchasesAsync(user.Id);

        result.Should().ContainSingle();
        result[0].PackageId.Should().Be("starter");
    }

    [Fact]
    public async Task CompleteMentoringSessionAsync_ShouldTransferPointsAndCreateBothLedgerEntries()
    {
        await using var context = CreateContext();
        var student = AddUser(context, points: 20);
        var mentor = AddUser(context, points: 10);
        var session = AddCompletedSession(context, student.Id, mentor.Id);
        await context.SaveChangesAsync();
        var service = new PointsService(context);

        await service.CompleteMentoringSessionAsync(
            student.Id,
            mentor.Id,
            session.Id);

        context.Users.Single(user => user.Id == student.Id).Points.Should().Be(5);
        context.Users.Single(user => user.Id == mentor.Id).Points.Should().Be(25);
        context.PointsTransactions.Should().HaveCount(2);
        context.PointsTransactions.Should().Contain(transaction =>
            transaction.UserId == student.Id
            && transaction.Amount == -15
            && transaction.TransactionType == PointsTransactionType.MentoringPayment
            && transaction.RelatedUserId == mentor.Id
            && transaction.MentoringSessionId == session.Id);
        context.PointsTransactions.Should().Contain(transaction =>
            transaction.UserId == mentor.Id
            && transaction.Amount == 15
            && transaction.TransactionType == PointsTransactionType.MentoringEarning
            && transaction.RelatedUserId == student.Id
            && transaction.MentoringSessionId == session.Id);
    }

    [Fact]
    public async Task CompleteMentoringSessionAsync_ShouldRejectInsufficientStudentBalanceWithoutChanges()
    {
        await using var context = CreateContext();
        var student = AddUser(context, points: 14);
        var mentor = AddUser(context, points: 10);
        var session = AddCompletedSession(context, student.Id, mentor.Id);
        await context.SaveChangesAsync();
        var service = new PointsService(context);

        var action = () => service.CompleteMentoringSessionAsync(
            student.Id,
            mentor.Id,
            session.Id);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("The student does not have enough Points.");
        context.Users.Single(user => user.Id == student.Id).Points.Should().Be(14);
        context.Users.Single(user => user.Id == mentor.Id).Points.Should().Be(10);
        context.PointsTransactions.Should().BeEmpty();
    }

    [Fact]
    public async Task CompleteMentoringSessionAsync_ShouldAllowExactlyFifteenPointsOnce()
    {
        await using var context = CreateContext();
        var student = AddUser(context, points: 15);
        var mentor = AddUser(context, points: 10);
        var session = AddCompletedSession(context, student.Id, mentor.Id);
        await context.SaveChangesAsync();
        var service = new PointsService(context);

        await service.CompleteMentoringSessionAsync(student.Id, mentor.Id, session.Id);

        context.Users.Single(user => user.Id == student.Id).Points.Should().Be(0);
        context.Users.Single(user => user.Id == mentor.Id).Points.Should().Be(25);
        context.PointsTransactions.Should().HaveCount(2);
    }

    [Fact]
    public async Task CompleteMentoringSessionAsync_ShouldRejectSelfMentoringWithoutChanges()
    {
        await using var context = CreateContext();
        var user = AddUser(context, points: 50);
        await context.SaveChangesAsync();
        var service = new PointsService(context);

        var action = () => service.CompleteMentoringSessionAsync(
            user.Id,
            user.Id,
            Guid.NewGuid());

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("A user cannot mentor themselves.");
        context.Users.Single().Points.Should().Be(50);
        context.PointsTransactions.Should().BeEmpty();
    }

    [Fact]
    public async Task CompleteMentoringSessionAsync_ShouldRejectDuplicateSessionProcessing()
    {
        await using var context = CreateContext();
        var student = AddUser(context, points: 30);
        var mentor = AddUser(context, points: 10);
        var session = AddCompletedSession(context, student.Id, mentor.Id);
        await context.SaveChangesAsync();
        var service = new PointsService(context);

        await service.CompleteMentoringSessionAsync(student.Id, mentor.Id, session.Id);
        var action = () => service.CompleteMentoringSessionAsync(
            student.Id,
            mentor.Id,
            session.Id);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("This learning session has already been paid.");
        context.Users.Single(user => user.Id == student.Id).Points.Should().Be(15);
        context.Users.Single(user => user.Id == mentor.Id).Points.Should().Be(25);
        context.PointsTransactions.Should().HaveCount(2);
    }

    [Fact]
    public async Task CompleteMentoringSessionAsync_ShouldRejectUsersOutsideSessionWithoutChanges()
    {
        await using var context = CreateContext();
        var student = AddUser(context, points: 20);
        var mentor = AddUser(context, points: 10);
        var unrelatedUser = AddUser(context, points: 50);
        var session = AddCompletedSession(context, student.Id, mentor.Id);
        await context.SaveChangesAsync();
        var service = new PointsService(context);

        var action = () => service.CompleteMentoringSessionAsync(
            student.Id,
            unrelatedUser.Id,
            session.Id);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("The learning session does not belong to these users.");
        context.Users.Single(user => user.Id == student.Id).Points.Should().Be(20);
        context.Users.Single(user => user.Id == unrelatedUser.Id).Points.Should().Be(50);
        context.PointsTransactions.Should().BeEmpty();
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static User AddUser(AppDbContext context, int points)
    {
        var user = ServiceTestData.User(Guid.NewGuid());
        user.Points = points;
        context.Users.Add(user);
        return user;
    }

    private static LearningSession AddCompletedSession(
        AppDbContext context,
        Guid studentId,
        Guid mentorId)
    {
        var connection = new Connection
        {
            Id = Guid.NewGuid(),
            UserAId = studentId,
            UserBId = mentorId,
            CreatedAt = DateTime.UtcNow
        };
        var session = new LearningSession
        {
            Id = Guid.NewGuid(),
            ConnectionId = connection.Id,
            Connection = connection,
            Title = "React",
            Status = "Completed",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ScheduledAt = DateTime.UtcNow
        };
        context.Connections.Add(connection);
        context.LearningSessions.Add(session);
        return session;
    }

    private static PointsTransaction Transaction(
        Guid userId,
        int amount,
        PointsTransactionType type) => new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Amount = amount,
            TransactionType = type,
            Reason = "Test",
            CreatedAt = DateTime.UtcNow
        };
}
