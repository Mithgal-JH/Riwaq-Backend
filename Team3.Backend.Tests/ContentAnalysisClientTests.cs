using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Team3.Backend.Features.AI;
using Team3.Backend.Features.AI.Dtos;

namespace Team3.Backend.Tests;

public sealed class ContentAnalysisClientTests
{
    [Fact]
    public async Task Client_ShouldNotRetryUnprocessableText()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(
            HttpStatusCode.UnprocessableEntity)
        {
            Content = new StringContent("invalid text")
        });
        var client = CreateClient(handler);

        var action = () => client.AnalyzeAsync(new ContentAnalysisRequest
        {
            ContentId = "content-1",
            ContentVersion = 1,
            Text = "text",
            Language = "en"
        });

        var exception = await action.Should().ThrowAsync<AiServiceException>();

        exception.Which.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        handler.Requests.Should().ContainSingle();
    }

    [Fact]
    public async Task Client_ShouldRetryServerErrorWithTheSameRequestId()
    {
        var attempts = 0;
        var handler = new RecordingHandler(_ => attempts++ < 2
            ? new HttpResponseMessage(HttpStatusCode.InternalServerError)
            : JsonResponse(
                "{\"request_id\":\"req_response\",\"content_id\":\"content-1\",\"content_version\":1,\"processing_status\":\"completed\"}"));
        var client = CreateClient(handler);

        var response = await client.AnalyzeAsync(new ContentAnalysisRequest
        {
            ContentId = "content-1",
            ContentVersion = 1,
            Text = "text",
            Language = "en"
        });

        response.ProcessingStatus.Should().Be("completed");
        handler.Requests.Should().HaveCount(3);
        handler.Requests.Select(request => request.Body)
            .Select(ExtractRequestId)
            .Distinct()
            .Should().ContainSingle();
    }

    private static ContentAnalysisClient CreateClient(RecordingHandler handler)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://ai.test")
        };

        return new ContentAnalysisClient(
            httpClient,
            Options.Create(new AiOptions
            {
                BaseUrl = httpClient.BaseAddress!.ToString()
            }),
            NullLogger<ContentAnalysisClient>.Instance);
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
        const string marker = "\"request_id\":\"";
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
