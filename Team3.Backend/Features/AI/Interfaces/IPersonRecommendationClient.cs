using Team3.Backend.Features.AI.Dtos;

namespace Team3.Backend.Features.AI.Interfaces;

public interface IPersonRecommendationClient
{
    Task<PersonRecommendationAiResponse> GetRecommendationsAsync(
        string profileId,
        int topN,
        string requestId,
        CancellationToken cancellationToken = default);
}
