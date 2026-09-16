using Team3.Backend.Features.Users.Dtos;

namespace Team3.Backend.Features.Users.Interfaces;

public interface IUsersService
{
    Task<UserProfileResponse?> GetMyProfileAsync(
        string firebaseUid
    );

    Task<UserProfileResponse> UpdateMyProfileAsync(
        string firebaseUid,
        UpdateProfileRequest request
    );
}