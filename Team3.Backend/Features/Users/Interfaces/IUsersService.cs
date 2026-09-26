using Team3.Backend.Features.Users.Dtos;

namespace Team3.Backend.Features.Users.Interfaces;

public interface IUsersService
{
    Task<UserProfileResponse?> GetMyProfileAsync(
        Guid userId
    );

    Task<UserProfileResponse> UpdateMyProfileAsync(
        Guid userId,
        UpdateProfileRequest request
    );

    Task<UserProfileResponse> SelectLearningDirectionAsync(
        Guid userId,
        SelectLearningDirectionRequest request
    );

    Task<PublicUserProfileResponse?> GetPublicProfileAsync(
        Guid id
    );
}
