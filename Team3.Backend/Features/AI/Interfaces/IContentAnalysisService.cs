using Team3.Backend.Models;
using EducationalContentModel = Team3.Backend.Models.EducationalContent;

namespace Team3.Backend.Features.AI.Interfaces;

public interface IContentAnalysisService
{
    Task AnalyzeAsync(
        EducationalContentModel content,
        CancellationToken cancellationToken = default);
}
