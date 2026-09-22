
using Microsoft.EntityFrameworkCore;
using Team3.Backend.Data;
using Team3.Backend.Features.SkillVerificationRequests.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Features.SkillVerificationRequests;

public sealed class SkillVerificationRequestsRepository : ISkillVerificationRequestsRepository
{
    private readonly AppDbContext _context;

    public SkillVerificationRequestsRepository(AppDbContext context)
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

    public async Task<Skill?> GetSkillByIdAsync(Guid skillId)
    {
        return await _context.Skills
            .AsNoTracking()
            .FirstOrDefaultAsync(skill => skill.Id == skillId);
    }

    public async Task<SkillVerificationRequest?> GetRequestByIdAsync(Guid requestId)
    {
        return await _context.SkillVerificationRequests
            .Include(request => request.RequesterUser)
                .ThenInclude(user => user.Profile)
            .Include(request => request.RequesterUser)
                .ThenInclude(user => user.SelectedSkill)
            .Include(request => request.MentorUser)
                .ThenInclude(user => user.Profile)
            .Include(request => request.MentorUser)
                .ThenInclude(user => user.SelectedSkill)
            .Include(request => request.Skill)
            .FirstOrDefaultAsync(request => request.Id == requestId);
    }

    public async Task<SkillVerificationRequest?> GetPendingRequestForSkillAsync(
        Guid requesterUserId,
        Guid mentorUserId,
        Guid skillId)
    {
        return await _context.SkillVerificationRequests
            .FirstOrDefaultAsync(request =>
                request.RequesterUserId == requesterUserId &&
                request.MentorUserId == mentorUserId &&
                request.SkillId == skillId &&
                request.Status == "Pending");
    }

    public async Task<bool> HasSharedLearningSessionAsync(
        Guid requesterUserId,
        Guid mentorUserId)
    {
        return await _context.LearningSessions
            .AnyAsync(session =>
                session.Connection.UserAId == requesterUserId &&
                session.Connection.UserBId == mentorUserId ||
                session.Connection.UserAId == mentorUserId &&
                session.Connection.UserBId == requesterUserId);
    }

    public async Task<List<SkillVerificationRequest>> GetSentAsync(Guid requesterUserId)
    {
        return await _context.SkillVerificationRequests
            .AsNoTracking()
            .Include(request => request.RequesterUser)
                .ThenInclude(user => user.Profile)
            .Include(request => request.RequesterUser)
                .ThenInclude(user => user.SelectedSkill)
            .Include(request => request.MentorUser)
                .ThenInclude(user => user.Profile)
            .Include(request => request.MentorUser)
                .ThenInclude(user => user.SelectedSkill)
            .Include(request => request.Skill)
            .Where(request => request.RequesterUserId == requesterUserId)
            .OrderByDescending(request => request.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<SkillVerificationRequest>> GetReceivedAsync(Guid mentorUserId)
    {
        return await _context.SkillVerificationRequests
            .AsNoTracking()
            .Include(request => request.RequesterUser)
                .ThenInclude(user => user.Profile)
            .Include(request => request.RequesterUser)
                .ThenInclude(user => user.SelectedSkill)
            .Include(request => request.MentorUser)
                .ThenInclude(user => user.Profile)
            .Include(request => request.MentorUser)
                .ThenInclude(user => user.SelectedSkill)
            .Include(request => request.Skill)
            .Where(request => request.MentorUserId == mentorUserId)
            .OrderByDescending(request => request.CreatedAt)
            .ToListAsync();
    }

    public void Add(SkillVerificationRequest request)
    {
        _context.SkillVerificationRequests.Add(request);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
