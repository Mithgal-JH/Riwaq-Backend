using Microsoft.EntityFrameworkCore;
using Team3.Backend.Data;
using Team3.Backend.Features.EducationalContent.Interfaces;
using Team3.Backend.Models;
using EducationalContentModel = Team3.Backend.Models.EducationalContent;

namespace Team3.Backend.Features.EducationalContent;

public class EducationalContentRepository : IEducationalContentRepository
{
    private readonly AppDbContext _context;

    public EducationalContentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByFirebaseUidAsync(string firebaseUid)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.FirebaseUid == firebaseUid);
    }

    public async Task<List<EducationalContentModel>> GetAllAsync()
    {
        return await _context.EducationalContents
            .AsNoTracking()
            .OrderByDescending(content => content.CreatedAt)
            .ToListAsync();
    }

    public async Task<EducationalContentModel?> GetByIdAsync(Guid contentId)
    {
        return await _context.EducationalContents
            .AsNoTracking()
            .FirstOrDefaultAsync(content => content.Id == contentId);
    }

    public async Task<EducationalContentModel?> GetByIdForUserAsync(
        Guid contentId,
        Guid userId)
    {
        return await _context.EducationalContents
            .FirstOrDefaultAsync(content =>
                content.Id == contentId
                && content.UserId == userId);
    }

    public void Add(EducationalContentModel content)
    {
        _context.EducationalContents.Add(content);
    }

    public void Remove(EducationalContentModel content)
    {
        _context.EducationalContents.Remove(content);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}