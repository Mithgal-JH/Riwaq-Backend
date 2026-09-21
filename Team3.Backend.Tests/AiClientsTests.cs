using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Team3.Backend.Features.AI;
using Team3.Backend.Features.AI.Dtos;

namespace Team3.Backend.Tests;

public sealed class AiClientsTests
{
    [Fact]
    public async Task ContentAnalysis_ShouldPostContractPayloadAndGenerateRequestId()
    {
        var handler = new RecordingHandler(_ => JsonResponse(
            """
            {
              "request_id": "req_response",
              "content_id": "post_789",
              "content_version": 1,
              "processing_status": "completed",
              "processed_at": "2026-09-20T10:30:00Z",
              "topics": { "classification_status": "classified", "primary_topics": [], "secondary_topics": [], "topic_count": 0 },
              "difficulty": { "level": "INTERMEDIATE", "confidence": 0.9132 },
              "safety": { "status": "SAFE", "risk_categories": [], "review_required": false, "recommendation_signal": "ALLOW" },
              "needs_review": false,
              "model_versions": { "topic_model": "topic-v1", "difficulty_model": "difficulty-v1", "safety_model": "safety-v1" },
              "preprocessing_version": "preprocess-v1"
            }
            """));
        using var httpClient = CreateHttpClient(handler);
        var client = new ContentAnalysisClient(
            httpClient,
            Options.Create(new AiOptions { BaseUrl = httpClient.BaseAddress!.ToString() }),
            NullLogger<ContentAnalysisClient>.Instance);

        var response = await client.AnalyzeAsync(new ContentAnalysisRequest
        {
            ContentId = "post_789",
            ContentVersion = 1,
            Text = "Build a machine learning web application using Python.",
            Language = "en"
        });

        response.ProcessingStatus.Should().Be("completed");
        handler.Requests.Should().ContainSingle();
        handler.Requests[0].Path.Should().Be("/api/v1/ai/content-analysis");
        handler.Requests[0].Body.Should().Contain("\"request_id\":\"req_");
        handler.Requests[0].Body.Should().Contain("\"content_id\":\"post_789\"");
    }

    [Fact]
    public async Task PostRecommendation_ShouldReuseRequestIdWhenRetryingServerError()
    {
        var serverErrorCount = 0;
        var handler = new RecordingHandler(_ => serverErrorCount++ < 2
            ? new HttpResponseMessage(HttpStatusCode.InternalServerError)
            : JsonResponse("""{ "request_id": "req_same", "model_version": "model-v1", "count": 0, "items": [] }"""));
        using var httpClient = CreateHttpClient(handler);
        var client = new PostRecommendationClient(
            httpClient,
            Options.Create(new AiOptions { BaseUrl = httpClient.BaseAddress!.ToString() }),
            NullLogger<PostRecommendationClient>.Instance);
        var request = new PostRecommendationRequest
        {
            UserId = "usr_learner_42",
            DeclaredTopics = ["AI/Data"],
            LearningDirection = "Backend Cloud Architecture",
            EligibleCandidateIds = ["pst_101"]
        };

        var response = await client.RecommendPostsAsync(request);

        response.RequestId.Should().Be("req_same");
        handler.Requests.Should().HaveCount(3);
        handler.Requests.Select(item => item.Body)
            .Should().OnlyContain(body => body.Contains("\"request_id\":\""));
        handler.Requests.Select(item => ExtractRequestId(item.Body))
            .Distinct().Should().ContainSingle();
    }

    [Fact]
    public async Task ContentAnalysis_ShouldNotRetryMalformedRequest()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(
            HttpStatusCode.BadRequest)
        {
            Content = new StringContent("malformed")
        });
        using var httpClient = CreateHttpClient(handler);
        var client = new ContentAnalysisClient(
            httpClient,
            Options.Create(new AiOptions { BaseUrl = httpClient.BaseAddress!.ToString() }),
            NullLogger<ContentAnalysisClient>.Instance);

        var action = () => client.AnalyzeAsync(new ContentAnalysisRequest
        {
            ContentId = "post_789",
            ContentVersion = 1,
            Text = "text",
            Language = "en"
        });

        var exception = await action.Should().ThrowAsync<AiServiceException>();

        exception.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        handler.Requests.Should().ContainSingle();
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

    private static string ExtractRequestId(string body)
    {
        var marker = "\"request_id\":\"";
        var start = body.IndexOf(marker, StringComparison.Ordinal) + marker.Length;
        return body[start..body.IndexOf('"', start)];
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
