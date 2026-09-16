using FirebaseAdmin.Auth;
using Microsoft.EntityFrameworkCore;
using Team3.Backend.Data;
using Team3.Backend.Models;
using Team3.Backend.Features.Authentication.Dtos;
using Team3.Backend.Features.Authentication.Interfaces;

namespace Team3.Backend.Features.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly AppDbContext _context;

    public AuthenticationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AuthResponse> LoginWithFirebaseAsync(
        FirebaseLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.IdToken))
        {
            throw new ArgumentException("Firebase ID token is required.");
        }

        var decodedToken = await FirebaseAuth.DefaultInstance
            .VerifyIdTokenAsync(request.IdToken);

        var firebaseUid = decodedToken.Uid;

        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.FirebaseUid == firebaseUid);

        var isNewUser = false;

        if (user is null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                FirebaseUid = firebaseUid,
                Points = 0,
                LearningDirectionId = null
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            isNewUser = true;
        }

        return new AuthResponse
        {
            UserId = user.Id,
            FirebaseUid = user.FirebaseUid,
            Points = user.Points,
            LearningDirectionId = user.LearningDirectionId,
            IsNewUser = isNewUser
        };
    }
}