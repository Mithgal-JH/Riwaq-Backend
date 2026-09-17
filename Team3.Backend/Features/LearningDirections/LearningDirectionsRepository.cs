using Microsoft.EntityFrameworkCore;
using Team3.Backend.Data;
using Team3.Backend.Features.LearningDirections.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Features.LearningDirections;

public class LearningDirectionsRepository : ILearningDirectionsRepository
{
    private readonly AppDbContext _context;

    public LearningDirectionsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<LearningDirection>> GetAllAsync()
    {
        return await _context.LearningDirections
            .AsNoTracking()
            .OrderBy(direction => direction.Name)
            .ToListAsync();
    }

    public async Task<LearningDirection?> GetByIdWithSkillsAsync(
        Guid learningDirectionId)
    {
        return await _context.LearningDirections
            .AsNoTracking()
            .Include(direction => direction.LearningDirectionSkills)
                .ThenInclude(directionSkill => directionSkill.Skill)
            .FirstOrDefaultAsync(direction => direction.Id == learningDirectionId);
    }
}
