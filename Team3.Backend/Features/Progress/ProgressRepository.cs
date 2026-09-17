using Microsoft.EntityFrameworkCore;
using Team3.Backend.Data;
using Team3.Backend.Features.Progress.Interfaces;
using Team3.Backend.Models;
using ProgressEntity = Team3.Backend.Models.Progress;

namespace Team3.Backend.Features.Progress;

public class ProgressRepository : IProgressRepository
{
    private readonly AppDbContext _context;

    public ProgressRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByFirebaseUidAsync(string firebaseUid)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.FirebaseUid == firebaseUid);
    }

    public async Task<List<ProgressEntity>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Progresses
            .AsNoTracking()
            .Include(progress => progress.LearningDirection)
            .Where(progress => progress.UserId == userId)
            .OrderBy(progress => progress.UpdatedAt)
            .ToListAsync();
    }

    public async Task<ProgressEntity?> GetByIdForUserAsync(
        Guid progressId,
        Guid userId)
    {
        return await _context.Progresses
            .AsNoTracking()
            .Include(progress => progress.LearningDirection)
            .FirstOrDefaultAsync(progress =>
                progress.Id == progressId
                && progress.UserId == userId);
    }

    public async Task<ProgressEntity?> GetTrackedByIdForUserAsync(
        Guid progressId,
        Guid userId)
    {
        return await _context.Progresses
            .FirstOrDefaultAsync(progress =>
                progress.Id == progressId
                && progress.UserId == userId);
    }

    public async Task<LearningDirection?> GetLearningDirectionByIdAsync(
        Guid learningDirectionId)
    {
        return await _context.LearningDirections
            .AsNoTracking()
            .FirstOrDefaultAsync(direction => direction.Id == learningDirectionId);
    }

    public async Task<bool> ExistsForUserAsync(
        Guid userId,
        Guid learningDirectionId)
    {
        return await _context.Progresses
            .AnyAsync(progress =>
                progress.UserId == userId
                && progress.LearningDirectionId == learningDirectionId);
    }

    public void Add(ProgressEntity progress)
    {
        _context.Progresses.Add(progress);
    }

    public void Remove(ProgressEntity progress)
    {
        _context.Progresses.Remove(progress);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
