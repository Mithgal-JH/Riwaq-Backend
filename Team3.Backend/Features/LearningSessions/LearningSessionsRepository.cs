using Microsoft.EntityFrameworkCore;
using Team3.Backend.Data;
using Team3.Backend.Features.LearningSessions.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Features.LearningSessions;

public sealed class LearningSessionsRepository : ILearningSessionsRepository
{
    private readonly AppDbContext _context;

    public LearningSessionsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Connection?> GetConnectionAsync(Guid connectionId)
    {
        return await _context.Connections
            .AsNoTracking()
            .FirstOrDefaultAsync(connection => connection.Id == connectionId);
    }

    public async Task<List<LearningSession>> GetByUserIdAsync(Guid userId)
    {
        return await _context.LearningSessions
            .AsNoTracking()
            .Include(session => session.Connection)
            .Where(session =>
                session.Connection.UserAId == userId
                || session.Connection.UserBId == userId)
            .OrderBy(session => session.ScheduledAt)
            .ToListAsync();
    }

    public async Task<LearningSession?> GetByIdForUserAsync(
        Guid sessionId,
        Guid userId)
    {
        return await _context.LearningSessions
            .AsNoTracking()
            .Include(session => session.Connection)
            .FirstOrDefaultAsync(session =>
                session.Id == sessionId
                && (session.Connection.UserAId == userId
                    || session.Connection.UserBId == userId));
    }

    public async Task<LearningSession?> GetTrackedByIdForUserAsync(
        Guid sessionId,
        Guid userId)
    {
        return await _context.LearningSessions
            .Include(session => session.Connection)
            .FirstOrDefaultAsync(session =>
                session.Id == sessionId
                && (session.Connection.UserAId == userId
                    || session.Connection.UserBId == userId));
    }

    public void Add(LearningSession session)
    {
        _context.LearningSessions.Add(session);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}