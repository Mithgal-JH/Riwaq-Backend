using Microsoft.EntityFrameworkCore;
using Team3.Backend.Data;
using Team3.Backend.Features.Users.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Features.Users;

public class UsersRepository : IUsersRepository
{
    private readonly AppDbContext _context;

    public UsersRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdWithProfileAsync(Guid id)
    {
        return await _context.Users
            .Include(x => x.Profile)
            .Include(x => x.SelectedSkill)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Skill?> GetSkillByIdAsync(Guid skillId)
    {
        return await _context.Skills
            .AsNoTracking()
            .FirstOrDefaultAsync(skill => skill.Id == skillId);
    }

    public void AddProfile(Profile profile)
    {
        _context.Profiles.Add(profile);
    }

    public async Task SaveChangesAsync()
    {
        // Profile updates must not persist Identity User rows. A tracked User can be
        // marked Modified during relationship fixup; updating AspNetUsers then fails
        // when ConcurrencyStamp does not match (DbUpdateConcurrencyException).
        var profileEntries = _context.ChangeTracker.Entries<Profile>();

        if (profileEntries.Any(entry =>
                entry.State is EntityState.Added
                    or EntityState.Modified
                    or EntityState.Deleted))
        {
            foreach (var userEntry in _context.ChangeTracker.Entries<User>()
                         .Where(entry => entry.State == EntityState.Modified))
            {
                userEntry.State = EntityState.Unchanged;
            }
        }

        await _context.SaveChangesAsync();
    }
}
