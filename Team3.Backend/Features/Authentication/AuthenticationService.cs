using Team3.Backend.Features.Authentication.Dtos;
using Team3.Backend.Features.Authentication.Interfaces;

namespace Team3.Backend.Features.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly FirebaseAuthenticationService _firebaseAuthenticationService;

    public AuthenticationService(
        FirebaseAuthenticationService firebaseAuthenticationService)
    {
        _firebaseAuthenticationService = firebaseAuthenticationService;
    }

    public async Task<AuthResponse> LoginWithFirebaseAsync(
        FirebaseLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.IdToken))
        {
            throw new ArgumentException("Firebase ID token is required.");
        }

        var authResult = await _firebaseAuthenticationService
            .GetOrCreateUserAsync(request.IdToken);

        var user = authResult.User;

        return new AuthResponse
        {
            UserId = user.Id,
            FirebaseUid = user.FirebaseUid,
            Points = user.Points,
            LearningDirectionId = user.LearningDirectionId,
            IsNewUser = authResult.IsNewUser
        };
    }
}
