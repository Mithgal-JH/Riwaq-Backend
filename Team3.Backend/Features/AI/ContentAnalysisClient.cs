using Microsoft.Extensions.Options;
using Team3.Backend.Features.AI.Dtos;
using Team3.Backend.Features.AI.Interfaces;

namespace Team3.Backend.Features.AI;

public sealed class ContentAnalysisClient : AiHttpClient, IContentAnalysisClient
{
    public ContentAnalysisClient(
        HttpClient httpClient,
        IOptions<AiOptions> options,
        ILogger<ContentAnalysisClient> logger)
        : base(httpClient, options, logger)
    {
    }

    public Task<ContentAnalysisResponse> AnalyzeAsync(
        ContentAnalysisRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        request.RequestId = EnsureRequestId(request.RequestId);

        return PostAsync<ContentAnalysisRequest, ContentAnalysisResponse>(
            "/api/v1/ai/content/analyze",
            request,
            request.RequestId,
            cancellationToken);
    }

    private static string EnsureRequestId(string requestId)
    {
        return string.IsNullOrWhiteSpace(requestId)
            ? $"req_{Guid.NewGuid():N}"
            : requestId;
    }
}
