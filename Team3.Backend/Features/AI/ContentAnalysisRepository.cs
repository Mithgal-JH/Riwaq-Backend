using Microsoft.EntityFrameworkCore;
using Team3.Backend.Data;
using Team3.Backend.Features.AI.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Features.AI;

public sealed class ContentAnalysisRepository : IContentAnalysisRepository
{
    private readonly AppDbContext _context;

    public ContentAnalysisRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ContentAnalysis?> GetByContentVersionAsync(
        Guid contentId,
        int contentVersion)
    {
        return await _context.ContentAnalyses
            .FirstOrDefaultAsync(analysis =>
                analysis.EducationalContentId == contentId
                && analysis.ContentVersion == contentVersion);
    }

    public void Add(ContentAnalysis analysis)
    {
        _context.ContentAnalyses.Add(analysis);
    }

    public async Task<bool> SaveIfCurrentVersionAsync(ContentAnalysis analysis)
    {
        if (!_context.Database.IsRelational())
        {
            return await SaveIfCurrentVersionWithoutLockAsync(analysis);
        }

        await using var transaction = await _context.Database
            .BeginTransactionAsync();

        var currentVersion = await _context.EducationalContents
            .FromSqlInterpolated($"SELECT * FROM \"EducationalContents\" WHERE \"Id\" = {analysis.EducationalContentId} FOR UPDATE")
            .AsNoTracking()
            .Select(content => (int?)content.ContentVersion)
            .FirstOrDefaultAsync();

        if (currentVersion != analysis.ContentVersion)
        {
            await transaction.RollbackAsync();
            return false;
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        return true;
    }

    private async Task<bool> SaveIfCurrentVersionWithoutLockAsync(
        ContentAnalysis analysis)
    {
        var currentVersion = await _context.EducationalContents
            .AsNoTracking()
            .Where(content => content.Id == analysis.EducationalContentId)
            .Select(content => (int?)content.ContentVersion)
            .FirstOrDefaultAsync();

        if (currentVersion != analysis.ContentVersion)
        {
            return false;
        }

        await _context.SaveChangesAsync();
        return true;
    }
}
