using Microsoft.EntityFrameworkCore;
using Team3.Backend.Data;
using Team3.Backend.Features.Interests.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Features.Interests;

public class InterestsRepository : IInterestsRepository
{
    private readonly AppDbContext _context;

    public InterestsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Interest>> GetAllAsync()
    {
        return await _context.Interests
            .AsNoTracking()
            .OrderBy(interest => interest.Name)
            .ToListAsync();
    }

    public async Task<Interest?> GetByIdAsync(Guid interestId)
    {
        return await _context.Interests
            .AsNoTracking()
            .FirstOrDefaultAsync(interest => interest.Id == interestId);
    }

    public async Task<User?> GetUserByFirebaseUidAsync(string firebaseUid)
    {
        return await _context.Users
            .Include(user => user.UserInterests)
                .ThenInclude(userInterest => userInterest.Interest)
            .FirstOrDefaultAsync(user => user.FirebaseUid == firebaseUid);
    }

    public async Task<bool> UserHasInterestAsync(
        Guid userId,
        Guid interestId)
    {
        return await _context.UserInterests
            .AnyAsync(userInterest =>
                userInterest.UserId == userId
                && userInterest.InterestId == interestId);
    }

    public async Task<bool> RemoveUserInterestAsync(
        Guid userId,
        Guid interestId)
    {
        var userInterest = await _context.UserInterests
            .FirstOrDefaultAsync(item =>
                item.UserId == userId
                && item.InterestId == interestId);

        if (userInterest is null)
        {
            return false;
        }

        _context.UserInterests.Remove(userInterest);
        return true;
    }

    public void AddUserInterest(UserInterest userInterest)
    {
        _context.UserInterests.Add(userInterest);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
