using Team3.Backend.Models;

namespace Team3.Backend.Features.Users.Interfaces;

public interface IUsersRepository
{
    Task<User?> GetByFirebaseUidWithProfileAsync(string firebaseUid);

    Task<User?> GetByIdWithProfileAsync(Guid id);

    void AddProfile(Profile profile);

    Task SaveChangesAsync();
}
