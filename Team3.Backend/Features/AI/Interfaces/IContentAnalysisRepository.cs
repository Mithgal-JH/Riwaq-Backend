using Team3.Backend.Models;

namespace Team3.Backend.Features.AI.Interfaces;

public interface IContentAnalysisRepository
{
    Task<ContentAnalysis?> GetByContentVersionAsync(
        Guid contentId,
        int contentVersion);

    void Add(ContentAnalysis analysis);

    Task<bool> SaveIfCurrentVersionAsync(ContentAnalysis analysis);
}
