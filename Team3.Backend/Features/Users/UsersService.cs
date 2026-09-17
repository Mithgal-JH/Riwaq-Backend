using Team3.Backend.Features.Users.Dtos;
using Team3.Backend.Features.Users.Interfaces;
using Team3.Backend.Models;

namespace Team3.Backend.Features.Users;

public class UsersService : IUsersService
{
    private readonly IUsersRepository _usersRepository;

    public UsersService(IUsersRepository usersRepository)
    {
        _usersRepository = usersRepository;
    }

    public async Task<UserProfileResponse?> GetMyProfileAsync(
        string firebaseUid)
    {
        var user = await _usersRepository
            .GetByFirebaseUidWithProfileAsync(firebaseUid);

        if (user is null)
        {
            return null;
        }

        return MapToUserProfileResponse(user);
    }

    public async Task<UserProfileResponse> UpdateMyProfileAsync(
        string firebaseUid,
        UpdateProfileRequest request)
    {
        var user = await _usersRepository
            .GetByFirebaseUidWithProfileAsync(firebaseUid);

        if (user is null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        if (user.Profile is null)
        {
            var profile = new Profile
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Bio = request.Bio,
                University = request.University,
                UpdatedAt = DateTime.UtcNow
            };

            _usersRepository.AddProfile(profile);
            user.Profile = profile;
        }
        else
        {
            user.Profile.FirstName = request.FirstName;
            user.Profile.LastName = request.LastName;
            user.Profile.Bio = request.Bio;
            user.Profile.University = request.University;
            user.Profile.UpdatedAt = DateTime.UtcNow;
        }

        await _usersRepository.SaveChangesAsync();

        return MapToUserProfileResponse(user);
    }

    public async Task<PublicUserProfileResponse?> GetPublicProfileAsync(
        Guid id)
    {
        var user = await _usersRepository.GetByIdWithProfileAsync(id);

        if (user is null)
        {
            return null;
        }

        return new PublicUserProfileResponse
        {
            UserId = user.Id,
            Points = user.Points,
            LearningDirectionId = user.LearningDirectionId,
            LearningDirectionName = user.LearningDirection?.Name,
            FirstName = user.Profile?.FirstName,
            LastName = user.Profile?.LastName,
            Bio = user.Profile?.Bio,
            University = user.Profile?.University
        };
    }

    private static UserProfileResponse MapToUserProfileResponse(User user)
    {
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
}
