using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Team3.Backend.Data;
using Team3.Backend.Features.Experiences;
using Team3.Backend.Features.Interests;
using Team3.Backend.Features.Progress;
using Team3.Backend.Models;
using ProgressEntity = Team3.Backend.Models.Progress;

namespace Team3.Backend.Tests;

public class RepositoryOwnershipTests
{
    [Fact]
    public async Task ExperiencesRepository_ShouldNotReturnAnotherUsersExperience()
    {
        await using var context = CreateContext();
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var experience = ServiceTestData.Experience(Guid.NewGuid(), ownerId);
        context.Users.AddRange(ServiceTestData.User(ownerId), ServiceTestData.User(otherUserId));
        context.Experiences.Add(experience);
        await context.SaveChangesAsync();
        var repository = new ExperiencesRepository(context);

        var result = await repository.GetByIdForUserAsync(experience.Id, otherUserId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task ProgressRepository_ShouldNotReturnAnotherUsersProgress()
    {
        await using var context = CreateContext();
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var direction = ServiceTestData.LearningDirection(Guid.NewGuid());
        var progress = ServiceTestData.Progress(Guid.NewGuid(), ownerId, direction.Id);
        progress.LearningDirection = null!;
        context.Users.AddRange(ServiceTestData.User(ownerId), ServiceTestData.User(otherUserId));
        context.LearningDirections.Add(direction);
        context.Progresses.Add(progress);
        await context.SaveChangesAsync();
        var repository = new ProgressRepository(context);

        var result = await repository.GetTrackedByIdForUserAsync(progress.Id, otherUserId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task InterestsRepository_ShouldRemoveOnlyTheRequestedUsersAssociation()
    {
        await using var context = CreateContext();
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var interest = new Interest { Id = Guid.NewGuid(), Name = "Science" };
        context.Users.AddRange(ServiceTestData.User(ownerId), ServiceTestData.User(otherUserId));
        context.Interests.Add(interest);
        context.UserInterests.Add(new UserInterest
        {
            UserId = ownerId,
            InterestId = interest.Id
        });
        await context.SaveChangesAsync();
        var repository = new InterestsRepository(context);

        var removed = await repository.RemoveUserInterestAsync(otherUserId, interest.Id);

        removed.Should().BeFalse();
        (await context.UserInterests.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task ProgressRepository_ShouldDetectDuplicateProgressForSameUserAndDirection()
    {
        await using var context = CreateContext();
        var userId = Guid.NewGuid();
        var direction = ServiceTestData.LearningDirection(Guid.NewGuid());
        context.Users.Add(ServiceTestData.User(userId));
        context.LearningDirections.Add(direction);
        context.Progresses.Add(new ProgressEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LearningDirectionId = direction.Id,
            Level = "Beginner",
            StartedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
        var repository = new ProgressRepository(context);

        (await repository.ExistsForUserAsync(userId, direction.Id)).Should().BeTrue();
        (await repository.ExistsForUserAsync(Guid.NewGuid(), direction.Id)).Should().BeFalse();
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}
