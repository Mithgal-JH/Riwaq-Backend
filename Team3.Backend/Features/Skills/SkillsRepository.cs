using Microsoft.EntityFrameworkCore;
using Team3.Backend.Data;
using Team3.Backend.Features.Skills.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Features.Skills;

public class SkillsRepository : ISkillsRepository
{
    private readonly AppDbContext _context;

    public SkillsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Skill>> GetAllAsync()
    {
        return await _context.Skills
            .AsNoTracking()
            .OrderBy(skill => skill.Name)
            .ToListAsync();
    }

    public async Task<Skill?> GetByIdAsync(Guid skillId)
    {
        return await _context.Skills
            .AsNoTracking()
            .FirstOrDefaultAsync(skill => skill.Id == skillId);
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _context.Users
            .Include(user => user.UserSkills)
                .ThenInclude(userSkill => userSkill.Skill)
            .FirstOrDefaultAsync(user => user.Id == userId);
    }

    public async Task<bool> UserHasSkillAsync(Guid userId, Guid skillId)
    {
        return await _context.UserSkills
            .AnyAsync(userSkill =>
                userSkill.UserId == userId && userSkill.SkillId == skillId);
    }

    public async Task<bool> RemoveUserSkillAsync(Guid userId, Guid skillId)
    {
        var userSkill = await _context.UserSkills
            .FirstOrDefaultAsync(item =>
                item.UserId == userId && item.SkillId == skillId);

        if (userSkill is null)
        {
            return false;
        }

        _context.UserSkills.Remove(userSkill);
        return true;
    }

    public void AddUserSkill(UserSkill userSkill)
    {
        _context.UserSkills.Add(userSkill);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
