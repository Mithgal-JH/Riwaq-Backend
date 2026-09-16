using Microsoft.EntityFrameworkCore;
using Team3.Backend.Data;
using Team3.Backend.Features.Users.Dtos;
using Team3.Backend.Features.Users.Interfaces;

namespace Team3.Backend.Features.Users;

public class UsersService : IUsersService
{
    private readonly AppDbContext _context;

    public UsersService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfileResponse?> GetMyProfileAsync(
        string firebaseUid)
    {
        var user = await _context.Users
            .Include(x => x.Profile)
            .FirstOrDefaultAsync(x => x.FirebaseUid == firebaseUid);

        if (user is null)
        {
            return null;
        }

        return new UserProfileResponse
        {
            UserId = user.Id,
            FirebaseUid = user.FirebaseUid,
            Points = user.Points,
            LearningDirectionId = user.LearningDirectionId,

            FirstName = user.Profile?.FirstName,
            LastName = user.Profile?.LastName,
            Bio = user.Profile?.Bio,
            University = user.Profile?.University
        };
    }

    public async Task<UserProfileResponse> UpdateMyProfileAsync(
        string firebaseUid,
        UpdateProfileRequest request)
    {
        var user = await _context.Users
            .Include(x => x.Profile)
            .FirstOrDefaultAsync(x => x.FirebaseUid == firebaseUid);

        if (user is null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        if (user.Profile is null)
        {
            user.Profile = new Models.Profile
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                UpdatedAt = DateTime.UtcNow
            };
        }

        user.Profile.FirstName = request.FirstName;
        user.Profile.LastName = request.LastName;
        user.Profile.Bio = request.Bio;
        user.Profile.University = request.University;
        user.Profile.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new UserProfileResponse
        {
            UserId = user.Id,
            FirebaseUid = user.FirebaseUid,
            Points = user.Points,
            LearningDirectionId = user.LearningDirectionId,

            FirstName = user.Profile.FirstName,
            LastName = user.Profile.LastName,
            Bio = user.Profile.Bio,
            University = user.Profile.University
        };
    }
}