using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Team3.Backend.Features.AI;
using Team3.Backend.Features.AI.Dtos;
using Team3.Backend.Features.AI.Interfaces;
using Team3.Backend.Features.Recommendations;
using Team3.Backend.Features.Recommendations.Interfaces;
using Team3.Backend.Features.Users.Interfaces;
using Team3.Backend.Models;
using BackendResponse = Team3.Backend.Features.Recommendations.Dtos.PostRecommendationResponse;

namespace Team3.Backend.Tests;

public sealed class PostRecommendationIntegrationTests
{
    [Fact]
    public async Task Service_ShouldBuildContractRequestAndPreserveAiOrder()
    {
        var userId = Guid.NewGuid();
        var firstPostId = Guid.NewGuid();
        var secondPostId = Guid.NewGuid();
        var users = new Mock<IUsersRepository>();
        var repository = new Mock<IPostRecommendationRepository>();
        var client = new Mock<IPostRecommendationClient>();
        var user = ServiceTestData.User(userId);
        user.SelectedSkill = new Skill { Name = "Backend" };
        user.UserSkills.Add(new UserSkill
        {
            UserId = userId,
            Skill = new Skill { Name = "Python" }
        });
        user.UserInterests.Add(new UserInterest
        {
            UserId = userId,
            Interest = new Interest { Name = "AI" }
        });
        users.Setup(item => item.GetByIdForAiSyncAsync(userId))
            .ReturnsAsync(user);
        repository.Setup(item => item.GetEligibleCandidateIdsAsync(100))
            .ReturnsAsync(Enumerable.Range(0, 101)
                .Select(_ => Guid.NewGuid()).ToList());
        repository.Setup(item => item.GetRecentInteractionsAsync(userId))
            .ReturnsAsync(new List<RecentInteractionRequest>
            {
                new()
                {
                    PostId = firstPostId.ToString(),
                    InteractionType = "like",
                    Timestamp = DateTimeOffset.UtcNow
                }
            });
        repository.Setup(item => item.GetByIdsAsync(
                It.IsAny<IReadOnlyCollection<Guid>>()))
            .ReturnsAsync(new List<EducationalContent>
            {
                Post(firstPostId, "First"),
                Post(secondPostId, "Second")
            });
        client.Setup(item => item.RecommendPostsAsync(
                It.IsAny<PostRecommendationRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PostRecommendationResponse
            {
                RequestId = "req_1",
                ModelVersion = "model-v1",
                Count = 2,
                Items =
                [
                    Recommendation(secondPostId, 1),
                    Recommendation(firstPostId, 2)
                ]
            });
        var service = new PostRecommendationService(
            users.Object,
            repository.Object,
            client.Object,
            NullLogger<PostRecommendationService>.Instance);

        var result = await service.RecommendAsync(userId, 5, "req_1");

        client.Verify(item => item.RecommendPostsAsync(
            It.Is<PostRecommendationRequest>(request =>
                request.RequestId == "req_1"
                && request.UserId == userId.ToString()
                && request.LearningDirection == "Backend"
                && request.DeclaredTopics.SequenceEqual(new[] { "Python", "AI" })
                && request.EligibleCandidateIds.Count == 100
                && request.ExcludePostIds == null
                && request.Limit == 5
                && request.RecentInteractions!.Count == 1
                && request.RecentInteractions[0].InteractionType == "like"),
            It.IsAny<CancellationToken>()), Times.Once);
        result.Items.Select(item => item.Post.Id)
            .Should().Equal(secondPostId, firstPostId);
        result.Items.Select(item => item.Rank).Should().Equal(1, 2);
    }

    [Fact]
    public async Task Service_ShouldSendNullInteractionsAndSkipMissingReturnedPosts()
    {
        var userId = Guid.NewGuid();
        var existingPostId = Guid.NewGuid();
        var missingPostId = Guid.NewGuid();
        var users = new Mock<IUsersRepository>();
        var repository = new Mock<IPostRecommendationRepository>();
        var client = new Mock<IPostRecommendationClient>();
        var user = ServiceTestData.User(userId);
        user.SelectedSkill = new Skill { Name = "Backend" };
        users.Setup(item => item.GetByIdForAiSyncAsync(userId)).ReturnsAsync(user);
        repository.Setup(item => item.GetEligibleCandidateIdsAsync(100))
            .ReturnsAsync(new List<Guid> { existingPostId });
        repository.Setup(item => item.GetRecentInteractionsAsync(userId))
            .ReturnsAsync([]);
        repository.Setup(item => item.GetByIdsAsync(
                It.IsAny<IReadOnlyCollection<Guid>>()))
            .ReturnsAsync(new List<EducationalContent>
            {
                Post(existingPostId, "Existing")
            });
        client.Setup(item => item.RecommendPostsAsync(
                It.IsAny<PostRecommendationRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PostRecommendationResponse
            {
                RequestId = "req_1",
                Items =
                [
                    Recommendation(missingPostId, 1),
                    Recommendation(existingPostId, 2)
                ]
            });
        var service = new PostRecommendationService(
            users.Object,
            repository.Object,
            client.Object,
            NullLogger<PostRecommendationService>.Instance);

        var result = await service.RecommendAsync(userId);

        client.Verify(item => item.RecommendPostsAsync(
            It.Is<PostRecommendationRequest>(request =>
                request.RecentInteractions == null
                && request.ExcludePostIds == null),
            It.IsAny<CancellationToken>()), Times.Once);
        result.Items.Should().ContainSingle();
        result.Items[0].Post.Id.Should().Be(existingPostId);
    }

    [Fact]
    public async Task Service_ShouldReturnChronologicalFallbackAfterAiFailure()
    {
        var userId = Guid.NewGuid();
        var olderId = Guid.NewGuid();
        var newerId = Guid.NewGuid();
        var users = new Mock<IUsersRepository>();
        var repository = new Mock<IPostRecommendationRepository>();
        var client = new Mock<IPostRecommendationClient>();
        var user = ServiceTestData.User(userId);
        user.SelectedSkill = new Skill { Name = "Backend" };
        users.Setup(item => item.GetByIdForAiSyncAsync(userId)).ReturnsAsync(user);
        repository.Setup(item => item.GetEligibleCandidateIdsAsync(100))
            .ReturnsAsync(new List<Guid> { olderId, newerId });
        repository.Setup(item => item.GetRecentInteractionsAsync(userId))
            .ReturnsAsync([]);
        repository.Setup(item => item.GetByIdsAsync(
                It.IsAny<IReadOnlyCollection<Guid>>()))
            .ReturnsAsync(new List<EducationalContent>
            {
                Post(olderId, "Older", DateTime.UtcNow.AddDays(-1)),
                Post(newerId, "Newer", DateTime.UtcNow)
            });
        client.Setup(item => item.RecommendPostsAsync(
                It.IsAny<PostRecommendationRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AiServiceException(
                "failure",
                "operation",
                HttpStatusCode.InternalServerError));
        var service = new PostRecommendationService(
            users.Object,
            repository.Object,
            client.Object,
            NullLogger<PostRecommendationService>.Instance);

        var result = await service.RecommendAsync(userId, 2, "req_fallback");

        result.Items.Select(item => item.Post.Id)
            .Should().Equal(newerId, olderId);
        result.RequestId.Should().Be("req_fallback");
    }

    [Fact]
    public async Task Service_ShouldRejectMissingLearningDirection()
    {
        var userId = Guid.NewGuid();
        var users = new Mock<IUsersRepository>();
        users.Setup(item => item.GetByIdForAiSyncAsync(userId))
            .ReturnsAsync(ServiceTestData.User(userId));
        var service = new PostRecommendationService(
            users.Object,
            Mock.Of<IPostRecommendationRepository>(),
            Mock.Of<IPostRecommendationClient>(),
            NullLogger<PostRecommendationService>.Instance);

        var action = () => service.RecommendAsync(userId);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("A learning direction is required for post recommendations.");
    }

    private static PostRecommendationItem Recommendation(Guid id, int rank)
    {
        return new PostRecommendationItem
        {
            Id = id.ToString(),
            Rank = rank,
            Score = 0.8m
        };
    }

    private static EducationalContent Post(
        Guid id,
        string title,
        DateTime? createdAt = null)
    {
        return new EducationalContent
        {
            Id = id,
            UserId = Guid.NewGuid(),
            Title = title,
            ContentType = "Text",
            CreatedAt = createdAt ?? DateTime.UtcNow,
            UpdatedAt = createdAt ?? DateTime.UtcNow
        };
    }
}
