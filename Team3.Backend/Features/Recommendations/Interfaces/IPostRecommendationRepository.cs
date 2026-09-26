using Team3.Backend.Features.AI.Dtos;
using Team3.Backend.Models;
using EducationalContentModel = Team3.Backend.Models.EducationalContent;

namespace Team3.Backend.Features.Recommendations.Interfaces;

public interface IPostRecommendationRepository
{
    Task<List<Guid>> GetEligibleCandidateIdsAsync(int maximum);

    Task<List<RecentInteractionRequest>> GetRecentInteractionsAsync(
        Guid userId);

    Task<List<EducationalContentModel>> GetByIdsAsync(
        IReadOnlyCollection<Guid> contentIds);
}
