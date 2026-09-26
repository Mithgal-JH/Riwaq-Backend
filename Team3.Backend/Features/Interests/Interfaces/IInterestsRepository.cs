using Team3.Backend.Models;

namespace Team3.Backend.Features.Interests.Interfaces;

public interface IInterestsRepository
{
    Task<List<Interest>> GetAllAsync();

    Task<Interest?> GetByIdAsync(Guid interestId);

    Task<User?> GetUserByIdAsync(Guid userId);

    Task<bool> UserHasInterestAsync(Guid userId, Guid interestId);

    Task<bool> RemoveUserInterestAsync(Guid userId, Guid interestId);

    void AddUserInterest(UserInterest userInterest);

    Task SaveChangesAsync();
}
