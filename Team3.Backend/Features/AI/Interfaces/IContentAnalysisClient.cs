using Team3.Backend.Features.AI.Dtos;

namespace Team3.Backend.Features.AI.Interfaces;

public interface IContentAnalysisClient
{
    Task<ContentAnalysisResponse> AnalyzeAsync(
        ContentAnalysisRequest request,
        CancellationToken cancellationToken = default);
}
