using Microsoft.EntityFrameworkCore;
using Team3.Backend.Data;
using Team3.Backend.Features.AI.Dtos;
using Team3.Backend.Features.Recommendations.Interfaces;
using Team3.Backend.Models;
using EducationalContentModel = Team3.Backend.Models.EducationalContent;

namespace Team3.Backend.Features.Recommendations;

public sealed class PostRecommendationRepository : IPostRecommendationRepository
{
    private readonly AppDbContext _context;

    public PostRecommendationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Guid>> GetEligibleCandidateIdsAsync(int maximum)
    {
        return await _context.EducationalContents
            .AsNoTracking()
            .OrderByDescending(content => content.CreatedAt)
            .ThenByDescending(content => content.Id)
            .Select(content => content.Id)
            .Take(maximum)
            .ToListAsync();
    }

    public async Task<List<RecentInteractionRequest>> GetRecentInteractionsAsync(
        Guid userId)
    {
        var likes = await _context.Likes
            .AsNoTracking()
            .Where(like => like.UserId == userId)
            .Select(like => new RecentInteractionRequest
            {
                PostId = like.EducationalContentId.ToString(),
                InteractionType = "like",
                Timestamp = new DateTimeOffset(like.CreatedAt)
            })
            .ToListAsync();

        var saves = await _context.Saves
            .AsNoTracking()
            .Where(save => save.UserId == userId)
            .Select(save => new RecentInteractionRequest
            {
                PostId = save.EducationalContentId.ToString(),
                InteractionType = "save",
                Timestamp = new DateTimeOffset(save.CreatedAt)
            })
            .ToListAsync();

        var reposts = await _context.Reposts
            .AsNoTracking()
            .Where(repost => repost.UserId == userId)
            .Select(repost => new RecentInteractionRequest
            {
                PostId = repost.EducationalContentId.ToString(),
                InteractionType = "repost",
                Timestamp = new DateTimeOffset(repost.CreatedAt)
            })
            .ToListAsync();

        return likes
            .Concat(saves)
            .Concat(reposts)
            .OrderByDescending(interaction => interaction.Timestamp)
            .ToList();
    }

    public async Task<List<EducationalContentModel>> GetByIdsAsync(
        IReadOnlyCollection<Guid> contentIds)
    {
        if (contentIds.Count == 0)
        {
            return [];
        }

        return await _context.EducationalContents
            .AsNoTracking()
            .Where(content => contentIds.Contains(content.Id))
            .ToListAsync();
    }
}
