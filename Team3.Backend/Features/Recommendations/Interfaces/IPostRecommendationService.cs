using Team3.Backend.Features.Recommendations.Dtos;

namespace Team3.Backend.Features.Recommendations.Interfaces;

public interface IPostRecommendationService
{
    Task<PostRecommendationResponse> RecommendAsync(
        Guid userId,
        int limit = 10,
        string? requestId = null,
        CancellationToken cancellationToken = default);
}
