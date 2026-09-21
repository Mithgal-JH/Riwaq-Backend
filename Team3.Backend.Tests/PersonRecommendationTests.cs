using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Team3.Backend.Features.AI;
using Team3.Backend.Features.AI.Dtos;
using Team3.Backend.Features.AI.Interfaces;
using Team3.Backend.Features.Authentication.Interfaces;
using Team3.Backend.Features.Recommendations;
using Team3.Backend.Features.Recommendations.Dtos;
using Team3.Backend.Features.Recommendations.Interfaces;
using Team3.Backend.Features.Users.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Tests;

public sealed class PersonRecommendationTests
{
    [Fact]
    public async Task Client_ShouldSendExactGetContractAndDeserializeSnakeCaseResponse()
    {
        var handler = new RecordingHandler(_ => JsonResponse(
            """
            {
              "request_id": "req_123",
              "profile_id": "profile_1",
              "processing_status": "completed",
              "processed_at": "2026-09-20T10:30:00Z",
              "recommendations": [{
                "candidate_profile_id": "profile_2",
                "similarity_score": 0.6069,
                "shared_skills": ["Java"],
                "shared_interests": [],
                "same_learning_direction": false
              }],
              "recommendation_count": 1,
              "low_confidence": false,
              "model_version": "model-v1",
              "preprocessing_version": "preprocess-v1"
            }
            """));
        using var httpClient = CreateHttpClient(handler);
        var client = CreateClient(httpClient);

        var response = await client.GetRecommendationsAsync(
            "profile_1",
            5,
            "req_123");

        response.Recommendations.Should().ContainSingle();
        response.Recommendations[0].CandidateProfileId.Should().Be("profile_2");
        handler.Requests.Should().ContainSingle();
        handler.Requests[0].Method.Should().Be(HttpMethod.Get);
        handler.Requests[0].PathAndQuery.Should()
            .Be("/api/v1/ai/recommendations/people/profile_1?top_n=5&request_id=req_123");
    }

