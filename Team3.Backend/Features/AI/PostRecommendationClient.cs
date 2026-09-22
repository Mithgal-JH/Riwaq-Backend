using Microsoft.Extensions.Options;
using Team3.Backend.Features.AI.Dtos;
using Team3.Backend.Features.AI.Interfaces;

namespace Team3.Backend.Features.AI;

public sealed class PostRecommendationClient : AiHttpClient, IPostRecommendationClient
{
    public PostRecommendationClient(
        HttpClient httpClient,
        IOptions<AiOptions> options,
        ILogger<PostRecommendationClient> logger)
        : base(httpClient, options, logger)
    {
    }

    public Task<PostRecommendationResponse> RecommendPostsAsync(
        PostRecommendationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        request.RequestId = EnsureRequestId(request.RequestId);

        return PostAsync<PostRecommendationRequest, PostRecommendationResponse>(
            "/api/v1/recommendations/posts",
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
