using Team3.Backend.Features.AI.Dtos;

namespace Team3.Backend.Features.AI.Interfaces;

public interface IPostRecommendationClient
{
    Task<PostRecommendationResponse> RecommendPostsAsync(
        PostRecommendationRequest request,
        CancellationToken cancellationToken = default);
}
