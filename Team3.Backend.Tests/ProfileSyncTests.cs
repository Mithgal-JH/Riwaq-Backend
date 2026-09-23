using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Team3.Backend.Features.AI;
using Team3.Backend.Features.AI.Dtos;
using Team3.Backend.Features.AI.Interfaces;
using Team3.Backend.Features.Interests;
using Team3.Backend.Features.Interests.Interfaces;
using Team3.Backend.Features.Skills;
using Team3.Backend.Features.Skills.Interfaces;
using Team3.Backend.Features.Users;
using Team3.Backend.Services.Caching;
using Team3.Backend.Features.Users.Dtos;
using Team3.Backend.Features.Users.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Tests;

public sealed class ProfileSyncTests
{
    [Fact]
    public async Task ProfileSyncService_ShouldSendCompleteProfileRepresentation()
    {
        var userId = Guid.NewGuid();
        var user = ServiceTestData.User(userId);
        user.Profile = new Profile { UserId = userId, Bio = "Build useful systems." };
        user.SelectedSkill = new Skill { Id = Guid.NewGuid(), Name = "AI & Machine Learning" };
        user.UserSkills.Add(new UserSkill
        {
            UserId = userId,
            Skill = new Skill { Name = "SQL" }
        });
        user.UserSkills.Add(new UserSkill
        {
            UserId = userId,
            Skill = new Skill { Name = "Python" }
        });
        user.UserInterests.Add(new UserInterest
        {
            UserId = userId,
            Interest = new Interest { Name = "Machine Learning" }
        });

        var repository = new Mock<IUsersRepository>();
        var client = new Mock<IProfileSyncClient>();
        repository.Setup(item => item.GetByIdForAiSyncAsync(userId))
            .ReturnsAsync(user);
        client.Setup(item => item.SyncAsync(
                It.IsAny<ProfileSyncRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProfileSyncResponse());
        var service = new ProfileSyncService(
            repository.Object,
            client.Object,
            NullLogger<ProfileSyncService>.Instance);

        await service.UpsertProfileAsync(userId);

        client.Verify(item => item.SyncAsync(
            It.Is<ProfileSyncRequest>(request =>
                request.SyncType == "upsert"
                && request.Profiles!.Count == 1
                && request.Profiles[0].ProfileId == userId.ToString()
                && request.Profiles[0].Skills.SequenceEqual(new[] { "Python", "SQL" })
                && request.Profiles[0].Interests.SequenceEqual(new[] { "Machine Learning" })
                && request.Profiles[0].LearningDirection == "AI & Machine Learning"
                && request.Profiles[0].Bio == "Build useful systems."),
            It.IsAny<CancellationToken>()), Times.Once);
        repository.Verify(item => item.GetByIdForAiSyncAsync(userId), Times.Once);
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ProfileSyncService_ShouldSendEmptyCollectionsAndNullOptionalValues()
    {
        var userId = Guid.NewGuid();
        var repository = new Mock<IUsersRepository>();
        var client = new Mock<IProfileSyncClient>();
        repository.Setup(item => item.GetByIdForAiSyncAsync(userId))
            .ReturnsAsync(ServiceTestData.User(userId));
        client.Setup(item => item.SyncAsync(
                It.IsAny<ProfileSyncRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProfileSyncResponse());
        var service = new ProfileSyncService(
            repository.Object,
            client.Object,
            NullLogger<ProfileSyncService>.Instance);

        await service.UpsertProfileAsync(userId);

        client.Verify(item => item.SyncAsync(
            It.Is<ProfileSyncRequest>(request =>
                request.Profiles![0].Skills.Count == 0
                && request.Profiles[0].Interests.Count == 0
                && request.Profiles[0].LearningDirection == null
                && request.Profiles[0].Bio == null),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProfileSyncService_ShouldSendDeletePayload()
    {
        var userId = Guid.NewGuid();
        var repository = new Mock<IUsersRepository>();
        var client = new Mock<IProfileSyncClient>();
        client.Setup(item => item.SyncAsync(
                It.IsAny<ProfileSyncRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProfileSyncResponse());
        var service = new ProfileSyncService(
            repository.Object,
            client.Object,
            NullLogger<ProfileSyncService>.Instance);

        await service.DeleteProfileAsync(userId);

        client.Verify(item => item.SyncAsync(
            It.Is<ProfileSyncRequest>(request =>
                request.SyncType == "delete"
                && request.ProfileIds!.SequenceEqual(new[] { userId.ToString() })
                && request.Profiles == null),
            It.IsAny<CancellationToken>()), Times.Once);
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ProfileSyncService_ShouldNotLoadEachSkillOrInterestSeparately()
    {
        var userId = Guid.NewGuid();
        var repository = new Mock<IUsersRepository>();
        var client = new Mock<IProfileSyncClient>();
        repository.Setup(item => item.GetByIdForAiSyncAsync(userId))
            .ReturnsAsync(ServiceTestData.User(userId));
        client.Setup(item => item.SyncAsync(
                It.IsAny<ProfileSyncRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProfileSyncResponse());
        var service = new ProfileSyncService(
            repository.Object,
            client.Object,
            NullLogger<ProfileSyncService>.Instance);

        await service.UpsertProfileAsync(userId);

        repository.Verify(item => item.GetByIdForAiSyncAsync(userId), Times.Once);
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ProfileSyncService_ShouldSwallowAiFailure()
    {
        var userId = Guid.NewGuid();
        var repository = new Mock<IUsersRepository>();
        var client = new Mock<IProfileSyncClient>();
        repository.Setup(item => item.GetByIdForAiSyncAsync(userId))
            .ReturnsAsync(ServiceTestData.User(userId));
        client.Setup(item => item.SyncAsync(
                It.IsAny<ProfileSyncRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AiServiceException("failed", "operation"));
        var service = new ProfileSyncService(
            repository.Object,
            client.Object,
            NullLogger<ProfileSyncService>.Instance);

        var action = () => service.UpsertProfileAsync(userId);

        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task UsersService_ShouldSyncOnlyAfterProfilePersistence()
    {
        var userId = Guid.NewGuid();
        var events = new List<string>();
        var repository = new Mock<IUsersRepository>();
        var sync = new Mock<IProfileSyncService>();
        var user = ServiceTestData.User(userId);
        repository.Setup(item => item.GetByIdWithProfileAsync(userId))
            .ReturnsAsync(user);
        repository.Setup(item => item.SaveChangesAsync())
            .Callback(() => events.Add("database"))
            .Returns(Task.CompletedTask);
        sync.Setup(item => item.UpsertProfileAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .Callback(() => events.Add("ai"))
            .Returns(Task.CompletedTask);
        var service = new UsersService(repository.Object, sync.Object);

        await service.UpdateMyProfileAsync(userId, new UpdateProfileRequest
        {
            Bio = "Updated bio"
        });

        events.Should().Equal("database", "ai");
    }

    [Fact]
    public async Task UsersService_ShouldNotSyncWhenProfilePersistenceFails()
    {
        var userId = Guid.NewGuid();
        var repository = new Mock<IUsersRepository>();
        var sync = new Mock<IProfileSyncService>();
        repository.Setup(item => item.GetByIdWithProfileAsync(userId))
            .ReturnsAsync(ServiceTestData.User(userId));
        repository.Setup(item => item.SaveChangesAsync())
            .ThrowsAsync(new InvalidOperationException("database failed"));
        var service = new UsersService(repository.Object, sync.Object);

        var action = () => service.UpdateMyProfileAsync(
            userId,
            new UpdateProfileRequest { Bio = "Updated bio" });

        await action.Should().ThrowAsync<InvalidOperationException>();
        sync.Verify(item => item.UpsertProfileAsync(
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SkillsService_ShouldSyncAfterSkillPersistence()
    {
        var userId = Guid.NewGuid();
        var skillId = Guid.NewGuid();
        var events = new List<string>();
        var repository = new Mock<ISkillsRepository>();
        var sync = new Mock<IProfileSyncService>();
        var skill = new Skill { Id = skillId, Name = "Python" };
        repository.Setup(item => item.GetUserByIdAsync(userId))
            .ReturnsAsync(ServiceTestData.User(userId));
        repository.Setup(item => item.GetByIdAsync(skillId)).ReturnsAsync(skill);
        repository.Setup(item => item.UserHasSkillAsync(userId, skillId))
            .ReturnsAsync(false);
        repository.Setup(item => item.SaveChangesAsync())
            .Callback(() => events.Add("database"))
            .Returns(Task.CompletedTask);
        sync.Setup(item => item.UpsertProfileAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .Callback(() => events.Add("ai"))
            .Returns(Task.CompletedTask);
        var cacheService = new Mock<ICacheService>();
        var service = new SkillsService(
            repository.Object,
            cacheService.Object,
            sync.Object);

        await service.AddMySkillAsync(userId, skillId);

        events.Should().Equal("database", "ai");
    }

    [Fact]
    public async Task InterestsService_ShouldSyncAfterInterestPersistence()
    {
        var userId = Guid.NewGuid();
        var interestId = Guid.NewGuid();
        var events = new List<string>();
        var repository = new Mock<IInterestsRepository>();
        var sync = new Mock<IProfileSyncService>();
        var interest = new Interest { Id = interestId, Name = "AI" };
        repository.Setup(item => item.GetUserByIdAsync(userId))
            .ReturnsAsync(ServiceTestData.User(userId));
        repository.Setup(item => item.GetByIdAsync(interestId)).ReturnsAsync(interest);
        repository.Setup(item => item.UserHasInterestAsync(userId, interestId))
            .ReturnsAsync(false);
        repository.Setup(item => item.SaveChangesAsync())
            .Callback(() => events.Add("database"))
            .Returns(Task.CompletedTask);
        sync.Setup(item => item.UpsertProfileAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .Callback(() => events.Add("ai"))
            .Returns(Task.CompletedTask);
        var cacheService = new Mock<ICacheService>();
        var service = new InterestsService(
            repository.Object,
            cacheService.Object,
            sync.Object);

        await service.AddMyInterestAsync(userId, interestId);

        events.Should().Equal("database", "ai");
    }

    [Fact]
    public async Task ProfileSyncClient_ShouldUseContractEndpointAndSnakeCasePayloads()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                "{\"sync_status\":\"completed\",\"profiles_indexed\":1,\"index_version\":\"v1\"}",
                Encoding.UTF8,
                "application/json")
        });
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://ai.test")
        };
        var client = new ProfileSyncClient(
            httpClient,
            Options.Create(new AiOptions { BaseUrl = httpClient.BaseAddress!.ToString() }),
            NullLogger<ProfileSyncClient>.Instance);

        await client.SyncAsync(new ProfileSyncRequest
        {
            SyncType = "upsert",
            Profiles = [new ProfileSyncItem
            {
                ProfileId = "profile-1",
                Skills = ["Python"],
                Interests = ["AI"],
                LearningDirection = "Backend",
                Bio = "Bio"
            }]
        });

        handler.Requests.Should().ContainSingle();
        handler.Requests[0].Path.Should()
            .Be("/api/v1/ai/recommendations/people/sync");
        using var document = JsonDocument.Parse(handler.Requests[0].Body);
        document.RootElement.GetProperty("sync_type").GetString()
            .Should().Be("upsert");
        document.RootElement.GetProperty("profiles")[0]
            .GetProperty("profile_id").GetString().Should().Be("profile-1");
    }

    [Fact]
    public async Task ProfileSyncClient_ShouldUseDeletePayloadWithoutUpsertFields()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                "{\"sync_status\":\"completed\",\"profiles_indexed\":0,\"index_version\":\"v1\"}",
                Encoding.UTF8,
                "application/json")
        });
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://ai.test")
        };
        var client = new ProfileSyncClient(
            httpClient,
            Options.Create(new AiOptions { BaseUrl = httpClient.BaseAddress!.ToString() }),
            NullLogger<ProfileSyncClient>.Instance);

        await client.SyncAsync(new ProfileSyncRequest
        {
            SyncType = "delete",
            ProfileIds = ["profile-1"]
        });

        using var document = JsonDocument.Parse(handler.Requests.Single().Body);
        document.RootElement.GetProperty("sync_type").GetString()
            .Should().Be("delete");
        document.RootElement.GetProperty("profile_ids")[0]
            .GetString().Should().Be("profile-1");
        document.RootElement.TryGetProperty("profiles", out _).Should().BeFalse();
    }

    private sealed class RecordingHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        : HttpMessageHandler
    {
        public List<RecordedRequest> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(new RecordedRequest(
                request.RequestUri!.AbsolutePath,
                await request.Content!.ReadAsStringAsync(cancellationToken)));
            return responseFactory(request);
        }
    }

    private sealed record RecordedRequest(string Path, string Body);
}
