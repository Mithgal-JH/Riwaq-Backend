using Microsoft.EntityFrameworkCore;
using Team3.Backend.Data;
using Team3.Backend.Features.Experiences.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Features.Experiences;

public class ExperiencesRepository : IExperiencesRepository
{
    private readonly AppDbContext _context;

    public ExperiencesRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == userId);
    }

    public async Task<List<Experience>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Experiences
            .AsNoTracking()
            .Where(experience => experience.UserId == userId)
            .OrderBy(experience => experience.Title)
            .ToListAsync();
    }

    public async Task<Experience?> GetByIdForUserAsync(
        Guid experienceId,
        Guid userId)
    {
        return await _context.Experiences
            .FirstOrDefaultAsync(experience =>
                experience.Id == experienceId
                && experience.UserId == userId);
    }

    public async Task<Experience?> GetByIdAsync(Guid experienceId)
    {
        return await _context.Experiences
            .AsNoTracking()
            .FirstOrDefaultAsync(experience => experience.Id == experienceId);
    }

    public void Add(Experience experience)
    {
        _context.Experiences.Add(experience);
    }

    public void Remove(Experience experience)
    {
        _context.Experiences.Remove(experience);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