    [Fact]
    public async Task Client_ShouldNotRetryAiProfileNotFound()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(
            HttpStatusCode.NotFound)
        {
            Content = new StringContent("not indexed")
        });
        using var httpClient = CreateHttpClient(handler);
        var client = CreateClient(httpClient);

        var action = () => client.GetRecommendationsAsync(
            "profile_1",
            5,
            "req_123");

        var exception = await action.Should().ThrowAsync<AiServiceException>();

        exception.Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
        handler.Requests.Should().ContainSingle();
    }

    [Fact]
    public async Task Client_ShouldRetryServerErrorWithTheSameRequest()
    {
        var attempts = 0;
        var handler = new RecordingHandler(_ => attempts++ < 2
            ? new HttpResponseMessage(HttpStatusCode.InternalServerError)
            : JsonResponse("""{ "request_id": "req_123", "profile_id": "profile_1", "processing_status": "completed", "recommendations": [], "recommendation_count": 0, "low_confidence": true, "model_version": "v1", "preprocessing_version": "v1" }"""));
        using var httpClient = CreateHttpClient(handler);
        var client = CreateClient(httpClient);

        var response = await client.GetRecommendationsAsync(
            "profile_1",
            5,
            "req_123");

        response.LowConfidence.Should().BeTrue();
        handler.Requests.Should().HaveCount(3);
        handler.Requests.Select(request => request.PathAndQuery)
            .Distinct().Should().ContainSingle();
    }

    [Fact]
    public async Task Service_ShouldUseLocalUserIdHydrateCandidatesAndPreserveAiOrder()
    {
        var userId = Guid.NewGuid();
        var firstCandidateId = Guid.NewGuid();
        var secondCandidateId = Guid.NewGuid();
        var missingCandidateId = Guid.NewGuid();
        var usersRepository = new Mock<IUsersRepository>();
        var client = new Mock<IPersonRecommendationClient>();
        usersRepository.Setup(item => item.GetByIdWithProfileAsync(userId))
            .ReturnsAsync(ServiceTestData.User(userId));
        usersRepository.Setup(item => item.GetPublicProfilesByIdsAsync(
                It.IsAny<IReadOnlyCollection<Guid>>()))
            .ReturnsAsync(new List<User>
            {
                User(firstCandidateId, "First"),
                User(secondCandidateId, "Second")
            });
        client.Setup(item => item.GetRecommendationsAsync(
                userId.ToString(),
                5,
                "req_123",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PersonRecommendationAiResponse
            {
                RequestId = "req_123",
                ProfileId = userId.ToString(),
                ProcessingStatus = "completed",
                Recommendations =
                [
                    Recommendation(secondCandidateId, 0.9m),
                    Recommendation(firstCandidateId, 0.8m),
                    Recommendation(userId, 1.0m),
                    Recommendation("not-a-guid", 0.7m),
                    Recommendation(missingCandidateId, 0.6m)
                ],
                RecommendationCount = 5
            });
        var service = new PersonRecommendationService(
            usersRepository.Object,
            client.Object);

        var response = await service.GetForUserAsync(userId, 5, "req_123");

        response.Recommendations.Select(item => item.Profile.UserId)
            .Should().Equal(secondCandidateId, firstCandidateId);
        response.Recommendations.Select(item => item.Rank)
            .Should().Equal(1, 2);
        usersRepository.Verify(item => item.GetPublicProfilesByIdsAsync(
            It.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 3
                && ids.Contains(firstCandidateId)
                && ids.Contains(secondCandidateId)
                && ids.Contains(missingCandidateId))), Times.Once);
    }

    [Fact]
    public async Task Service_ShouldReturnLowConfidenceAndEmptyRecommendationsAsSuccess()
    {
        var userId = Guid.NewGuid();
        var usersRepository = new Mock<IUsersRepository>();
        var client = new Mock<IPersonRecommendationClient>();
        usersRepository.Setup(item => item.GetByIdWithProfileAsync(userId))
            .ReturnsAsync(ServiceTestData.User(userId));
        client.Setup(item => item.GetRecommendationsAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PersonRecommendationAiResponse
            {
                RequestId = "req_123",
                ProfileId = userId.ToString(),
                ProcessingStatus = "completed",
                LowConfidence = true
            });
        var service = new PersonRecommendationService(
            usersRepository.Object,
            client.Object);

        var response = await service.GetForUserAsync(userId);

        response.LowConfidence.Should().BeTrue();
        response.Recommendations.Should().BeEmpty();
        usersRepository.Verify(item => item.GetPublicProfilesByIdsAsync(
            It.IsAny<IReadOnlyCollection<Guid>>()), Times.Never);
    }

    [Fact]
    public async Task Service_ShouldRejectInvalidTopN()
    {
        var service = new PersonRecommendationService(
            Mock.Of<IUsersRepository>(),
            Mock.Of<IPersonRecommendationClient>());

        var action = () => service.GetForUserAsync(Guid.NewGuid(), 21);

        await action.Should().ThrowAsync<ArgumentException>()
            .WithMessage("topN must be between 1 and 20.");
    }

    [Fact]
    public async Task Controller_ShouldUseCurrentUserAndReturnBackendResponse()
    {
        var userId = Guid.NewGuid();
        var service = new Mock<IPersonRecommendationService>();
        service.Setup(item => item.GetForUserAsync(
                userId,
                5,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PersonRecommendationResponse
            {
                ProfileId = userId.ToString(),
                ProcessingStatus = "completed"
            });
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(item => item.UserId).Returns(userId);
        var controller = new RecommendationsController(
            service.Object,
            currentUser.Object);

        var result = await controller.GetPeople();

        result.Result.Should().BeOfType<OkObjectResult>();
        service.Verify(item => item.GetForUserAsync(
            userId,
            5,
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Controller_ShouldHideAiFailureDetails()
    {
        var userId = Guid.NewGuid();
        var service = new Mock<IPersonRecommendationService>();
        service.Setup(item => item.GetForUserAsync(
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AiServiceException(
                "internal detail",
                "operation",
                HttpStatusCode.InternalServerError,
                "secret response"));
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(item => item.UserId).Returns(userId);
        var controller = new RecommendationsController(
            service.Object,
            currentUser.Object);

        var result = await controller.GetPeople();
        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;

        objectResult.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
        objectResult.Value!.ToString().Should().NotContain("secret response");
        objectResult.Value!.ToString().Should().NotContain("internal detail");
    }

    [Fact]
    public async Task Controller_ShouldReturnBadRequestForInvalidTopN()
    {
        var userId = Guid.NewGuid();
        var service = new Mock<IPersonRecommendationService>();
        service.Setup(item => item.GetForUserAsync(
                userId,
                21,
                null,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("topN must be between 1 and 20."));
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(item => item.UserId).Returns(userId);
        var controller = new RecommendationsController(
            service.Object,
            currentUser.Object);

        var result = await controller.GetPeople(21);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    private static PersonRecommendationAiItem Recommendation(
        Guid profileId,
        decimal score)
    {
        return Recommendation(profileId.ToString(), score);
    }

    private static PersonRecommendationAiItem Recommendation(
        string profileId,
        decimal score)
    {
        return new PersonRecommendationAiItem
        {
            CandidateProfileId = profileId,
            SimilarityScore = score
        };
    }

    private static User User(Guid id, string firstName)
    {
        var user = ServiceTestData.User(id);
        user.Profile = new Profile
        {
            UserId = id,
            FirstName = firstName
        };
        return user;
    }

    private static PersonRecommendationClient CreateClient(HttpClient httpClient)
    {
        return new PersonRecommendationClient(
            httpClient,
            Options.Create(new AiOptions
            {
                BaseUrl = httpClient.BaseAddress!.ToString()
            }),
            NullLogger<PersonRecommendationClient>.Instance);
    }

    private static HttpClient CreateHttpClient(RecordingHandler handler)
    {
        return new HttpClient(handler)
        {
            BaseAddress = new Uri("https://ai.test")
        };
    }

    private static HttpResponseMessage JsonResponse(string body)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
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
                request.Method,
                request.RequestUri!.PathAndQuery));
            return responseFactory(request);
        }
    }

    private sealed record RecordedRequest(HttpMethod Method, string PathAndQuery);
}
