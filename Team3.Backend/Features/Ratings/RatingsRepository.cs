using Microsoft.EntityFrameworkCore;
using Team3.Backend.Data;
using Team3.Backend.Features.Ratings.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Features.Ratings;

public sealed class RatingsRepository : IRatingsRepository
{
    private readonly AppDbContext _context;

    public RatingsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _context.Users
            .Include(user => user.Profile)
            .Include(user => user.SelectedSkill)
            .FirstOrDefaultAsync(user => user.Id == userId);
    }

    public async Task<LearningSession?> GetSessionWithConnectionAsync(
        Guid learningSessionId)
    {
        return await _context.LearningSessions
            .Include(session => session.Connection)
            .ThenInclude(connection => connection.UserA)
            .ThenInclude(user => user.Profile)
            .Include(session => session.Connection)
            .ThenInclude(connection => connection.UserB)
            .ThenInclude(user => user.Profile)
            .FirstOrDefaultAsync(session => session.Id == learningSessionId);
    }

    public async Task<bool> UserHasRatedSessionAsync(
        Guid learningSessionId,
        Guid userId)
    {
        return await _context.Ratings
            .AnyAsync(rating =>
                rating.LearningSessionId == learningSessionId &&
                rating.RaterUserId == userId);
    }

    public void Add(Rating rating)
    {
        _context.Ratings.Add(rating);
    }

    public async Task<List<Rating>> GetReceivedRatingsAsync(Guid userId)
    {
        return await _context.Ratings
            .AsNoTracking()
            .Include(rating => rating.RaterUser)
                .ThenInclude(user => user.Profile)
            .Include(rating => rating.RaterUser)
                .ThenInclude(user => user.SelectedSkill)
            .Include(rating => rating.RatedUser)
                .ThenInclude(user => user.Profile)
            .Include(rating => rating.LearningSession)
            .Where(rating => rating.RatedUserId == userId)
            .OrderByDescending(rating => rating.CreatedAt)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
