using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Team3.Backend.Data;
using Team3.Backend.Features.AI;
using Team3.Backend.Features.AI.Dtos;
using Team3.Backend.Features.AI.Interfaces;
using Team3.Backend.Features.EducationalContent;
using Team3.Backend.Features.EducationalContent.Dtos;
using Team3.Backend.Features.EducationalContent.Interfaces;
using Team3.Backend.Models;
using EducationalContentModel = Team3.Backend.Models.EducationalContent;

namespace Team3.Backend.Tests;

public sealed class ContentAnalysisIntegrationTests
{
    [Fact]
    public async Task AnalysisService_ShouldPersistCompleteAiResponse()
    {
        await using var context = CreateContext();
        var content = Content(Guid.NewGuid(), 1, "Build APIs", "Use C#.");
        context.EducationalContents.Add(content);
        await context.SaveChangesAsync();

        var client = new Mock<IContentAnalysisClient>();
        client.Setup(item => item.AnalyzeAsync(
                It.IsAny<ContentAnalysisRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContentAnalysisRequest request, CancellationToken _) =>
                Response(content, request.RequestId));
        var service = CreateAnalysisService(
            context,
            client.Object);

        await service.AnalyzeAsync(content);

        var analysis = await context.ContentAnalyses.SingleAsync();
        analysis.EducationalContentId.Should().Be(content.Id);
        analysis.ContentVersion.Should().Be(1);
        analysis.RequestId.Should().StartWith("req_");
        analysis.AnalysisState.Should().Be("Completed");
        analysis.ProcessingStatus.Should().Be("completed");
        analysis.ClassificationStatus.Should().Be("classified");
        analysis.TopicReasonCode.Should().Be("topic_match");
        analysis.PrimaryTopicsJson.Should().Contain("AI_DATA");
        analysis.SecondaryTopicsJson.Should().Contain("PROGRAMMING_WEB");
        analysis.TopicCount.Should().Be(2);
        analysis.DifficultyLevel.Should().Be("INTERMEDIATE");
        analysis.DifficultyConfidence.Should().Be(0.9132m);
        analysis.SafetyStatus.Should().Be("SAFE");
        analysis.SafetyConfidence.Should().Be(0.99m);
        analysis.SafetyThreshold.Should().Be(0.5m);
        analysis.SafetyReviewRequired.Should().BeTrue();
        analysis.RiskCategoriesJson.Should().Be("[]");
        analysis.RecommendationSignal.Should().Be("ALLOW");
        analysis.NeedsReview.Should().BeFalse();
        analysis.TopicModel.Should().Be("topic-v6");
        analysis.DifficultyModel.Should().Be("difficulty-v1");
        analysis.SafetyModel.Should().Be("safety-v2");
        analysis.PreprocessingVersion.Should().Be("preprocess-v1");
    }

