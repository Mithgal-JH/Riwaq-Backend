using Microsoft.Extensions.Options;
using Team3.Backend.Features.AI.Dtos;
using Team3.Backend.Features.AI.Interfaces;

namespace Team3.Backend.Features.AI;

public sealed class PersonRecommendationClient : AiHttpClient, IPersonRecommendationClient
{
    public PersonRecommendationClient(
        HttpClient httpClient,
        IOptions<AiOptions> options,
        ILogger<PersonRecommendationClient> logger)
        : base(httpClient, options, logger)
    {
    }

    public Task<PersonRecommendationAiResponse> GetRecommendationsAsync(
        string profileId,
        int topN,
        string requestId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(profileId))
        {
            throw new ArgumentException("Profile ID is required.", nameof(profileId));
        }

        if (topN is < 1 or > 20)
        {
            throw new ArgumentOutOfRangeException(nameof(topN));
        }

        if (string.IsNullOrWhiteSpace(requestId))
        {
            throw new ArgumentException("Request ID is required.", nameof(requestId));
        }

        var path = $"/api/v1/ai/recommendations/people/"
            + $"{Uri.EscapeDataString(profileId)}"
            + $"?top_n={topN}&request_id={Uri.EscapeDataString(requestId)}";

        return GetAsync<PersonRecommendationAiResponse>(
            path,
            requestId,
            cancellationToken);
    }
}
