using Team3.Backend.Features.Recommendations.Dtos;

namespace Team3.Backend.Features.Recommendations.Interfaces;

public interface IPersonRecommendationService
{
    Task<PersonRecommendationResponse> GetForUserAsync(
        Guid userId,
        int topN = 5,
        string? requestId = null,
        CancellationToken cancellationToken = default);
}