    [Fact]
    public async Task AnalysisService_ShouldUpsertPostOnlyAfterValidAnalysis()
    {
        await using var context = CreateContext();
        var content = Content(Guid.NewGuid(), 1, "Build APIs", "Use C#.");
        context.EducationalContents.Add(content);
        await context.SaveChangesAsync();
        var client = new Mock<IContentAnalysisClient>();
        var upsert = new Mock<IPostUpsertedClient>();
        client.Setup(item => item.AnalyzeAsync(
                It.IsAny<ContentAnalysisRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContentAnalysisRequest request, CancellationToken _) =>
                Response(content, request.RequestId));
        upsert.Setup(item => item.UpsertAsync(
                It.IsAny<PostUpsertedRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PostUpsertedResponse());
        var service = new ContentAnalysisService(
            client.Object,
            new ContentAnalysisRepository(context),
            NullLogger<ContentAnalysisService>.Instance,
            upsert.Object);

        await service.AnalyzeAsync(content);

        upsert.Verify(item => item.UpsertAsync(
            It.Is<PostUpsertedRequest>(request =>
                request.PostId == content.Id.ToString()
                && request.CreatorId == content.UserId.ToString()
                && request.Body == "Build APIs\n\nUse C#."
                && request.PrimaryTopic == "AI_DATA"
                && request.Difficulty.Level == "INTERMEDIATE"
                && request.Safety.Status == "SAFE"
                && request.Safety.RecommendationSignal == "ALLOW"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AnalysisService_ShouldBuildTitleAndDescriptionTextAndPersistRequestId()
    {
        await using var context = CreateContext();
        var content = Content(Guid.NewGuid(), 1, "Build APIs", "Use C#.");
        context.EducationalContents.Add(content);
        await context.SaveChangesAsync();
        var client = new Mock<IContentAnalysisClient>();
        client.Setup(item => item.AnalyzeAsync(
                It.IsAny<ContentAnalysisRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContentAnalysisRequest request, CancellationToken _) =>
                Response(content, request.RequestId));
        var service = CreateAnalysisService(context, client.Object);

        await service.AnalyzeAsync(content);

        client.Verify(item => item.AnalyzeAsync(
            It.Is<ContentAnalysisRequest>(request =>
                request.ContentId == content.Id.ToString()
                && request.ContentVersion == 1
                && request.Text == "Build APIs\n\nUse C#."
                && request.Language == "en"
                && request.RequestId.StartsWith("req_")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(null, "Only description", "Only description")]
    [InlineData("Only title", null, "Only title")]
    public async Task AnalysisService_ShouldUseAvailableText(
        string? title,
        string? description,
        string expectedText)
    {
        await using var context = CreateContext();
        var content = Content(Guid.NewGuid(), 1, title ?? string.Empty, description);
        context.EducationalContents.Add(content);
        await context.SaveChangesAsync();
        var client = new Mock<IContentAnalysisClient>();
        client.Setup(item => item.AnalyzeAsync(
                It.IsAny<ContentAnalysisRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContentAnalysisRequest request, CancellationToken _) =>
                Response(content, request.RequestId));
        var service = CreateAnalysisService(context, client.Object);

        await service.AnalyzeAsync(content);

        client.Verify(item => item.AnalyzeAsync(
            It.Is<ContentAnalysisRequest>(request => request.Text == expectedText),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AnalysisService_ShouldRejectTextOverTwentyThousandCharacters()
    {
        await using var context = CreateContext();
        var content = Content(Guid.NewGuid(), 1, new string('x', 20_001), null);
        var client = new Mock<IContentAnalysisClient>();
        var service = CreateAnalysisService(context, client.Object);

        await service.AnalyzeAsync(content);

        client.Verify(item => item.AnalyzeAsync(
            It.IsAny<ContentAnalysisRequest>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AnalysisService_ShouldIgnoreStaleResponse()
    {
        await using var context = CreateContext();
        var content = Content(Guid.NewGuid(), 2, "Current", null);
        context.EducationalContents.Add(content);
        await context.SaveChangesAsync();
        var client = new Mock<IContentAnalysisClient>();
        client.Setup(item => item.AnalyzeAsync(
                It.IsAny<ContentAnalysisRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContentAnalysisRequest request, CancellationToken _) =>
                Response(content, request.RequestId, 1));
        var service = CreateAnalysisService(context, client.Object);

        await service.AnalyzeAsync(content);

        var analysis = await context.ContentAnalyses.SingleAsync();
        analysis.AnalysisState.Should().Be("Pending");
        analysis.ProcessingStatus.Should().BeNull();
    }

    [Fact]
    public async Task AnalysisService_ShouldRejectMismatchedResponseRequestId()
    {
        await using var context = CreateContext();
        var content = Content(Guid.NewGuid(), 1, "Current", null);
        context.EducationalContents.Add(content);
        await context.SaveChangesAsync();
        var client = new Mock<IContentAnalysisClient>();
        client.Setup(item => item.AnalyzeAsync(
                It.IsAny<ContentAnalysisRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContentAnalysisRequest _, CancellationToken _) =>
                Response(content, "req_different"));
        var service = CreateAnalysisService(context, client.Object);

        await service.AnalyzeAsync(content);

        var analysis = await context.ContentAnalyses.SingleAsync();
        analysis.AnalysisState.Should().Be("Failed");
        analysis.FailureCode.Should().Be("request_id_mismatch");
        analysis.RequestId.Should().StartWith("req_")
            .And.NotBe("req_different");
    }

    [Fact]
    public async Task AnalysisService_ShouldRecordAiFailureWithoutDeletingContent()
    {
        await using var context = CreateContext();
        var content = Content(Guid.NewGuid(), 1, "Valid content", null);
        context.EducationalContents.Add(content);
        await context.SaveChangesAsync();
        var client = new Mock<IContentAnalysisClient>();
        client.Setup(item => item.AnalyzeAsync(
                It.IsAny<ContentAnalysisRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AiServiceException(
                "failure",
                "operation",
                HttpStatusCode.InternalServerError));
        var service = CreateAnalysisService(context, client.Object);

        await service.AnalyzeAsync(content);

        (await context.EducationalContents.AnyAsync(item => item.Id == content.Id))
            .Should().BeTrue();
        var analysis = await context.ContentAnalyses.SingleAsync();
        analysis.AnalysisState.Should().Be("Failed");
        analysis.FailureCode.Should().Be("http_500");
    }

    [Fact]
    public async Task AnalysisService_ShouldReusePersistedRequestIdForAnExistingVersion()
    {
        await using var context = CreateContext();
        var content = Content(Guid.NewGuid(), 1, "Valid content", null);
        context.EducationalContents.Add(content);
        context.ContentAnalyses.Add(new ContentAnalysis
        {
            Id = Guid.NewGuid(),
            EducationalContentId = content.Id,
            ContentVersion = 1,
            RequestId = "req_existing",
            AnalysisState = "Failed"
        });
        await context.SaveChangesAsync();
        var client = new Mock<IContentAnalysisClient>();
        client.Setup(item => item.AnalyzeAsync(
                It.IsAny<ContentAnalysisRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContentAnalysisRequest request, CancellationToken _) =>
                Response(content, request.RequestId));
        var service = CreateAnalysisService(context, client.Object);

        await service.AnalyzeAsync(content);

        client.Verify(item => item.AnalyzeAsync(
            It.Is<ContentAnalysisRequest>(request =>
                request.RequestId == "req_existing"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ContentDelete_ShouldCascadeLocalAnalysisWithoutCallingAi()
    {
        await using var context = CreateContext();
        var content = Content(Guid.NewGuid(), 1, "Content", null);
        context.EducationalContents.Add(content);
        context.ContentAnalyses.Add(new ContentAnalysis
        {
            Id = Guid.NewGuid(),
            EducationalContentId = content.Id,
            ContentVersion = 1,
            RequestId = "req_delete",
            AnalysisState = "Completed"
        });
        await context.SaveChangesAsync();

        context.EducationalContents.Remove(content);
        await context.SaveChangesAsync();

        (await context.ContentAnalyses.AnyAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task EducationalContentService_ShouldCreateVersionOneAndAnalyzeAfterSave()
    {
        var userId = Guid.NewGuid();
        var events = new List<string>();
        var repository = new Mock<IEducationalContentRepository>();
        var analysis = new Mock<IContentAnalysisService>();
        var user = ServiceTestData.User(userId);
        repository.Setup(item => item.GetUserByFirebaseUidAsync(user.FirebaseUid))
            .ReturnsAsync(user);
        repository.Setup(item => item.SaveChangesAsync())
            .Callback(() => events.Add("database"))
            .Returns(Task.CompletedTask);
        analysis.Setup(item => item.AnalyzeAsync(
                It.IsAny<EducationalContentModel>(),
                It.IsAny<CancellationToken>()))
            .Callback(() => events.Add("ai"))
            .Returns(Task.CompletedTask);
        var service = new EducationalContentService(
            repository.Object,
            analysis.Object);

        var response = await service.CreateAsync(
            user.FirebaseUid,
            new CreateEducationalContentRequest
            {
                Title = "Title",
                Description = "Description",
                ContentType = "Text"
            });

        response.Should().NotBeNull();
        events.Should().Equal("database", "ai");
        analysis.Verify(item => item.AnalyzeAsync(
            It.Is<EducationalContentModel>(content =>
                content.ContentVersion == 1
                && content.Id.ToString() == response.Id.ToString()),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EducationalContentService_ShouldIncrementVersionBeforeAnalyzingUpdate()
    {
        var userId = Guid.NewGuid();
        var contentId = Guid.NewGuid();
        var events = new List<string>();
        var repository = new Mock<IEducationalContentRepository>();
        var analysis = new Mock<IContentAnalysisService>();
        var user = ServiceTestData.User(userId);
        var content = Content(contentId, 1, "Old", null);
        content.UserId = userId;
        repository.Setup(item => item.GetUserByFirebaseUidAsync(user.FirebaseUid))
            .ReturnsAsync(user);
        repository.Setup(item => item.GetByIdForUserAsync(contentId, userId))
            .ReturnsAsync(content);
        repository.Setup(item => item.SaveChangesAsync())
            .Callback(() => events.Add("database"))
            .Returns(Task.CompletedTask);
        analysis.Setup(item => item.AnalyzeAsync(
                It.IsAny<EducationalContentModel>(),
                It.IsAny<CancellationToken>()))
            .Callback(() => events.Add("ai"))
            .Returns(Task.CompletedTask);
        var service = new EducationalContentService(
            repository.Object,
            analysis.Object);

        await service.UpdateAsync(
            user.FirebaseUid,
            contentId,
            new UpdateEducationalContentRequest { Title = "New" });

        content.ContentVersion.Should().Be(2);
        events.Should().Equal("database", "ai");
        analysis.Verify(item => item.AnalyzeAsync(
            It.Is<EducationalContentModel>(item => item.ContentVersion == 2),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private static ContentAnalysisService CreateAnalysisService(
        AppDbContext context,
        IContentAnalysisClient client)
    {
        return new ContentAnalysisService(
            client,
            new ContentAnalysisRepository(context),
            NullLogger<ContentAnalysisService>.Instance);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static EducationalContentModel Content(
        Guid id,
        int version,
        string title,
        string? description)
    {
        return new EducationalContentModel
        {
            Id = id,
            UserId = Guid.NewGuid(),
            Title = title,
            Description = description,
            ContentType = "Text",
            ContentVersion = version,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static ContentAnalysisResponse Response(
        EducationalContentModel content,
        string requestId,
        int? responseVersion = null)
    {
        return new ContentAnalysisResponse
        {
            RequestId = requestId,
            ContentId = content.Id.ToString(),
            ContentVersion = responseVersion ?? content.ContentVersion,
            ProcessingStatus = "completed",
            ProcessedAt = DateTimeOffset.UtcNow,
            Topics = new ContentTopics
            {
                ClassificationStatus = "classified",
                ReasonCode = "topic_match",
                PrimaryTopics = [new TopicResult
                {
                    Topic = "AI_DATA",
                    Confidence = 0.9875m
                }],
                SecondaryTopics = [new TopicResult
                {
                    Topic = "PROGRAMMING_WEB",
                    Confidence = 0.4210m
                }],
                TopicCount = 2
            },
            Difficulty = new DifficultyResult
            {
                Level = "INTERMEDIATE",
                Confidence = 0.9132m
            },
            Safety = new SafetyResult
            {
                Status = "SAFE",
                Confidence = 0.99m,
                Threshold = 0.5m,
                ReviewRequired = true,
                RecommendationSignal = "ALLOW"
            },
            ModelVersions = new ModelVersions
            {
                TopicModel = "topic-v6",
                DifficultyModel = "difficulty-v1",
                SafetyModel = "safety-v2"
            },
            PreprocessingVersion = "preprocess-v1"
        };
    }
}
