using Team3.Backend.Features.Authentication.Dtos;

namespace Team3.Backend.Features.Authentication.Interfaces;

public interface IAuthenticationService
{
    Task<AuthResponse> LoginWithFirebaseAsync(
        FirebaseLoginRequest request);
}